using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace WMCApi.Data;

public static class SeedData
{

    public static async Task SeedLocationsAsync(WmcDbContext db, string contentRootPath, ILogger logger)
    {
        try
        {
            if (await db.Locations.AnyAsync())
                return;

            var path = Path.Combine(contentRootPath, "Data", "locations-data.json");
            if (!File.Exists(path))
            {
                logger.LogWarning("Seed data file not found at {Path}", path);
                return;
            }

            await using var stream = File.OpenRead(path);
            var locations = await JsonSerializer.DeserializeAsync<List<Location>>(stream, JsonConfig.SnakeCaseOptions);

            if (locations is null || locations.Count == 0)
            {
                logger.LogWarning("Seed data file was empty or could not be parsed");
                return;
            }

            db.Locations.AddRange(locations);
            await db.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} locations", locations.Count);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Failed to parse seed data JSON");
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Failed to save seed data to database");
        }
    }
}
