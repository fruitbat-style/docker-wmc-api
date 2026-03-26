using Microsoft.AspNetCore.Authorization;
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
        [FromQuery] string flavors = "",
        [FromQuery] string products = "")
    {
        if (lat < -90 || lat > 90)
            return BadRequest("lat must be between -90 and 90.");
        if (lng < -180 || lng > 180)
            return BadRequest("lng must be between -180 and 180.");
        if (radius < 0)
            return BadRequest("radius must be a positive number.");

        if (!TryParseIds(flavors, out var flavorIds))
            return BadRequest("flavors must be comma-separated positive integers.");
        if (!TryParseIds(products, out var productIds))
            return BadRequest("products must be comma-separated positive integers.");

        var locations = await _locationService.SearchAsync(lat, lng, radius, flavorIds, productIds);
        return Ok(locations);
    }

    [Authorize]
    [HttpPost(Name = "CreateLocation")]
    public async Task<ActionResult<Location>> Create([FromBody] LocationUpdateRequest request)
    {
        var created = await _locationService.CreateAsync(request);
        return CreatedAtRoute("GetLocations", new { id = created.Id }, created);
    }

    [Authorize]
    [HttpPut("{id}", Name = "UpdateLocation")]
    public async Task<ActionResult<Location>> Update(int id, [FromBody] LocationUpdateRequest request)
    {
        var updated = await _locationService.UpdateAsync(id, request);
        if (updated is null)
            return NotFound();
        return Ok(updated);
    }

    [HttpGet("filters", Name = "GetFilters")]
    public async Task<ActionResult<FiltersResponse>> GetFilters()
    {
        var filters = await _locationService.GetFiltersAsync();
        return Ok(filters);
    }

    private static bool TryParseIds(string input, out int[] ids)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            ids = [];
            return true;
        }

        var parts = input.Split(',', StringSplitOptions.RemoveEmptyEntries);
        var result = new int[parts.Length];
        for (var i = 0; i < parts.Length; i++)
        {
            if (!int.TryParse(parts[i].Trim(), out var id) || id < 1)
            {
                ids = [];
                return false;
            }
            result[i] = id;
        }
        ids = result;
        return true;
    }
}
