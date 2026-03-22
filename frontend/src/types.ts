export interface LocationItem {
  id: number;
  location_id: number;
  product_id: number;
  flavor_id: number;
  product_name: string;
  flavor_name: string;
}

export interface Location {
  id: number;
  name: string;
  address: string;
  lat: number;
  lng: number;
  phone: string;
  photo_url: string;
  website_url: string;
  items: LocationItem[];
}

export type LocationMethod = 'gps' | 'address' | 'metro';

export interface Filters {
  locationMethod: LocationMethod;
  address: string;
  metro: string;
  distance: number;
  flavors: string[];
  productTypes: string[];
}
