import type { Filters, FiltersResponse, MetroArea } from '../types';
import logo from '../assets/MGC_Logo_purple.webp';

const DISTANCE_STEPS = [1, 2, 5, 10, 25, 0]; // 0 = All
const DISTANCE_LABELS = ['1mi', '2mi', '5mi', '10mi', '25mi', 'All'];

interface SidePanelProps {
  filters: Filters;
  onFiltersChange: (filters: Filters) => void;
  onSearch: () => void;
  open: boolean;
  onToggle: () => void;
  metroAreas: MetroArea[];
  availableFilters: FiltersResponse | null;
}

export default function SidePanel({ filters, onFiltersChange, onSearch, open, onToggle, metroAreas, availableFilters }: SidePanelProps) {
  const update = (partial: Partial<Filters>) =>
    onFiltersChange({ ...filters, ...partial });

  const toggleFlavor = (flavor: string) => {
    const flavors = filters.flavors.includes(flavor)
      ? filters.flavors.filter((f) => f !== flavor)
      : [...filters.flavors, flavor];
    update({ flavors });
  };

  const toggleProductType = (type: string) => {
    const productTypes = filters.productTypes.includes(type)
      ? filters.productTypes.filter((t) => t !== type)
      : [...filters.productTypes, type];
    update({ productTypes });
  };

  const distanceIndex = DISTANCE_STEPS.indexOf(filters.distance);
  const distanceLabel = filters.distance === 0 ? 'All' : `${filters.distance} Miles`;

  return (
    <div
      className="absolute top-0 bottom-0 z-20 flex transition-transform duration-300 ease-in-out"
      style={{ transform: open ? 'translateX(0)' : 'translateX(calc(-100% + 28px))' }}
    >
      <aside className="w-[384px] max-w-[85vw] h-full bg-[#4c2c5a] flex flex-col shrink-0 shadow-[20px_0px_40px_-15px_rgba(0,0,0,0.1)]">
        {/* Header */}
        <div className="bg-[#4c2c5a] px-6 py-4 shadow-[0px_4px_6px_-1px_rgba(0,0,0,0.1),0px_2px_4px_-2px_rgba(0,0,0,0.1)]">
          <div className="flex items-center gap-4">
            <img src={logo} alt="Morning Glory Chai" className="w-16 h-16 rounded-lg object-contain" />
            <div>
              <h1 className="font-['Roboto'] font-bold text-2xl text-[#fdf8f5] leading-[30px]">
                Where's My Chai?
              </h1>
            </div>
          </div>
        </div>

        {/* Scrollable Content */}
        <div className="flex-1 overflow-y-auto px-6 pt-8 pb-4 flex flex-col gap-8">
          {/* Location Section */}
          <section className="flex flex-col gap-4">
            <h2 className="font-['Roboto'] font-bold text-lg text-white leading-7">Location</h2>
            <div className="flex flex-col gap-3">
              <RadioOption
                checked={filters.locationMethod === 'gps'}
                onChange={() => update({ locationMethod: 'gps' })}
                label="Use my location"
              />
              <div className="flex flex-col gap-2">
                <RadioOption
                  checked={filters.locationMethod === 'address'}
                  onChange={() => update({ locationMethod: 'address' })}
                  label="Enter an address"
                />
                <input
                  type="text"
                  placeholder="123 Spice Lane, Seattle..."
                  value={filters.address}
                  onChange={(e) => update({ address: e.target.value })}
                  className="w-full bg-white rounded-lg shadow-[0px_1px_2px_0px_rgba(0,0,0,0.05)] px-3 py-3 text-sm font-['Roboto'] text-[#1c1b1a] placeholder:text-[#6b7280] outline-none border-none"
                />
              </div>
              <div className="flex flex-col gap-2 pt-2">
                <RadioOption
                  checked={filters.locationMethod === 'metro'}
                  onChange={() => update({ locationMethod: 'metro' })}
                  label="Choose Metro Area"
                />
                <div className="relative">
                  <select
                    value={filters.metro}
                    onChange={(e) => update({ metro: e.target.value })}
                    className="w-full bg-white rounded-lg shadow-[0px_1px_2px_0px_rgba(0,0,0,0.05)] px-3 py-3 text-sm font-['Roboto'] text-[#1c1b1a] outline-none appearance-none border-none cursor-pointer"
                  >
                    {metroAreas.map((metro) => (
                      <option key={metro.name} value={metro.name}>{metro.name}</option>
                    ))}
                  </select>
                  <svg className="absolute right-3 top-1/2 -translate-y-1/2 w-5 h-5 text-[#7d747e] pointer-events-none" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                  </svg>
                </div>
              </div>
            </div>
          </section>

          {/* Distance Section */}
          <section className="flex flex-col gap-4">
            <div className="flex items-center justify-between">
              <h2 className="font-['Roboto'] font-bold text-lg text-white leading-7">Distance</h2>
              <span className="bg-[#fdd3f4] text-[#4c2c5a] font-['Roboto'] font-bold text-xs px-2 py-1 rounded-full">
                {distanceLabel}
              </span>
            </div>
            <div className="px-2 flex flex-col gap-2">
              <input
                type="range"
                min={0}
                max={DISTANCE_STEPS.length - 1}
                value={distanceIndex >= 0 ? distanceIndex : 2}
                onChange={(e) => update({ distance: DISTANCE_STEPS[Number(e.target.value)] })}
                className="w-full h-1.5 bg-[#e6e2df] rounded-full appearance-none cursor-pointer [&::-webkit-slider-thumb]:appearance-none [&::-webkit-slider-thumb]:w-4 [&::-webkit-slider-thumb]:h-4 [&::-webkit-slider-thumb]:rounded-full [&::-webkit-slider-thumb]:bg-[#fdd3f4] [&::-moz-range-thumb]:bg-[#fdd3f4] [&::-moz-range-thumb]:border-none [&::-moz-range-thumb]:w-4 [&::-moz-range-thumb]:h-4 [&::-moz-range-thumb]:rounded-full"
              />
              <div className="flex justify-between">
                {DISTANCE_LABELS.map((label) => (
                  <span key={label} className="font-['Roboto'] font-bold text-[10px] text-white uppercase tracking-tight">
                    {label}
                  </span>
                ))}
              </div>
            </div>
          </section>

          {/* Flavors Section */}
          {availableFilters && availableFilters.flavors.length > 0 && (
            <section className="flex flex-col gap-4">
              <h2 className="font-['Roboto'] font-bold text-lg text-white leading-7">Flavors</h2>
              <div className="flex flex-col gap-2">
                {availableFilters.flavors.map((flavor) => (
                  <CheckboxOption
                    key={flavor.id}
                    checked={filters.flavors.includes(flavor.name)}
                    onChange={() => toggleFlavor(flavor.name)}
                    label={flavor.name}
                  />
                ))}
              </div>
            </section>
          )}

          {/* Product Type Section */}
          {availableFilters && availableFilters.product_types.length > 0 && (
            <section className="flex flex-col gap-4">
              <h2 className="font-['Roboto'] font-bold text-lg text-white leading-7">Product Type</h2>
              <div className="flex flex-col gap-2">
                {availableFilters.product_types.map((type) => (
                  <CheckboxOption
                    key={type.id}
                    checked={filters.productTypes.includes(type.name)}
                    onChange={() => toggleProductType(type.name)}
                    label={type.name}
                  />
                ))}
              </div>
            </section>
          )}
        </div>

        {/* Footer CTA */}
        <div className="bg-[#ece7e4] border-t border-[rgba(206,195,206,0.1)] px-6 py-6">
          <button
            onClick={onSearch}
            className="w-full bg-[#4c2c5a] text-white font-['Roboto'] font-bold text-lg py-4 rounded-xl shadow-[0px_10px_15px_-3px_rgba(0,0,0,0.1),0px_4px_6px_-4px_rgba(0,0,0,0.1)] hover:bg-[#3a1f47] transition-colors cursor-pointer"
          >
            Find my Chai
          </button>
        </div>
      </aside>

      {/* Toggle Tab */}
      <button
        onClick={onToggle}
        className="self-center -ml-px h-12 w-7 bg-[#4c2c5a] rounded-r-lg flex items-center justify-center shadow-[4px_0px_8px_rgba(0,0,0,0.15)] cursor-pointer border-none outline-none"
        aria-label={open ? 'Close filters' : 'Open filters'}
      >
        <svg
          className="w-4 h-4 text-white transition-transform duration-300"
          style={{ transform: open ? 'rotate(0deg)' : 'rotate(180deg)' }}
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
        </svg>
      </button>
    </div>
  );
}

