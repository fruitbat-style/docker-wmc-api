import type { Location } from './types';

const GOOGLE_MAPS_API_KEY = import.meta.env.VITE_GOOGLE_MAPS_API_KEY;

export async function geocodeAddress(address: string): Promise<{ lat: number; lng: number } | null> {
  const res = await fetch(
    `https://maps.googleapis.com/maps/api/geocode/json?address=${encodeURIComponent(address)}&key=${GOOGLE_MAPS_API_KEY}`
  );
  if (!res.ok) return null;
  const data = await res.json();
  if (data.status !== 'OK' || !data.results?.length) return null;
  const { lat, lng } = data.results[0].geometry.location;
  return { lat, lng };
}

export async function fetchLocations(params: {
  lat?: number;
  lng?: number;
  radius?: number;
  flavor?: number;
  product?: number;
}): Promise<Location[]> {
  const searchParams = new URLSearchParams();
  if (params.lat) searchParams.set('lat', String(params.lat));
  if (params.lng) searchParams.set('lng', String(params.lng));
  if (params.radius) searchParams.set('radius', String(params.radius));
  if (params.flavor) searchParams.set('flavor', String(params.flavor));
  if (params.product) searchParams.set('product', String(params.product));

  const res = await fetch(`/api/locations?${searchParams}`);
  if (!res.ok) throw new Error('Failed to fetch locations');
  return res.json();
}
