import { useEffect, useState } from 'react'
import './App.css'

interface FilterOption {
  id: number
  name: string
}

interface LocationItem {
  id: number
  location_id: number
  flavor_id: number
  product_id: number
  flavor: FilterOption
  product_type: FilterOption
}

interface Location {
  id: number
  name: string
  address: string
  lat: number
  lng: number
  phone: string
  photo_url: string
  website_url: string
  active: boolean
  items: LocationItem[]
}

interface FiltersResponse {
  flavors: FilterOption[]
  product_types: FilterOption[]
}

type View = { kind: 'list' } | { kind: 'edit'; id: number } | { kind: 'add' }
type Tab = 'locations' | 'flavors' | 'productTypes'

export default function App() {
  const [user, setUser] = useState<string | null>(null)
  const [authChecked, setAuthChecked] = useState(false)

  useEffect(() => {
    fetch('/api/auth/me', { credentials: 'include' })
      .then((res) => (res.ok ? res.json() : null))
      .then((data) => setUser(data?.username ?? null))
      .finally(() => setAuthChecked(true))
  }, [])

  if (!authChecked) return <div className="loading">Loading...</div>

  if (!user) {
    return (
      <div className="app">
        <LoginPage onLogin={setUser} />
      </div>
    )
  }

  return <AdminApp user={user} onLogout={() => setUser(null)} />
}

function LoginPage({ onLogin }: { onLogin: (username: string) => void }) {
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [submitting, setSubmitting] = useState(false)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setSubmitting(true)
    setError(null)
    try {
      const res = await fetch('/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include',
        body: JSON.stringify({ username, password }),
      })
      if (!res.ok) {
        const data = await res.json().catch(() => null)
        throw new Error(data?.message ?? 'Login failed')
      }
      onLogin(username)
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Login failed')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="login-wrapper">
      <form className="login-card" onSubmit={handleSubmit}>
        <h1>WMC Admin</h1>
        {error && <div className="login-error">{error}</div>}
        <div className="login-field">
          <label htmlFor="username">Username</label>
          <input
            id="username"
            type="text"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            autoFocus
          />
        </div>
        <div className="login-field">
          <label htmlFor="password">Password</label>
          <input
            id="password"
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
          />
        </div>
        <button className="login-btn" type="submit" disabled={submitting || !username || !password}>
          {submitting ? 'Signing in...' : 'Sign in'}
        </button>
      </form>
    </div>
  )
}

function AdminApp({ user, onLogout }: { user: string; onLogout: () => void }) {
  const [locations, setLocations] = useState<Location[]>([])
  const [filters, setFilters] = useState<FiltersResponse | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [tab, setTab] = useState<Tab>('locations')
  const [view, setView] = useState<View>({ kind: 'list' })

  const handleLogout = async () => {
    await fetch('/api/auth/logout', { method: 'POST', credentials: 'include' })
    onLogout()
  }

  useEffect(() => {
    async function fetchData() {
      try {
        const [locRes, filterRes] = await Promise.all([
          fetch('/api/locations?includeInactive=true', { credentials: 'include' }),
          fetch('/api/locations/filters'),
        ])
        if (!locRes.ok) throw new Error(`Locations: ${locRes.status}`)
        if (!filterRes.ok) throw new Error(`Filters: ${filterRes.status}`)
        setLocations(await locRes.json())
        setFilters(await filterRes.json())
      } catch (e) {
        setError(e instanceof Error ? e.message : 'Failed to fetch data')
      } finally {
        setLoading(false)
      }
    }
    fetchData()
  }, [])

  if (loading) return <div className="loading">Loading...</div>
  if (error) return <div className="error">Error: {error}</div>

  if (view.kind === 'edit') {
    const location = locations.find((l) => l.id === view.id)
    if (!location) return <div className="error">Location not found</div>
    return (
      <div className="app">
        <LocationForm
          mode="edit"
          location={location}
          filters={filters!}
          onBack={() => setView({ kind: 'list' })}
          onSaved={(updated) => {
            setLocations((prev) => prev.map((l) => (l.id === updated.id ? updated : l)))
            setView({ kind: 'list' })
          }}
        />
      </div>
    )
  }

  if (view.kind === 'add') {
    return (
      <div className="app">
        <LocationForm
          mode="add"
          filters={filters!}
          onBack={() => setView({ kind: 'list' })}
          onSaved={(created) => {
            setLocations((prev) => [...prev, created])
            setView({ kind: 'list' })
          }}
        />
      </div>
    )
  }

  return (
    <div className="app">
      <div className="app-header">
        <h1>WMC Admin</h1>
        <div className="user-info">
          <span>{user}</span>
          <button className="logout-btn" onClick={handleLogout}>Logout</button>
        </div>
      </div>

      <nav className="tabs">
        <button className={tab === 'locations' ? 'active' : ''} onClick={() => setTab('locations')}>
          Locations ({locations.length})
        </button>
        <button className={tab === 'flavors' ? 'active' : ''} onClick={() => setTab('flavors')}>
          Flavors ({filters?.flavors.length ?? 0})
        </button>
        <button className={tab === 'productTypes' ? 'active' : ''} onClick={() => setTab('productTypes')}>
          Product Types ({filters?.product_types.length ?? 0})
        </button>
      </nav>

      {tab === 'locations' && (
        <LocationsTable
          locations={locations}
          onEdit={(id) => setView({ kind: 'edit', id })}
          onAdd={() => setView({ kind: 'add' })}
        />
      )}
      {tab === 'flavors' && <FilterTable title="Flavors" items={filters?.flavors ?? []} />}
      {tab === 'productTypes' && <FilterTable title="Product Types" items={filters?.product_types ?? []} />}
    </div>
  )
}

