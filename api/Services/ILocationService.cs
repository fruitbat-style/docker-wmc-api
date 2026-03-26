namespace WMCApi.Services;

public interface ILocationService
{
    Task<List<Location>> SearchAsync(double lat, double lng, double radius, int[] flavors, int[] products);
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
