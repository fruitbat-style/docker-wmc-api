using Microsoft.EntityFrameworkCore;
using WMCApi.Data;

namespace WMCApi.Services;

public class LocationService : ILocationService
{
    /// <summary>Mean radius of the Earth in miles, used for Haversine distance calculation.</summary>
    internal const double EarthRadiusMiles = 3958.8;

    private readonly WmcDbContext _db;

    public LocationService(WmcDbContext db)
    {
        _db = db;
    }

    public async Task<List<Location>> SearchAsync(double lat, double lng, double radius, int[] flavors, int[] products)
    {
        var query = _db.Locations
            .Include(l => l.Items).ThenInclude(i => i.Flavor)
            .Include(l => l.Items).ThenInclude(i => i.ProductType)
            .AsQueryable();

        if (flavors.Length > 0)
            query = query.Where(l => l.Items.Any(i => flavors.Contains(i.FlavorId)));

        if (products.Length > 0)
            query = query.Where(l => l.Items.Any(i => products.Contains(i.ProductId)));

        var locations = await query.ToListAsync();

        var filterByDistance = radius > 0 && (lat != 0 || lng != 0);
        if (filterByDistance)
            locations = locations.Where(l => HaversineDistance(lat, lng, l.Lat, l.Lng) <= radius).ToList();

        return locations;
    }

    public async Task<FiltersResponse> GetFiltersAsync()
    {
        var flavors = await _db.Flavors
            .OrderBy(f => f.Id)
            .ToListAsync();

        var products = await _db.ProductTypes
            .OrderBy(p => p.Id)
            .ToListAsync();

        return new FiltersResponse
        {
            Flavors = flavors.Select(f => new FilterOption { Id = f.Id, Name = f.Name }).ToList(),
            ProductTypes = products.Select(p => new FilterOption { Id = p.Id, Name = p.Name }).ToList(),
        };
    }

    public async Task<Location> CreateAsync(LocationUpdateRequest request)
    {
        var location = new Location
        {
            Name = request.Name,
            Address = request.Address,
            Phone = request.Phone,
            WebsiteUrl = request.WebsiteUrl,
            Lat = request.Lat,
            Lng = request.Lng,
        };

        foreach (var flavorId in request.FlavorIds)
        {
            foreach (var productTypeId in request.ProductTypeIds)
            {
                location.Items.Add(new LocationItem
                {
                    FlavorId = flavorId,
                    ProductId = productTypeId,
                });
            }
        }

        _db.Locations.Add(location);
        await _db.SaveChangesAsync();

        return await _db.Locations
            .Include(l => l.Items).ThenInclude(i => i.Flavor)
            .Include(l => l.Items).ThenInclude(i => i.ProductType)
            .FirstAsync(l => l.Id == location.Id);
    }

    public async Task<Location?> UpdateAsync(int id, LocationUpdateRequest request)
    {
        var location = await _db.Locations
            .Include(l => l.Items)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (location is null)
            return null;

        location.Name = request.Name;
        location.Address = request.Address;
        location.Phone = request.Phone;
        location.WebsiteUrl = request.WebsiteUrl;
        location.Lat = request.Lat;
        location.Lng = request.Lng;

        _db.LocationItems.RemoveRange(location.Items);

        foreach (var flavorId in request.FlavorIds)
        {
            foreach (var productTypeId in request.ProductTypeIds)
            {
                location.Items.Add(new LocationItem
                {
                    LocationId = id,
                    FlavorId = flavorId,
                    ProductId = productTypeId,
                });
            }
        }

        await _db.SaveChangesAsync();

        // Reload with navigation properties
        return await _db.Locations
            .Include(l => l.Items).ThenInclude(i => i.Flavor)
            .Include(l => l.Items).ThenInclude(i => i.ProductType)
            .FirstAsync(l => l.Id == id);
    }

    /// <summary>
    /// Calculates the great-circle distance between two points on Earth using the Haversine formula.
    /// </summary>
    /// <returns>Distance in miles.</returns>
    internal static double HaversineDistance(double lat1, double lng1, double lat2, double lng2)
    {
        var dLat = ToRadians(lat2 - lat1) / 2;
        var dLng = ToRadians(lng2 - lng1) / 2;
        var a = Math.Pow(Math.Sin(dLat), 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Pow(Math.Sin(dLng), 2);
        return EarthRadiusMiles * 2 * Math.Asin(Math.Sqrt(a));
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
}
