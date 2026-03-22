using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace WMCApi.Data;

public static class SeedData
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public static async Task SeedLocationsAsync(WmcDbContext db, string contentRootPath)
    {
        if (await db.Locations.AnyAsync())
            return;

        var path = Path.Combine(contentRootPath, "Data", "locations-data.json");
        await using var stream = File.OpenRead(path);
        var locations = await JsonSerializer.DeserializeAsync<List<Location>>(stream, JsonOptions);

        if (locations is null)
            return;

        db.Locations.AddRange(locations);
        await db.SaveChangesAsync();
    }
}