function LocationsTable({
  locations,
  onEdit,
  onAdd,
}: {
  locations: Location[]
  onEdit: (id: number) => void
  onAdd: () => void
}) {
  const [search, setSearch] = useState('')
  const [showInactive, setShowInactive] = useState(false)

  let filtered = showInactive ? locations : locations.filter((l) => l.active)
  if (search.length > 2)
    filtered = filtered.filter((l) => l.name.toLowerCase().includes(search.toLowerCase()))

  return (
    <>
      <div className="search-bar">
        <input
          type="text"
          placeholder="Filter by name..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        {search && <button className="clear-btn" onClick={() => setSearch('')}>&times;</button>}
        <label className="checkbox-label">
          <input
            type="checkbox"
            checked={showInactive}
            onChange={(e) => setShowInactive(e.target.checked)}
          />
          Show Inactive Locations
        </label>
        {search.length > 0 && search.length <= 2 && (
          <span className="search-hint">Type at least 3 characters to filter</span>
        )}
        {search.length > 2 && (
          <span className="search-hint">{filtered.length} result{filtered.length !== 1 ? 's' : ''}</span>
        )}
        <button className="add-btn" onClick={onAdd}>+ Add New Location</button>
      </div>
      <div className="table-wrapper">
      <table>
        <thead>
          <tr>
            <th>ID</th>
            <th>Name</th>
            <th>Address</th>
            <th>Phone</th>
            <th>Website</th>
            <th>Lat</th>
            <th>Lng</th>
            <th>Flavors</th>
            <th>Product Types</th>
          </tr>
        </thead>
        <tbody>
          {filtered.map((loc) => (
            <tr key={loc.id}>
              <td>
                <a href="#" onClick={(e) => { e.preventDefault(); onEdit(loc.id) }}>
                  {loc.id}
                </a>
              </td>
              <td>{loc.name}</td>
              <td>{loc.address}</td>
              <td>{loc.phone}</td>
              <td>
                {loc.website_url ? (
                  <a href={loc.website_url} target="_blank" rel="noreferrer">
                    Link
                  </a>
                ) : (
                  '—'
                )}
              </td>
              <td>{loc.lat.toFixed(4)}</td>
              <td>{loc.lng.toFixed(4)}</td>
              <td>
                {[...new Set(loc.items.map((i) => i.flavor.name))].join(', ') || '—'}
              </td>
              <td>
                {[...new Set(loc.items.map((i) => i.product_type.name))].join(', ') || '—'}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
    </>
  )
}

function FilterTable({ title, items }: { title: string; items: FilterOption[] }) {
  return (
    <div className="table-wrapper">
      <table>
        <thead>
          <tr>
            <th>ID</th>
            <th>{title} Name</th>
          </tr>
        </thead>
        <tbody>
          {items.map((item) => (
            <tr key={item.id}>
              <td>{item.id}</td>
              <td>{item.name}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

const GOOGLE_MAPS_API_KEY = import.meta.env.VITE_GOOGLE_MAPS_API_KEY

async function geocodeAddress(address: string): Promise<{ lat: number; lng: number } | null> {
  const res = await fetch(
    `https://maps.googleapis.com/maps/api/geocode/json?address=${encodeURIComponent(address)}&key=${GOOGLE_MAPS_API_KEY}`
  )
  if (!res.ok) return null
  const data = await res.json()
  if (data.status !== 'OK' || !data.results?.length) return null
  const { lat, lng } = data.results[0].geometry.location
  return { lat, lng }
}

type LocationFormProps = {
  filters: FiltersResponse
  onBack: () => void
  onSaved: (location: Location) => void
} & (
  | { mode: 'edit'; location: Location }
  | { mode: 'add'; location?: undefined }
)

function LocationForm({ mode, location, filters, onBack, onSaved }: LocationFormProps) {
  const [name, setName] = useState(location?.name ?? '')
  const [address, setAddress] = useState(location?.address ?? '')
  const [phone, setPhone] = useState(location?.phone ?? '')
  const [websiteUrl, setWebsiteUrl] = useState(location?.website_url ?? '')
  const [active, setActive] = useState(location?.active ?? true)
  const [selectedFlavors, setSelectedFlavors] = useState<Set<number>>(
    new Set(location?.items.map((i) => i.flavor_id) ?? [])
  )
  const [selectedProductTypes, setSelectedProductTypes] = useState<Set<number>>(
    new Set(location?.items.map((i) => i.product_id) ?? [])
  )
  const [saving, setSaving] = useState(false)
  const [saveError, setSaveError] = useState<string | null>(null)

  const canSave = name.trim().length > 0 && address.trim().length > 0

  const toggleFlavor = (id: number) =>
    setSelectedFlavors((prev) => {
      const next = new Set(prev)
      next.has(id) ? next.delete(id) : next.add(id)
      return next
    })

  const toggleProductType = (id: number) =>
    setSelectedProductTypes((prev) => {
      const next = new Set(prev)
      next.has(id) ? next.delete(id) : next.add(id)
      return next
    })

  const handleSave = async () => {
    if (!canSave) return
    setSaving(true)
    setSaveError(null)
    try {
      const coords = await geocodeAddress(address)
      if (!coords) throw new Error('Could not geocode address')

      const payload = {
        name,
        address,
        phone,
        website_url: websiteUrl,
        active,
        lat: coords.lat,
        lng: coords.lng,
        flavor_ids: [...selectedFlavors],
        product_type_ids: [...selectedProductTypes],
      }

      const url = mode === 'edit' ? `/api/locations/${location.id}` : '/api/locations'
      const method = mode === 'edit' ? 'PUT' : 'POST'

      const res = await fetch(url, {
        method,
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include',
        body: JSON.stringify(payload),
      })
      if (!res.ok) throw new Error(`Save failed: ${res.status}`)
      const saved: Location = await res.json()
      onSaved(saved)
    } catch (e) {
      setSaveError(e instanceof Error ? e.message : 'Save failed')
    } finally {
      setSaving(false)
    }
  }

  return (
    <>
      <button className="back-btn" onClick={onBack}>&larr; Back to list</button>
      <h1>{mode === 'edit' ? `Edit Location: ${location.name}` : 'Add New Location'}</h1>

      <div className="form-card">
        {mode === 'edit' && (
          <div className="form-section">
            <h2>Info</h2>
            <div className="form-row">
              <label>ID</label>
              <span className="readonly-value">{location.id}</span>
            </div>
            <div className="form-row">
              <label>Latitude</label>
              <span className="readonly-value">{location.lat}</span>
            </div>
            <div className="form-row">
              <label>Longitude</label>
              <span className="readonly-value">{location.lng}</span>
            </div>
          </div>
        )}

        <div className="form-section">
          <h2>Details</h2>
          <div className="form-row">
            <label htmlFor="name">Name *</label>
            <input id="name" type="text" value={name} onChange={(e) => setName(e.target.value)} />
          </div>
          <div className="form-row">
            <label htmlFor="address">Address *</label>
            <input id="address" type="text" value={address} onChange={(e) => setAddress(e.target.value)} />
          </div>
          <div className="form-row">
            <label htmlFor="phone">Phone</label>
            <input id="phone" type="text" value={phone} onChange={(e) => setPhone(e.target.value)} />
          </div>
          <div className="form-row">
            <label htmlFor="website">Website</label>
            <input id="website" type="text" value={websiteUrl} onChange={(e) => setWebsiteUrl(e.target.value)} />
          </div>
          <div className="form-row">
            <label className="checkbox-label">
              <input
                type="checkbox"
                checked={active}
                onChange={(e) => setActive(e.target.checked)}
              />
              Active
            </label>
          </div>
        </div>

        <div className="form-section">
          <h2>Flavor</h2>
          <div className="checkbox-group">
            {filters.flavors.map((f) => (
              <label key={f.id} className="checkbox-label">
                <input
                  type="checkbox"
                  value={f.id}
                  checked={selectedFlavors.has(f.id)}
                  onChange={() => toggleFlavor(f.id)}
                />
                {f.name}
              </label>
            ))}
          </div>
        </div>

        <div className="form-section">
          <h2>Product Type</h2>
          <div className="checkbox-group">
            {filters.product_types.map((pt) => (
              <label key={pt.id} className="checkbox-label">
                <input
                  type="checkbox"
                  value={pt.id}
                  checked={selectedProductTypes.has(pt.id)}
                  onChange={() => toggleProductType(pt.id)}
                />
                {pt.name}
              </label>
            ))}
          </div>
        </div>

        <div className="form-section">
          {saveError && <div className="save-error">{saveError}</div>}
          <button className="save-btn" onClick={handleSave} disabled={saving || !canSave}>
            {saving ? 'Saving...' : 'Save'}
          </button>
          {!canSave && <span className="save-hint">Name and Address are required</span>}
        </div>

        {mode === 'edit' && (
          <div className="form-section">
            <h2>JSON</h2>
            <textarea
              className="json-preview"
              readOnly
              value={JSON.stringify(location, null, 2)}
            />
          </div>
        )}
      </div>
    </>
  )
}
