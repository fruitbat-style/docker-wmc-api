using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using WMCApi.Data;

namespace WMCApi.Tests.Data;

public class SeedDataTests : IDisposable
{
    private readonly WmcDbContext _db;
    private readonly Mock<ILogger> _loggerMock;
    private readonly string _tempDir;

    public SeedDataTests()
    {
        var options = new DbContextOptionsBuilder<WmcDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _db = new WmcDbContext(options);
        _loggerMock = new Mock<ILogger>();
        _tempDir = Path.Combine(Path.GetTempPath(), $"wmc-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(Path.Combine(_tempDir, "Data"));
    }

    public void Dispose()
    {
        _db.Dispose();
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private void WriteSeedFile(string json)
    {
        File.WriteAllText(Path.Combine(_tempDir, "Data", "locations-data.json"), json);
    }

    private static string BuildSeedJson(List<object> locations)
    {
        return JsonSerializer.Serialize(locations, JsonConfig.SnakeCaseOptions);
    }

    // --- Early exit: database already seeded ---

    [Fact]
    public async Task SeedLocationsAsync_DatabaseHasData_SkipsSeeding()
    {
        _db.Locations.Add(new Location { Id = 1, Name = "Existing" });
        await _db.SaveChangesAsync();

        await SeedData.SeedLocationsAsync(_db, _tempDir, _loggerMock.Object);

        Assert.Single(await _db.Locations.ToListAsync());
        Assert.Equal("Existing", (await _db.Locations.FirstAsync()).Name);
    }

    // --- File not found ---

    [Fact]
    public async Task SeedLocationsAsync_FileNotFound_LogsWarning()
    {
        // _tempDir/Data/locations-data.json does not exist
        await SeedData.SeedLocationsAsync(_db, _tempDir, _loggerMock.Object);

        Assert.Empty(await _db.Locations.ToListAsync());
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not found")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    // --- Valid seed data ---

    [Fact]
    public async Task SeedLocationsAsync_ValidData_SeedsLocations()
    {
        var locations = new List<object>
        {
            new {
                Name = "Shop A", Address = "123 Main St", Lat = 47.6, Lng = -122.3,
                Phone = "", PhotoUrl = "", WebsiteUrl = "",
                Items = new[] { new { FlavorId = 1, FlavorName = "Chai", ProductId = 1, ProductName = "Concentrate" } }
            },
            new {
                Name = "Shop B", Address = "456 Oak Ave", Lat = 45.5, Lng = -122.6,
                Phone = "", PhotoUrl = "", WebsiteUrl = "",
                Items = new[] { new { FlavorId = 1, FlavorName = "Chai", ProductId = 2, ProductName = "Powder" } }
            },
        };
        WriteSeedFile(BuildSeedJson(locations));

        await SeedData.SeedLocationsAsync(_db, _tempDir, _loggerMock.Object);

        var seeded = await _db.Locations.ToListAsync();
        Assert.Equal(2, seeded.Count);
    }

    [Fact]
    public async Task SeedLocationsAsync_ValidData_SeedsFlavorsAndProductTypes()
    {
        var locations = new List<object>
        {
            new {
                Name = "Shop A", Address = "", Lat = 0.0, Lng = 0.0,
                Phone = "", PhotoUrl = "", WebsiteUrl = "",
                Items = new[]
                {
                    new { FlavorId = 1, FlavorName = "Chai", ProductId = 1, ProductName = "Concentrate" },
                    new { FlavorId = 2, FlavorName = "Vanilla", ProductId = 2, ProductName = "Powder" },
                }
            },
        };
        WriteSeedFile(BuildSeedJson(locations));

        await SeedData.SeedLocationsAsync(_db, _tempDir, _loggerMock.Object);

        var flavors = await _db.Flavors.OrderBy(f => f.Id).ToListAsync();
        Assert.Equal(2, flavors.Count);
        Assert.Equal("Chai", flavors[0].Name);
        Assert.Equal("Vanilla", flavors[1].Name);

        var productTypes = await _db.ProductTypes.OrderBy(p => p.Id).ToListAsync();
        Assert.Equal(2, productTypes.Count);
        Assert.Equal("Concentrate", productTypes[0].Name);
        Assert.Equal("Powder", productTypes[1].Name);
    }

    [Fact]
    public async Task SeedLocationsAsync_ValidData_LogsCount()
    {
        var locations = new List<object>
        {
            new {
                Name = "Shop A", Address = "", Lat = 0.0, Lng = 0.0,
                Phone = "", PhotoUrl = "", WebsiteUrl = "",
                Items = Array.Empty<object>()
            },
        };
        WriteSeedFile(BuildSeedJson(locations));

        await SeedData.SeedLocationsAsync(_db, _tempDir, _loggerMock.Object);

        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("1")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    // --- Empty / null data ---

    [Fact]
    public async Task SeedLocationsAsync_EmptyArray_LogsWarning()
    {
        WriteSeedFile("[]");

        await SeedData.SeedLocationsAsync(_db, _tempDir, _loggerMock.Object);

        Assert.Empty(await _db.Locations.ToListAsync());
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("empty")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SeedLocationsAsync_NullJson_LogsWarning()
    {
        WriteSeedFile("null");

        await SeedData.SeedLocationsAsync(_db, _tempDir, _loggerMock.Object);

        Assert.Empty(await _db.Locations.ToListAsync());
    }

    // --- Invalid JSON ---

    [Fact]
    public async Task SeedLocationsAsync_InvalidJson_LogsError()
    {
        WriteSeedFile("not valid json {{{");

        await SeedData.SeedLocationsAsync(_db, _tempDir, _loggerMock.Object);

        Assert.Empty(await _db.Locations.ToListAsync());
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<JsonException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    // --- Idempotency ---

    [Fact]
    public async Task SeedLocationsAsync_CalledTwice_OnlySeedsOnce()
    {
        var locations = new List<object>
        {
            new {
                Name = "Shop A", Address = "", Lat = 0.0, Lng = 0.0,
                Phone = "", PhotoUrl = "", WebsiteUrl = "",
                Items = Array.Empty<object>()
            },
        };
        WriteSeedFile(BuildSeedJson(locations));

        await SeedData.SeedLocationsAsync(_db, _tempDir, _loggerMock.Object);
        await SeedData.SeedLocationsAsync(_db, _tempDir, _loggerMock.Object);

        Assert.Single(await _db.Locations.ToListAsync());
    }
}
