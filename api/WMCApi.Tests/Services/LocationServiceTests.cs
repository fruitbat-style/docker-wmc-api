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
        _db.Locations.AddRange(
            new Location
            {
                Id = 1, Name = "Seattle Shop", Lat = 47.6062, Lng = -122.3321,
                Items =
                [
                    new LocationItem { Id = 1, LocationId = 1, FlavorId = 1, FlavorName = "Original", ProductId = 1, ProductName = "Concentrate" },
                    new LocationItem { Id = 2, LocationId = 1, FlavorId = 2, FlavorName = "Masala", ProductId = 1, ProductName = "Concentrate" },
                ]
            },
            new Location
            {
                Id = 2, Name = "Portland Shop", Lat = 45.5152, Lng = -122.6784,
                Items =
                [
                    new LocationItem { Id = 3, LocationId = 2, FlavorId = 1, FlavorName = "Original", ProductId = 2, ProductName = "Powder" },
                ]
            },
            new Location
            {
                Id = 3, Name = "San Francisco Shop", Lat = 37.7749, Lng = -122.4194,
                Items =
                [
                    new LocationItem { Id = 4, LocationId = 3, FlavorId = 3, FlavorName = "Vanilla", ProductId = 2, ProductName = "Powder" },
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
        var results = await _service.SearchAsync(0, 0, 0, 0, 0);
        Assert.Equal(3, results.Count);
    }

    [Fact]
    public async Task SearchAsync_EmptyDatabase_ReturnsEmptyList()
    {
        var results = await _service.SearchAsync(0, 0, 0, 0, 0);
        Assert.Empty(results);
    }

    [Fact]
    public async Task SearchAsync_IncludesItems()
    {
        await SeedTestData();
        var results = await _service.SearchAsync(0, 0, 0, 0, 0);
        var seattle = results.First(l => l.Name == "Seattle Shop");
        Assert.Equal(2, seattle.Items.Count);
    }

    // --- SearchAsync: flavor filter ---

    [Fact]
    public async Task SearchAsync_FlavorFilter_ReturnsMatchingLocations()
    {
        await SeedTestData();
        var results = await _service.SearchAsync(0, 0, 0, flavor: 1, product: 0);
        Assert.Equal(2, results.Count); // Seattle + Portland have FlavorId=1
        Assert.All(results, l => Assert.Contains(l.Items, i => i.FlavorId == 1));
    }

    [Fact]
    public async Task SearchAsync_FlavorFilter_ExcludesNonMatching()
    {
        await SeedTestData();
        var results = await _service.SearchAsync(0, 0, 0, flavor: 3, product: 0);
        Assert.Single(results);
        Assert.Equal("San Francisco Shop", results[0].Name);
    }

    [Fact]
    public async Task SearchAsync_FlavorFilter_NoMatch_ReturnsEmpty()
    {
        await SeedTestData();
        var results = await _service.SearchAsync(0, 0, 0, flavor: 999, product: 0);
        Assert.Empty(results);
    }

    [Fact]
    public async Task SearchAsync_FlavorZero_SkipsFlavorFilter()
    {
        await SeedTestData();
        var results = await _service.SearchAsync(0, 0, 0, flavor: 0, product: 0);
        Assert.Equal(3, results.Count);
    }

    // --- SearchAsync: product filter ---

    [Fact]
    public async Task SearchAsync_ProductFilter_ReturnsMatchingLocations()
    {
        await SeedTestData();
        var results = await _service.SearchAsync(0, 0, 0, flavor: 0, product: 2);
        Assert.Equal(2, results.Count); // Portland + SF have ProductId=2
        Assert.All(results, l => Assert.Contains(l.Items, i => i.ProductId == 2));
    }

    [Fact]
    public async Task SearchAsync_ProductFilter_NoMatch_ReturnsEmpty()
    {
        await SeedTestData();
        var results = await _service.SearchAsync(0, 0, 0, flavor: 0, product: 999);
        Assert.Empty(results);
    }

    // --- SearchAsync: combined flavor + product ---

    [Fact]
    public async Task SearchAsync_FlavorAndProduct_ReturnsBothMatching()
    {
        await SeedTestData();
        // FlavorId=1 AND ProductId=1 → only Seattle (has both)
        var results = await _service.SearchAsync(0, 0, 0, flavor: 1, product: 1);
        Assert.Single(results);
        Assert.Equal("Seattle Shop", results[0].Name);
    }

    [Fact]
    public async Task SearchAsync_FlavorAndProduct_NoOverlap_ReturnsEmpty()
    {
        await SeedTestData();
        // FlavorId=3 (SF only) AND ProductId=1 (Seattle only) → no overlap
        var results = await _service.SearchAsync(0, 0, 0, flavor: 3, product: 1);
        Assert.Empty(results);
    }

    // --- SearchAsync: distance filter ---

    [Fact]
    public async Task SearchAsync_DistanceFilter_ReturnsNearbyLocations()
    {
        await SeedTestData();
        // Search near Seattle with 5-mile radius — only Seattle Shop is within range
        var results = await _service.SearchAsync(47.6062, -122.3321, radius: 5, flavor: 0, product: 0);
        Assert.Single(results);
        Assert.Equal("Seattle Shop", results[0].Name);
    }

    [Fact]
    public async Task SearchAsync_DistanceFilter_LargeRadius_ReturnsAll()
    {
        await SeedTestData();
        // Seattle to SF is ~680 miles, so 1000-mile radius gets everything
        var results = await _service.SearchAsync(47.6062, -122.3321, radius: 1000, flavor: 0, product: 0);
        Assert.Equal(3, results.Count);
    }

    [Fact]
    public async Task SearchAsync_DistanceFilter_ZeroRadius_SkipsFilter()
    {
        await SeedTestData();
        var results = await _service.SearchAsync(47.6062, -122.3321, radius: 0, flavor: 0, product: 0);
        Assert.Equal(3, results.Count);
    }

    [Fact]
    public async Task SearchAsync_DistanceFilter_ZeroCoords_SkipsFilter()
    {
        await SeedTestData();
        // radius > 0 but both lat and lng are 0 → filterByDistance = false
        var results = await _service.SearchAsync(0, 0, radius: 5, flavor: 0, product: 0);
        Assert.Equal(3, results.Count);
    }

    [Fact]
    public async Task SearchAsync_DistanceFilter_LatNonZero_LngZero_FiltersEnabled()
    {
        await SeedTestData();
        // lat != 0, lng = 0 → filterByDistance = true
        var results = await _service.SearchAsync(47.6062, 0, radius: 5, flavor: 0, product: 0);
        // No locations near (47.6, 0) within 5 miles
        Assert.Empty(results);
    }

    // --- SearchAsync: distance + filters combined ---

    [Fact]
    public async Task SearchAsync_DistanceAndFlavor_CombinesFilters()
    {
        await SeedTestData();
        // Near Seattle (5mi) + FlavorId=2 (Masala, only Seattle)
        var results = await _service.SearchAsync(47.6062, -122.3321, radius: 5, flavor: 2, product: 0);
        Assert.Single(results);
        Assert.Equal("Seattle Shop", results[0].Name);
    }

    // --- GetFiltersAsync ---

    [Fact]
    public async Task GetFiltersAsync_ReturnsDistinctFlavors()
    {
        await SeedTestData();
        var filters = await _service.GetFiltersAsync();
        Assert.Equal(3, filters.Flavors.Count); // Original, Masala, Vanilla
    }

    [Fact]
    public async Task GetFiltersAsync_ReturnsDistinctProducts()
    {
        await SeedTestData();
        var filters = await _service.GetFiltersAsync();
        Assert.Equal(2, filters.ProductTypes.Count); // Concentrate, Powder
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
    public async Task GetFiltersAsync_DuplicateFlavors_ReturnsDistinct()
    {
        // Seattle and Portland both have FlavorId=1 "Original"
        await SeedTestData();
        var filters = await _service.GetFiltersAsync();
        Assert.Single(filters.Flavors, f => f.Name == "Original");
    }
}
