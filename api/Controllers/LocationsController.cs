using Microsoft.AspNetCore.Mvc;
using WMCApi.Services;

namespace WMCApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{
    private readonly ILocationService _locationService;

    public LocationsController(ILocationService locationService)
    {
        _locationService = locationService;
    }

    [HttpGet(Name = "GetLocations")]
    public async Task<ActionResult<List<Location>>> Get(
        [FromQuery] double lat = 0,
        [FromQuery] double lng = 0,
        [FromQuery] double radius = 0,
        [FromQuery] int flavor = 0,
        [FromQuery] int product = 0)
    {
        if (lat < -90 || lat > 90)
            return BadRequest("lat must be between -90 and 90.");
        if (lng < -180 || lng > 180)
            return BadRequest("lng must be between -180 and 180.");
        if (radius < 0)
            return BadRequest("radius must be a positive number.");
        if (flavor < 0)
            return BadRequest("flavor must be a positive integer.");
        if (product < 0)
            return BadRequest("product must be a positive integer.");

        var locations = await _locationService.SearchAsync(lat, lng, radius, flavor, product);
        return Ok(locations);
    }

    [HttpGet("filters", Name = "GetFilters")]
    public async Task<ActionResult<FiltersResponse>> GetFilters()
    {
        var filters = await _locationService.GetFiltersAsync();
        return Ok(filters);
    }
}
