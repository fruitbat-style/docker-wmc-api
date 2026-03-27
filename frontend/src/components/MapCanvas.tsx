import { APIProvider, Map, AdvancedMarker, InfoWindow, useMap } from '@vis.gl/react-google-maps';
import type { Location } from '../types';
import { useState, useEffect } from 'react';
import markerIcon from '../assets/markerFlowerIcon.png';

const API_KEY = import.meta.env.VITE_GOOGLE_MAPS_API_KEY;

interface MapCanvasProps {
  locations: Location[];
  center: [number, number];
}

function MapUpdater({ center, locations }: { center: [number, number]; locations: Location[] }) {
  const map = useMap();
  useEffect(() => {
    if (!map) return;
    if (locations.length > 0) {
      const bounds = new google.maps.LatLngBounds();
      locations.forEach((loc) => bounds.extend({ lat: loc.lat, lng: loc.lng }));
      map.fitBounds(bounds, 50);
    } else {
      map.panTo({ lat: center[0], lng: center[1] });
    }
  }, [center, locations, map]);
  return null;
}

function ChaiMarker({ location, open, onOpen, onClose }: { location: Location; open: boolean; onOpen: () => void; onClose: () => void }) {
  return (
    <>
      <AdvancedMarker
        position={{ lat: location.lat, lng: location.lng }}
        onClick={onOpen}
      >
        <img src={markerIcon} alt={location.name} width={40} height={40} />
      </AdvancedMarker>
      {open && (
        <InfoWindow
          position={{ lat: location.lat, lng: location.lng }}
          onCloseClick={onClose}
          pixelOffset={[0, -40]}
          headerDisabled
        >
          <div className="font-['Roboto'] min-w-[200px]">
            <div className="flex items-start justify-between gap-2 mb-1">
              <h3 className="font-['Roboto'] font-bold text-sm text-[#351643] m-0">{location.name}</h3>
              <button
                onClick={onClose}
                className="text-[#7d747e] hover:text-[#351643] cursor-pointer bg-transparent border-none p-0 text-lg leading-none shrink-0 font-bold"
              >
                &times;
              </button>
            </div>
            <p className="text-xs text-[#4c444d] m-0 mb-1">{location.address}</p>
            {location.phone && <p className="text-xs text-[#4c444d] m-0 mb-1">{location.phone}</p>}
            {location.website_url && (
              <a
                href={location.website_url}
                target="_blank"
                rel="noopener noreferrer"
                className="text-xs text-[#4c2c5a] underline"
              >
                Visit website
              </a>
            )}
          </div>
        </InfoWindow>
      )}
    </>
  );
}

export default function MapCanvas({ locations, center }: MapCanvasProps) {
  const [openId, setOpenId] = useState<number | null>(null);

  return (
    <div className="absolute inset-0">
      <APIProvider apiKey={API_KEY}>
        <Map
          defaultCenter={{ lat: center[0], lng: center[1] }}
          defaultZoom={12}
          mapId="wmc-map"
          disableDefaultUI={false}
          gestureHandling="greedy"
          className="h-full w-full"
        >
          <MapUpdater center={center} locations={locations} />
          {locations.map((loc) => (
            <ChaiMarker
              key={loc.id}
              location={loc}
              open={openId === loc.id}
              onOpen={() => setOpenId(loc.id)}
              onClose={() => setOpenId(null)}
            />
          ))}
        </Map>
      </APIProvider>

    </div>
  );
}
