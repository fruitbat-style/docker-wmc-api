namespace WMCApi.Services;

public interface ILocationService
{
    Task<List<Location>> SearchAsync(double lat, double lng, double radius, int[] flavors, int[] products);
    Task<Location> CreateAsync(LocationUpdateRequest request);
    Task<Location?> UpdateAsync(int id, LocationUpdateRequest request);
    Task<FiltersResponse> GetFiltersAsync();
}

public class FiltersResponse
{
    public List<FilterOption> Flavors { get; set; } = [];
    public List<FilterOption> ProductTypes { get; set; } = [];
}

public class FilterOption
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public class LocationUpdateRequest
{
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";
    public string Phone { get; set; } = "";
    public string WebsiteUrl { get; set; } = "";
    public double Lat { get; set; }
    public double Lng { get; set; }
    public int[] FlavorIds { get; set; } = [];
    public int[] ProductTypeIds { get; set; } = [];
}
