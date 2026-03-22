import type { Location } from './types';

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
