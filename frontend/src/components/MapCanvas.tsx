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

function ChaiMarker({ location }: { location: Location }) {
  const [open, setOpen] = useState(false);

  return (
    <>
      <AdvancedMarker
        position={{ lat: location.lat, lng: location.lng }}
        onClick={() => setOpen(true)}
      >
        <img src={markerIcon} alt={location.name} width={40} height={40} />
      </AdvancedMarker>
      {open && (
        <InfoWindow
          position={{ lat: location.lat, lng: location.lng }}
          onCloseClick={() => setOpen(false)}
          pixelOffset={[0, -40]}
        >
          <div className="font-['Roboto'] min-w-[200px]">
            <h3 className="font-['Roboto'] font-bold text-sm text-[#351643] m-0 mb-1">{location.name}</h3>
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
            <ChaiMarker key={loc.id} location={loc} />
          ))}
        </Map>
      </APIProvider>

    </div>
  );
}
