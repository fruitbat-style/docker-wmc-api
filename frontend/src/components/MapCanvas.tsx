import { APIProvider, Map, Marker, InfoWindow, useMap } from '@vis.gl/react-google-maps';
import type { Location } from '../types';
import { useState, useEffect } from 'react';
import markerIcon from '../assets/markerFlowerIcon.png';

const API_KEY = import.meta.env.VITE_GOOGLE_MAPS_API_KEY;

interface MapCanvasProps {
  locations: Location[];
  center: [number, number];
  searchQuery: string;
  onSearchChange: (query: string) => void;
}

function MapUpdater({ center }: { center: [number, number] }) {
  const map = useMap();
  useEffect(() => {
    if (map) {
      map.panTo({ lat: center[0], lng: center[1] });
    }
  }, [center, map]);
  return null;
}

function ChaiMarker({ location }: { location: Location }) {
  const [open, setOpen] = useState(false);

  return (
    <>
      <Marker
        position={{ lat: location.lat, lng: location.lng }}
        onClick={() => setOpen(true)}
        icon={{
          url: markerIcon,
          scaledSize: { width: 40, height: 40, equals: () => false },
        }}
      />
      {open && (
        <InfoWindow
          position={{ lat: location.lat, lng: location.lng }}
          onCloseClick={() => setOpen(false)}
          pixelOffset={[0, -40]}
        >
          <div className="font-['Manrope'] min-w-[200px]">
            <h3 className="font-['Noto_Serif'] font-bold text-sm text-[#351643] m-0 mb-1">{location.name}</h3>
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

export default function MapCanvas({ locations, center, searchQuery, onSearchChange }: MapCanvasProps) {
  return (
    <div className="flex-1 relative h-full">
      <APIProvider apiKey={API_KEY}>
        <Map
          defaultCenter={{ lat: center[0], lng: center[1] }}
          defaultZoom={13}
          disableDefaultUI={false}
          gestureHandling="greedy"
          className="h-full w-full"
        >
          <MapUpdater center={center} />
          {locations.map((loc) => (
            <ChaiMarker key={loc.id} location={loc} />
          ))}
        </Map>
      </APIProvider>

      {/* Search Overlay */}
      <div className="absolute top-8 left-1/2 -translate-x-1/2 z-[1000] w-[448px] max-w-[calc(100%-48px)]">
        <div className="backdrop-blur-md bg-[rgba(253,248,245,0.85)] rounded-full px-6 py-3 flex items-center gap-4 shadow-[0px_20px_25px_-5px_rgba(0,0,0,0.05),0px_8px_10px_-6px_rgba(0,0,0,0.05)]">
          <svg className="w-[18px] h-[18px] text-[#4c2c5a] shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
          </svg>
          <input
            type="text"
            placeholder="Search by shop name..."
            value={searchQuery}
            onChange={(e) => onSearchChange(e.target.value)}
            className="flex-1 bg-transparent font-['Noto_Serif'] text-base text-[#4c2c5a] placeholder:text-[rgba(76,44,90,0.4)] outline-none border-none"
          />
          <div className="w-px h-6 bg-[rgba(53,22,67,0.1)]" />
          <svg className="w-[22px] h-[22px] text-[#4c2c5a] shrink-0 cursor-pointer" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
          </svg>
        </div>
      </div>
    </div>
  );
}
