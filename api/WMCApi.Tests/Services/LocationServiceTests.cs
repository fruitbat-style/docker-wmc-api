using Microsoft.EntityFrameworkCore;
using WMCApi.Data;
using WMCApi.Services;

namespace WMCApi.Tests.Services;

public class LocationServiceTests : IDisposable
{
    private readonly WmcDbContext _db;
    private readonly LocationService _service;

    public LocationServiceTests()
    {
        var options = new DbContextOptionsBuilder<WmcDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _db = new WmcDbContext(options);
        _service = new LocationService(_db);
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    private async Task SeedTestData()
    {
        _db.Flavors.AddRange(
            new Flavor { Id = 1, Name = "Original" },
            new Flavor { Id = 2, Name = "Masala" },
            new Flavor { Id = 3, Name = "Vanilla" }
        );
        _db.ProductTypes.AddRange(
            new ProductType { Id = 1, Name = "Concentrate" },
            new ProductType { Id = 2, Name = "Powder" }
        );

        _db.Locations.AddRange(
            new Location
            {
                Id = 1, Name = "Seattle Shop", Lat = 47.6062, Lng = -122.3321,
                Items =
                [
                    new LocationItem { Id = 1, LocationId = 1, FlavorId = 1, ProductId = 1 },
                    new LocationItem { Id = 2, LocationId = 1, FlavorId = 2, ProductId = 1 },
                ]
            },
            new Location
            {
                Id = 2, Name = "Portland Shop", Lat = 45.5152, Lng = -122.6784,
                Items =
                [
                    new LocationItem { Id = 3, LocationId = 2, FlavorId = 1, ProductId = 2 },
                ]
            },
            new Location
            {
                Id = 3, Name = "San Francisco Shop", Lat = 37.7749, Lng = -122.4194,
                Items =
                [
                    new LocationItem { Id = 4, LocationId = 3, FlavorId = 3, ProductId = 2 },
                ]
            }
        );
        await _db.SaveChangesAsync();
    }

    // --- SearchAsync: no filters ---

    [Fact]
    public async Task SearchAsync_NoFilters_ReturnsAllLocations()
    {
        await SeedTestData();
        var results = await _service.SearchAsync(0, 0, 0, [], []);
        Assert.Equal(3, results.Count);
    }

    [Fact]
    public async Task SearchAsync_EmptyDatabase_ReturnsEmptyList()
    {
        var results = await _service.SearchAsync(0, 0, 0, [], []);
        Assert.Empty(results);
    }

    [Fact]
    public async Task SearchAsync_IncludesItems()
    {
        await SeedTestData();
        var results = await _service.SearchAsync(0, 0, 0, [], []);
        var seattle = results.First(l => l.Name == "Seattle Shop");
        Assert.Equal(2, seattle.Items.Count);
    }

    // --- SearchAsync: flavor filter ---

    [Fact]
    public async Task SearchAsync_SingleFlavor_ReturnsMatchingLocations()
    {
        await SeedTestData();
        var results = await _service.SearchAsync(0, 0, 0, flavors: [1], products: []);
        Assert.Equal(2, results.Count); // Seattle + Portland have FlavorId=1
        Assert.All(results, l => Assert.Contains(l.Items, i => i.FlavorId == 1));
    }

    [Fact]
    public async Task SearchAsync_MultipleFlavors_ReturnsLocationsMatchingAny()
    {
        await SeedTestData();
        var results = await _service.SearchAsync(0, 0, 0, flavors: [2, 3], products: []);
        Assert.Equal(2, results.Count); // Seattle (Masala=2) + SF (Vanilla=3)
    }

    [Fact]
    public async Task SearchAsync_FlavorFilter_ExcludesNonMatching()
    {
        await SeedTestData();
        var results = await _service.SearchAsync(0, 0, 0, flavors: [3], products: []);
        Assert.Single(results);
        Assert.Equal("San Francisco Shop", results[0].Name);
    }

    [Fact]
    public async Task SearchAsync_FlavorFilter_NoMatch_ReturnsEmpty()
    {
        await SeedTestData();
        var results = await _service.SearchAsync(0, 0, 0, flavors: [999], products: []);
        Assert.Empty(results);
    }

    [Fact]
    public async Task SearchAsync_EmptyFlavors_SkipsFlavorFilter()
    {
        await SeedTestData();
        var results = await _service.SearchAsync(0, 0, 0, flavors: [], products: []);
        Assert.Equal(3, results.Count);
    }

    // --- SearchAsync: product filter ---

    [Fact]
    public async Task SearchAsync_SingleProduct_ReturnsMatchingLocations()
    {
        await SeedTestData();
        var results = await _service.SearchAsync(0, 0, 0, flavors: [], products: [2]);
        Assert.Equal(2, results.Count); // Portland + SF have ProductId=2
        Assert.All(results, l => Assert.Contains(l.Items, i => i.ProductId == 2));
    }

    [Fact]
    public async Task SearchAsync_MultipleProducts_ReturnsLocationsMatchingAny()
    {
        await SeedTestData();
        var results = await _service.SearchAsync(0, 0, 0, flavors: [], products: [1, 2]);
        Assert.Equal(3, results.Count); // All locations match
    }

    [Fact]
    public async Task SearchAsync_ProductFilter_NoMatch_ReturnsEmpty()
    {
        await SeedTestData();
        var results = await _service.SearchAsync(0, 0, 0, flavors: [], products: [999]);
        Assert.Empty(results);
    }

    // --- SearchAsync: combined flavor + product ---

    [Fact]
    public async Task SearchAsync_FlavorAndProduct_ReturnsBothMatching()
    {
        await SeedTestData();
        // FlavorId=1 AND ProductId=1 → only Seattle (has both)
        var results = await _service.SearchAsync(0, 0, 0, flavors: [1], products: [1]);
        Assert.Single(results);
        Assert.Equal("Seattle Shop", results[0].Name);
    }

    [Fact]
    public async Task SearchAsync_FlavorAndProduct_NoOverlap_ReturnsEmpty()
    {
        await SeedTestData();
        // FlavorId=3 (SF only) AND ProductId=1 (Seattle only) → no overlap
        var results = await _service.SearchAsync(0, 0, 0, flavors: [3], products: [1]);
        Assert.Empty(results);
    }

    // --- SearchAsync: distance filter ---

    [Fact]
    public async Task SearchAsync_DistanceFilter_ReturnsNearbyLocations()
    {
        await SeedTestData();
        var results = await _service.SearchAsync(47.6062, -122.3321, radius: 5, [], []);
        Assert.Single(results);
        Assert.Equal("Seattle Shop", results[0].Name);
    }

    [Fact]
    public async Task SearchAsync_DistanceFilter_LargeRadius_ReturnsAll()
    {
        await SeedTestData();
        var results = await _service.SearchAsync(47.6062, -122.3321, radius: 1000, [], []);
        Assert.Equal(3, results.Count);
    }

    [Fact]
    public async Task SearchAsync_DistanceFilter_ZeroRadius_SkipsFilter()
    {
        await SeedTestData();
        var results = await _service.SearchAsync(47.6062, -122.3321, radius: 0, [], []);
        Assert.Equal(3, results.Count);
    }

    [Fact]
    public async Task SearchAsync_DistanceFilter_ZeroCoords_SkipsFilter()
    {
        await SeedTestData();
        var results = await _service.SearchAsync(0, 0, radius: 5, [], []);
        Assert.Equal(3, results.Count);
    }

    [Fact]
    public async Task SearchAsync_DistanceFilter_LatNonZero_LngZero_FiltersEnabled()
    {
        await SeedTestData();
        var results = await _service.SearchAsync(47.6062, 0, radius: 5, [], []);
        Assert.Empty(results);
    }

    // --- SearchAsync: distance + filters combined ---

    [Fact]
    public async Task SearchAsync_DistanceAndFlavor_CombinesFilters()
    {
        await SeedTestData();
        var results = await _service.SearchAsync(47.6062, -122.3321, radius: 5, flavors: [2], products: []);
        Assert.Single(results);
        Assert.Equal("Seattle Shop", results[0].Name);
    }

    // --- GetFiltersAsync ---

    [Fact]
    public async Task GetFiltersAsync_ReturnsDistinctFlavors()
    {
        await SeedTestData();
        var filters = await _service.GetFiltersAsync();
        Assert.Equal(3, filters.Flavors.Count);
    }

    [Fact]
    public async Task GetFiltersAsync_ReturnsDistinctProducts()
    {
        await SeedTestData();
        var filters = await _service.GetFiltersAsync();
        Assert.Equal(2, filters.ProductTypes.Count);
    }

    [Fact]
    public async Task GetFiltersAsync_FlavorsOrderedById()
    {
        await SeedTestData();
        var filters = await _service.GetFiltersAsync();
        Assert.Equal(1, filters.Flavors[0].Id);
        Assert.Equal(2, filters.Flavors[1].Id);
        Assert.Equal(3, filters.Flavors[2].Id);
    }

    [Fact]
    public async Task GetFiltersAsync_ProductsOrderedById()
    {
        await SeedTestData();
        var filters = await _service.GetFiltersAsync();
        Assert.Equal(1, filters.ProductTypes[0].Id);
        Assert.Equal(2, filters.ProductTypes[1].Id);
    }

    [Fact]
    public async Task GetFiltersAsync_EmptyDatabase_ReturnsEmptyLists()
    {
        var filters = await _service.GetFiltersAsync();
        Assert.Empty(filters.Flavors);
        Assert.Empty(filters.ProductTypes);
    }

    [Fact]
    public async Task GetFiltersAsync_FlavorNames_AreCorrect()
    {
        await SeedTestData();
        var filters = await _service.GetFiltersAsync();
        Assert.Equal("Original", filters.Flavors[0].Name);
        Assert.Equal("Masala", filters.Flavors[1].Name);
        Assert.Equal("Vanilla", filters.Flavors[2].Name);
    }
}
