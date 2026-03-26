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

    public async Task<List<Location>> SearchAsync(double lat, double lng, double radius, int flavor, int product)
    {
        var query = _db.Locations.Include(l => l.Items).AsQueryable();

        if (flavor > 0)
            query = query.Where(l => l.Items.Any(i => i.FlavorId == flavor));

        if (product > 0)
            query = query.Where(l => l.Items.Any(i => i.ProductId == product));

        var locations = await query.ToListAsync();

        var filterByDistance = radius > 0 && (lat != 0 || lng != 0);
        if (filterByDistance)
            locations = locations.Where(l => HaversineDistance(lat, lng, l.Lat, l.Lng) <= radius).ToList();

        return locations;
    }

    public async Task<FiltersResponse> GetFiltersAsync()
    {
        var flavors = await _db.LocationItems
            .Select(i => new { i.FlavorId, i.FlavorName })
            .Distinct()
            .OrderBy(f => f.FlavorId)
            .ToListAsync();

        var products = await _db.LocationItems
            .Select(i => new { i.ProductId, i.ProductName })
            .Distinct()
            .OrderBy(p => p.ProductId)
            .ToListAsync();

        return new FiltersResponse
        {
            Flavors = flavors.Select(f => new FilterOption { Id = f.FlavorId, Name = f.FlavorName }).ToList(),
            ProductTypes = products.Select(p => new FilterOption { Id = p.ProductId, Name = p.ProductName }).ToList(),
        };
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
