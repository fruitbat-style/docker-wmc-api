import { useState, useEffect, useCallback, useRef } from 'react';
import SidePanel from './components/SidePanel';
import MapCanvas from './components/MapCanvas';
import { fetchLocations, fetchFilters, geocodeAddress } from './api';
import type { Filters, FiltersResponse, Location, MetroArea } from './types';

const DEFAULT_CENTER: [number, number] = [47.6062, -122.3321]; // Seattle

export const METRO_AREAS: MetroArea[] = [
  { name: 'Seattle, WA', coords: [47.6062, -122.3321] },
  { name: 'Portland, OR', coords: [45.5152, -122.6784] },
  { name: 'Berkeley, CA', coords: [37.8716, -122.2727] },
  { name: 'Santa Cruz, CA', coords: [36.9741, -122.0308] },
  { name: 'San Francisco, CA', coords: [37.7749, -122.4194] },
];

const DEFAULT_FILTERS: Filters = {
  locationMethod: 'metro',
  address: '',
  metro: METRO_AREAS[0].name,
  distance: 5,
  flavors: [],
  productTypes: [],
};

export default function App() {
  const [filters, setFilters] = useState<Filters>(DEFAULT_FILTERS);
  const [locations, setLocations] = useState<Location[]>([]);
  const [center, setCenter] = useState<[number, number]>(DEFAULT_CENTER);
  const [loading, setLoading] = useState(false);
  const [panelOpen, setPanelOpen] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [availableFilters, setAvailableFilters] = useState<FiltersResponse | null>(null);
  const [pendingSearch, setPendingSearch] = useState(false);
  const centerRef = useRef(center);
  centerRef.current = center;
  const isInitialLoad = useRef(true);

  // Fetch available filters from API on mount
  useEffect(() => {
    fetchFilters()
      .then((data) => {
        setAvailableFilters(data);
        setFilters((prev) => ({
          ...prev,
          flavors: data.flavors.map((f) => f.name),
          productTypes: data.product_types.map((p) => p.name),
        }));
      })
      .catch((err) => console.error('Failed to fetch filters:', err));
  }, []);

  const doSearch = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      let lat = centerRef.current[0];
      let lng = centerRef.current[1];

      if (filters.locationMethod === 'gps') {
        try {
          const pos = await new Promise<GeolocationPosition>((resolve, reject) =>
            navigator.geolocation.getCurrentPosition(resolve, reject, { timeout: 5000 })
          );
          lat = pos.coords.latitude;
          lng = pos.coords.longitude;
          setCenter([lat, lng]);
        } catch {
          if (!isInitialLoad.current) {
            setError('Could not get your location. Using default location.');
          }
        }
      } else if (filters.locationMethod === 'address' && filters.address.trim()) {
        const result = await geocodeAddress(filters.address);
        if (result) {
          lat = result.lat;
          lng = result.lng;
          setCenter([lat, lng]);
        } else {
          setError('Could not find that address. Try a different one.');
          setLoading(false);
          return;
        }
      } else if (filters.locationMethod === 'metro') {
        const metro = METRO_AREAS.find((m) => m.name === filters.metro);
        if (metro) {
          [lat, lng] = metro.coords;
          setCenter(metro.coords);
        }
      }

      // Convert selected filter names to IDs; pass nothing if all are selected (no filter needed)
      let flavorIds: number[] | undefined;
      let productIds: number[] | undefined;
      if (availableFilters) {
        if (filters.flavors.length > 0 && filters.flavors.length < availableFilters.flavors.length) {
          flavorIds = availableFilters.flavors
            .filter((f) => filters.flavors.includes(f.name))
            .map((f) => f.id);
        }
        if (filters.productTypes.length > 0 && filters.productTypes.length < availableFilters.product_types.length) {
          productIds = availableFilters.product_types
            .filter((p) => filters.productTypes.includes(p.name))
            .map((p) => p.id);
        }
      }

      const data = await fetchLocations({
        lat,
        lng,
        radius: filters.distance || undefined,
        flavors: flavorIds,
        products: productIds,
      });
      setLocations(data);
    } catch {
      setError('Failed to load locations. Please try again.');
    } finally {
      setLoading(false);
      isInitialLoad.current = false;
    }
  }, [filters, availableFilters]);

  // Initial load
  useEffect(() => {
    doSearch();
  }, []); // eslint-disable-line react-hooks/exhaustive-deps

  // Run search after filters state has updated
  useEffect(() => {
    if (pendingSearch) {
      setPendingSearch(false);
      doSearch();
      setPanelOpen(false);
    }
  }, [pendingSearch, doSearch]);

  // Auto-dismiss error after 5 seconds
  useEffect(() => {
    if (!error) return;
    const timer = setTimeout(() => setError(null), 5000);
    return () => clearTimeout(timer);
  }, [error]);

  return (
    <div className="relative h-[100dvh] w-screen overflow-hidden bg-[#fdf8f5]">
      <SidePanel
        filters={filters}
        onFiltersChange={setFilters}
        onSearch={() => { doSearch(); setPanelOpen(false); }}
        onSearchAfterUpdate={() => setPendingSearch(true)}
        open={panelOpen}
        onToggle={() => setPanelOpen((v) => !v)}
        metroAreas={METRO_AREAS}
        availableFilters={availableFilters}
      />
      <MapCanvas
        locations={locations}
        center={center}
      />
      {loading && (
        <div className="absolute inset-0 z-[1001] bg-black/40 flex items-center justify-center">
          <div className="bg-[#4c2c5a] text-white font-['Roboto'] text-lg px-8 py-4 rounded-xl shadow-lg">
            Searching...
          </div>
        </div>
      )}
      {error && (
        <div className="absolute bottom-6 left-1/2 -translate-x-1/2 z-[1001] bg-[#d32f2f] text-white font-['Roboto'] text-sm px-5 py-3 rounded-lg shadow-lg flex items-center gap-3">
          <span>{error}</span>
          <button onClick={() => setError(null)} className="text-white/80 hover:text-white cursor-pointer bg-transparent border-none text-lg leading-none">×</button>
        </div>
      )}
    </div>
  );
}
