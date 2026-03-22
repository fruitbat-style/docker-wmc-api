using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WMCApi.Data;

namespace WMCApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{
    private readonly WmcDbContext _db;

    public LocationsController(WmcDbContext db)
    {
        _db = db;
    }

    [HttpGet(Name = "GetLocations")]
    public async Task<ActionResult<List<Location>>> Get(
        [FromQuery] double lat = 0,
        [FromQuery] double lng = 0,
        [FromQuery] double radius = 0,
        [FromQuery] int flavor = 0,
        [FromQuery] int product = 0)
    {
        var filterByDistance = radius > 0 && (lat != 0 || lng != 0);

        // Build a raw SQL query that does Haversine distance filtering in Postgres
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

        var locations = await _db.Locations
            .FromSqlRaw(sql, radius, flavor, product, lat, lng)
            .Include(l => l.Items)
            .ToListAsync();

        return Ok(locations);
    }
}