function RadioOption({ checked, onChange, label }: { checked: boolean; onChange: () => void; label: string }) {
  return (
    <label className="flex items-center gap-3 cursor-pointer">
      <div
        className={`w-[22px] h-[22px] rounded-full border flex items-center justify-center shrink-0 ${
          checked ? 'bg-[#fdd3f4] border-transparent' : 'bg-white border-[#7d747e]'
        }`}
        onClick={onChange}
      >
        {checked && (
          <svg className="w-3.5 h-3.5 text-white" fill="currentColor" viewBox="0 0 20 20">
            <circle cx="10" cy="10" r="5" />
          </svg>
        )}
      </div>
      <span className="font-['Roboto'] text-base text-white">{label}</span>
    </label>
  );
}

function CheckboxOption({ checked, onChange, label }: { checked: boolean; onChange: () => void; label: string }) {
  return (
    <div
      onClick={onChange}
      className="bg-white rounded-xl shadow-[0px_1px_2px_0px_rgba(0,0,0,0.05)] px-3 py-3 flex items-center justify-between cursor-pointer"
    >
      <span className="font-['Roboto'] font-semibold text-sm text-[#4c444d]">{label}</span>
      <div
        className={`w-[22px] h-[22px] rounded-md flex items-center justify-center shrink-0 ${
          checked ? 'bg-[#4c2c5a]' : 'bg-white border border-[#cec3ce]'
        }`}
      >
        {checked && (
          <svg className="w-3.5 h-3.5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={3} d="M5 13l4 4L19 7" />
          </svg>
        )}
      </div>
    </div>
  );
}
