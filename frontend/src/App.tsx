import { useState, useEffect, useCallback } from 'react';
import SidePanel from './components/SidePanel';
import MapCanvas from './components/MapCanvas';
import { fetchLocations, geocodeAddress } from './api';
import type { Filters, Location } from './types';

const DEFAULT_CENTER: [number, number] = [47.6062, -122.3321]; // Seattle

const DEFAULT_FILTERS: Filters = {
  locationMethod: 'gps',
  address: '',
  metro: 'Seattle, WA',
  distance: 5,
  flavors: ['Original Spicy', 'Herbal (no caffeine)', 'Green', 'Lemon', 'Hibiscus'],
  productTypes: ['Chai Served here'],
};

const METRO_COORDS: Record<string, [number, number]> = {
  'Seattle, WA': [47.6062, -122.3321],
  'San Francisco, CA': [37.7749, -122.4194],
  'Portland, OR': [45.5152, -122.6784],
  'Los Angeles, CA': [34.0522, -118.2437],
};

export default function App() {
  const [filters, setFilters] = useState<Filters>(DEFAULT_FILTERS);
  const [locations, setLocations] = useState<Location[]>([]);
  const [center, setCenter] = useState<[number, number]>(DEFAULT_CENTER);
  const [searchQuery, setSearchQuery] = useState('');
  const [loading, setLoading] = useState(false);

  const doSearch = useCallback(async () => {
    setLoading(true);
    try {
      let lat = center[0];
      let lng = center[1];

      if (filters.locationMethod === 'gps') {
        try {
          const pos = await new Promise<GeolocationPosition>((resolve, reject) =>
            navigator.geolocation.getCurrentPosition(resolve, reject, { timeout: 5000 })
          );
          lat = pos.coords.latitude;
          lng = pos.coords.longitude;
          setCenter([lat, lng]);
        } catch {
          // Fall back to default center
        }
      } else if (filters.locationMethod === 'address' && filters.address.trim()) {
        const result = await geocodeAddress(filters.address);
        if (result) {
          lat = result.lat;
          lng = result.lng;
          setCenter([lat, lng]);
        }
      } else if (filters.locationMethod === 'metro') {
        const coords = METRO_COORDS[filters.metro];
        if (coords) {
          [lat, lng] = coords;
          setCenter(coords);
        }
      }

      const data = await fetchLocations({
        lat,
        lng,
        radius: filters.distance || undefined,
      });
      setLocations(data);
    } catch (err) {
      console.error('Failed to fetch locations:', err);
    } finally {
      setLoading(false);
    }
  }, [filters, center]);

  // Initial load
  useEffect(() => {
    doSearch();
  }, []); // eslint-disable-line react-hooks/exhaustive-deps

  const filteredLocations = searchQuery
    ? locations.filter((loc) =>
        loc.name.toLowerCase().includes(searchQuery.toLowerCase())
      )
    : locations;

  return (
    <div className="flex h-screen bg-[#fdf8f5]">
      <SidePanel filters={filters} onFiltersChange={setFilters} onSearch={doSearch} />
      <MapCanvas
        locations={filteredLocations}
        center={center}
        searchQuery={searchQuery}
        onSearchChange={setSearchQuery}
      />
      {loading && (
        <div className="absolute top-4 right-4 z-[1001] bg-[#4c2c5a] text-white font-['Manrope'] text-sm px-4 py-2 rounded-full shadow-lg">
          Searching...
        </div>
      )}
    </div>
  );
}
