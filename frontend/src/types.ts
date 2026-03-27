export interface LocationItem {
  id: number;
  location_id: number;
  flavor_id: number;
  product_id: number;
  flavor: FilterOption;
  product_type: FilterOption;
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

export interface MetroArea {
  name: string;
  coords: [number, number];
}

export interface FilterOption {
  id: number;
  name: string;
}

export interface FiltersResponse {
  flavors: FilterOption[];
  product_types: FilterOption[];
}
