import type { Filters } from '../types';

const FLAVORS = ['Original Spicy', 'Herbal (no caffeine)', 'Green', 'Lemon', 'Hibiscus'];
const PRODUCT_TYPES = ['Chai Served here', '16oz Bottle', 'Dry mix'];
const DISTANCE_STEPS = [1, 2, 5, 10, 25, 0]; // 0 = All
const DISTANCE_LABELS = ['1mi', '2mi', '5mi', '10mi', '25mi', 'All'];

interface SidePanelProps {
  filters: Filters;
  onFiltersChange: (filters: Filters) => void;
  onSearch: () => void;
}

export default function SidePanel({ filters, onFiltersChange, onSearch }: SidePanelProps) {
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
    <aside className="w-[384px] h-full bg-[#f8f3f0] flex flex-col shrink-0 shadow-[20px_0px_40px_-15px_rgba(0,0,0,0.1)] z-10 relative">
      {/* Header */}
      <div className="bg-[#4c2c5a] px-6 py-4 shadow-[0px_4px_6px_-1px_rgba(0,0,0,0.1),0px_2px_4px_-2px_rgba(0,0,0,0.1)]">
        <div className="flex items-center gap-4">
          <div className="w-16 h-16 rounded-lg border border-[rgba(253,248,245,0.2)] bg-[#351643] flex items-center justify-center overflow-hidden">
            <span className="text-2xl text-white/80">🍵</span>
          </div>
          <div>
            <h1 className="font-['Noto_Serif'] font-bold text-2xl text-[#fdf8f5] leading-[30px]">
              Where's My Chai?
            </h1>
          </div>
        </div>
      </div>

      {/* Scrollable Content */}
      <div className="flex-1 overflow-y-auto px-6 pt-8 pb-4 flex flex-col gap-8">
        {/* Location Section */}
        <section className="flex flex-col gap-4">
          <div className="flex items-center gap-2">
            <svg className="w-4 h-5 text-[#351643]" fill="currentColor" viewBox="0 0 16 20">
              <path d="M8 0C3.58 0 0 3.58 0 8c0 5.25 7.2 11.38 7.5 11.62a.75.75 0 00.98 0C8.8 19.38 16 13.25 16 8c0-4.42-3.58-8-8-8zm0 11a3 3 0 110-6 3 3 0 010 6z" />
            </svg>
            <h2 className="font-['Noto_Serif'] font-bold text-lg text-[#351643] leading-7">Location</h2>
          </div>
          <div className="flex flex-col gap-3">
            <RadioOption
              checked={filters.locationMethod === 'gps'}
              onChange={() => update({ locationMethod: 'gps' })}
              label="Use my phone location"
            />
            <div className="flex flex-col gap-2">
              <RadioOption
                checked={filters.locationMethod === 'address'}
                onChange={() => update({ locationMethod: 'address' })}
                label="Enter my address"
              />
              <input
                type="text"
                placeholder="123 Spice Lane, Seattle..."
                value={filters.address}
                onChange={(e) => update({ address: e.target.value })}
                className="w-full bg-white rounded-lg shadow-[0px_1px_2px_0px_rgba(0,0,0,0.05)] px-3 py-3 text-sm font-['Manrope'] text-[#1c1b1a] placeholder:text-[#6b7280] outline-none border-none"
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
                  className="w-full bg-white rounded-lg shadow-[0px_1px_2px_0px_rgba(0,0,0,0.05)] px-3 py-3 text-sm font-['Manrope'] text-[#1c1b1a] outline-none appearance-none border-none cursor-pointer"
                >
                  <option value="San Francisco, CA">San Francisco, CA</option>
                  <option value="Seattle, WA">Seattle, WA</option>
                  <option value="Portland, OR">Portland, OR</option>
                  <option value="Los Angeles, CA">Los Angeles, CA</option>
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
            <div className="flex items-center gap-2">
              <svg className="w-3.5 h-5 text-[#351643]" fill="currentColor" viewBox="0 0 14 20">
                <path d="M7 0C3.13 0 0 3.13 0 7c0 4.5 6.3 10.08 6.56 10.32a.66.66 0 00.88 0C7.7 17.08 14 11.5 14 7c0-3.87-3.13-7-7-7zm0 9.5a2.5 2.5 0 110-5 2.5 2.5 0 010 5z" />
              </svg>
              <h2 className="font-['Noto_Serif'] font-bold text-lg text-[#351643] leading-7">Distance</h2>
            </div>
            <span className="bg-[#fdd3f4] text-[#4c2c5a] font-['Manrope'] font-bold text-xs px-2 py-1 rounded-full">
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
              className="w-full h-1.5 bg-[#e6e2df] rounded-full appearance-none cursor-pointer accent-[#4c2c5a]"
            />
            <div className="flex justify-between">
              {DISTANCE_LABELS.map((label) => (
                <span key={label} className="font-['Manrope'] font-bold text-[10px] text-[#7d747e] uppercase tracking-tight">
                  {label}
                </span>
              ))}
            </div>
          </div>
        </section>

        {/* Flavors Section */}
        <section className="flex flex-col gap-4">
          <div className="flex items-center gap-2">
            <svg className="w-5 h-5 text-[#351643]" fill="currentColor" viewBox="0 0 20 20">
              <path d="M10 2a8 8 0 100 16 8 8 0 000-16zm1 11H9v-2h2v2zm0-4H9V5h2v4z" />
            </svg>
            <h2 className="font-['Noto_Serif'] font-bold text-lg text-[#351643] leading-7">Flavors</h2>
          </div>
          <div className="flex flex-col gap-2">
            {FLAVORS.map((flavor) => (
              <CheckboxOption
                key={flavor}
                checked={filters.flavors.includes(flavor)}
                onChange={() => toggleFlavor(flavor)}
                label={flavor}
              />
            ))}
          </div>
        </section>

        {/* Product Type Section */}
        <section className="flex flex-col gap-4">
          <div className="flex items-center gap-2">
            <svg className="w-5 h-5 text-[#351643]" fill="currentColor" viewBox="0 0 20 20">
              <path d="M4 3h12a2 2 0 012 2v10a2 2 0 01-2 2H4a2 2 0 01-2-2V5a2 2 0 012-2zm0 2v10h12V5H4z" />
            </svg>
            <h2 className="font-['Noto_Serif'] font-bold text-lg text-[#351643] leading-7">Product Type</h2>
          </div>
          <div className="flex flex-col gap-2">
            {PRODUCT_TYPES.map((type) => (
              <CheckboxOption
                key={type}
                checked={filters.productTypes.includes(type)}
                onChange={() => toggleProductType(type)}
                label={type}
              />
            ))}
          </div>
        </section>
      </div>

      {/* Footer CTA */}
      <div className="bg-[#ece7e4] border-t border-[rgba(206,195,206,0.1)] px-6 py-6">
        <button
          onClick={onSearch}
          className="w-full bg-[#4c2c5a] text-white font-['Noto_Serif'] font-bold text-lg py-4 rounded-xl shadow-[0px_10px_15px_-3px_rgba(0,0,0,0.1),0px_4px_6px_-4px_rgba(0,0,0,0.1)] hover:bg-[#3a1f47] transition-colors cursor-pointer"
        >
          Find a Brew
        </button>
      </div>
    </aside>
  );
}

function RadioOption({ checked, onChange, label }: { checked: boolean; onChange: () => void; label: string }) {
  return (
    <label className="flex items-center gap-3 cursor-pointer">
      <div
        className={`w-[22px] h-[22px] rounded-full border flex items-center justify-center shrink-0 ${
          checked ? 'bg-[#351643] border-transparent' : 'bg-white border-[#7d747e]'
        }`}
        onClick={onChange}
      >
        {checked && (
          <svg className="w-3.5 h-3.5 text-white" fill="currentColor" viewBox="0 0 20 20">
            <circle cx="10" cy="10" r="5" />
          </svg>
        )}
      </div>
      <span className="font-['Manrope'] text-base text-[#1c1b1a]">{label}</span>
    </label>
  );
}

function CheckboxOption({ checked, onChange, label }: { checked: boolean; onChange: () => void; label: string }) {
  return (
    <div
      onClick={onChange}
      className="bg-white rounded-xl shadow-[0px_1px_2px_0px_rgba(0,0,0,0.05)] px-3 py-3 flex items-center justify-between cursor-pointer"
    >
      <span className="font-['Manrope'] font-semibold text-sm text-[#4c444d]">{label}</span>
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
