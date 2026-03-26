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
            var seedLocations = await JsonSerializer.DeserializeAsync<List<SeedLocation>>(stream, JsonConfig.SnakeCaseOptions);

            if (seedLocations is null || seedLocations.Count == 0)
            {
                logger.LogWarning("Seed data file was empty or could not be parsed");
                return;
            }

            // Extract and seed unique flavors
            var flavors = seedLocations
                .SelectMany(l => l.Items)
                .Select(i => new { i.FlavorId, i.FlavorName })
                .DistinctBy(f => f.FlavorId)
                .OrderBy(f => f.FlavorId)
                .Select(f => new Flavor { Id = f.FlavorId, Name = f.FlavorName })
                .ToList();
            db.Flavors.AddRange(flavors);

            // Extract and seed unique product types
            var productTypes = seedLocations
                .SelectMany(l => l.Items)
                .Select(i => new { i.ProductId, i.ProductName })
                .DistinctBy(p => p.ProductId)
                .OrderBy(p => p.ProductId)
                .Select(p => new ProductType { Id = p.ProductId, Name = p.ProductName })
                .ToList();
            db.ProductTypes.AddRange(productTypes);

            // Map seed data to Location entities
            var locations = seedLocations.Select(sl => new Location
            {
                Name = sl.Name,
                Address = sl.Address,
                Lat = sl.Lat,
                Lng = sl.Lng,
                Phone = sl.Phone,
                PhotoUrl = sl.PhotoUrl,
                WebsiteUrl = sl.WebsiteUrl,
                Items = sl.Items.Select(si => new LocationItem
                {
                    FlavorId = si.FlavorId,
                    ProductId = si.ProductId,
                }).ToList(),
            }).ToList();

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

    private class SeedLocation
    {
        public string Name { get; set; } = "";
        public string Address { get; set; } = "";
        public double Lat { get; set; }
        public double Lng { get; set; }
        public string Phone { get; set; } = "";
        public string PhotoUrl { get; set; } = "";
        public string WebsiteUrl { get; set; } = "";
        public List<SeedItem> Items { get; set; } = [];
    }

    private class SeedItem
    {
        public int FlavorId { get; set; }
        public string FlavorName { get; set; } = "";
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
    }
}
