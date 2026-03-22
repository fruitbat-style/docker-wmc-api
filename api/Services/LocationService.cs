using Microsoft.EntityFrameworkCore;
using WMCApi.Data;

namespace WMCApi.Services;

public class LocationService : ILocationService
{
    /// <summary>Mean radius of the Earth in miles, used for Haversine distance calculation.</summary>
    private const double EarthRadiusMiles = 3958.8;

    private readonly WmcDbContext _db;

    public LocationService(WmcDbContext db)
    {
        _db = db;
    }

    public async Task<List<Location>> SearchAsync(double lat, double lng, double radius, int flavor, int product)
    {
        var filterByDistance = radius > 0 && (lat != 0 || lng != 0);

        var sql = """
            SELECT l.*
            FROM locations l
            WHERE 1=1
            """;

        if (flavor > 0)
            sql += """
                 AND EXISTS (
                    SELECT 1 FROM location_items i
                    WHERE i."LocationId" = l."Id" AND i."FlavorId" = {1}
                )
                """;

        if (product > 0)
            sql += """
                 AND EXISTS (
                    SELECT 1 FROM location_items i
                    WHERE i."LocationId" = l."Id" AND i."ProductId" = {2}
                )
                """;

        if (filterByDistance)
            sql += """
                 AND (
                    3958.8 * 2 * ASIN(SQRT(
                        POWER(SIN(RADIANS(l."Lat" - {3}) / 2), 2) +
                        COS(RADIANS({3})) * COS(RADIANS(l."Lat")) *
                        POWER(SIN(RADIANS(l."Lng" - {4}) / 2), 2)
                    ))
                ) <= {0}
                """;

        return await _db.Locations
            .FromSqlRaw(sql, radius, flavor, product, lat, lng)
            .Include(l => l.Items)
            .ToListAsync();
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
}
