using Microsoft.AspNetCore.Mvc;
using Moq;
using WMCApi.Controllers;
using WMCApi.Services;

namespace WMCApi.Tests.Controllers;

public class LocationsControllerTests
{
    private readonly Mock<ILocationService> _serviceMock;
    private readonly LocationsController _controller;

    public LocationsControllerTests()
    {
        _serviceMock = new Mock<ILocationService>();
        _controller = new LocationsController(_serviceMock.Object);
    }

    // --- lat validation ---

    [Theory]
    [InlineData(-91)]
    [InlineData(91)]
    [InlineData(-200)]
    [InlineData(200)]
    public async Task Get_InvalidLat_ReturnsBadRequest(double lat)
    {
        var result = await _controller.Get(lat: lat);
        var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Contains("lat", bad.Value!.ToString()!);
    }

    [Theory]
    [InlineData(-90)]
    [InlineData(0)]
    [InlineData(90)]
    public async Task Get_ValidLat_DoesNotReturnBadRequest(double lat)
    {
        _serviceMock.Setup(s => s.SearchAsync(lat, 0, 0, 0, 0))
            .ReturnsAsync([]);

        var result = await _controller.Get(lat: lat);
        Assert.IsType<OkObjectResult>(result.Result);
    }

    // --- lng validation ---

    [Theory]
    [InlineData(-181)]
    [InlineData(181)]
    public async Task Get_InvalidLng_ReturnsBadRequest(double lng)
    {
        var result = await _controller.Get(lng: lng);
        var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Contains("lng", bad.Value!.ToString()!);
    }

    [Theory]
    [InlineData(-180)]
    [InlineData(0)]
    [InlineData(180)]
    public async Task Get_ValidLng_DoesNotReturnBadRequest(double lng)
    {
        _serviceMock.Setup(s => s.SearchAsync(0, lng, 0, 0, 0))
            .ReturnsAsync([]);

        var result = await _controller.Get(lng: lng);
        Assert.IsType<OkObjectResult>(result.Result);
    }

    // --- radius validation ---

    [Fact]
    public async Task Get_NegativeRadius_ReturnsBadRequest()
    {
        var result = await _controller.Get(radius: -1);
        var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Contains("radius", bad.Value!.ToString()!);
    }

    [Fact]
    public async Task Get_ZeroRadius_IsValid()
    {
        _serviceMock.Setup(s => s.SearchAsync(0, 0, 0, 0, 0))
            .ReturnsAsync([]);

        var result = await _controller.Get(radius: 0);
        Assert.IsType<OkObjectResult>(result.Result);
    }

    // --- flavor validation ---

    [Fact]
    public async Task Get_NegativeFlavor_ReturnsBadRequest()
    {
        var result = await _controller.Get(flavor: -1);
        var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Contains("flavor", bad.Value!.ToString()!);
    }

    // --- product validation ---

    [Fact]
    public async Task Get_NegativeProduct_ReturnsBadRequest()
    {
        var result = await _controller.Get(product: -1);
        var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Contains("product", bad.Value!.ToString()!);
    }

    // --- successful search ---

    [Fact]
    public async Task Get_ValidParams_ReturnsOkWithLocations()
    {
        var expected = new List<Location>
        {
            new() { Id = 1, Name = "Test", Lat = 47.6, Lng = -122.3 }
        };
        _serviceMock.Setup(s => s.SearchAsync(47.6, -122.3, 5, 0, 0))
            .ReturnsAsync(expected);

        var result = await _controller.Get(lat: 47.6, lng: -122.3, radius: 5);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var locations = Assert.IsType<List<Location>>(ok.Value);
        Assert.Single(locations);
        Assert.Equal("Test", locations[0].Name);
    }

    [Fact]
    public async Task Get_DefaultParams_CallsServiceWithZeros()
    {
        _serviceMock.Setup(s => s.SearchAsync(0, 0, 0, 0, 0))
            .ReturnsAsync([]);

        await _controller.Get();
        _serviceMock.Verify(s => s.SearchAsync(0, 0, 0, 0, 0), Times.Once);
    }

    // --- validation priority (first invalid param wins) ---

    [Fact]
    public async Task Get_MultipleBadParams_LatCheckedFirst()
    {
        var result = await _controller.Get(lat: 100, lng: 200);
        var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Contains("lat", bad.Value!.ToString()!);
    }

    // --- GetFilters ---

    [Fact]
    public async Task GetFilters_ReturnsOkWithFilters()
    {
        var expected = new FiltersResponse
        {
            Flavors = [new FilterOption { Id = 1, Name = "Original" }],
            ProductTypes = [new FilterOption { Id = 1, Name = "Concentrate" }],
        };
        _serviceMock.Setup(s => s.GetFiltersAsync()).ReturnsAsync(expected);

        var result = await _controller.GetFilters();
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var filters = Assert.IsType<FiltersResponse>(ok.Value);
        Assert.Single(filters.Flavors);
        Assert.Single(filters.ProductTypes);
    }
}
