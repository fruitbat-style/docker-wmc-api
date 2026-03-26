using System.Text.Json.Serialization;

namespace WMCApi;

public class Location
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";
    public double Lat { get; set; }
    public double Lng { get; set; }
    public string Phone { get; set; } = "";
    public string PhotoUrl { get; set; } = "";
    public string WebsiteUrl { get; set; } = "";
    public List<LocationItem> Items { get; set; } = [];
}

public class LocationItem
{
    public int Id { get; set; }
    public int LocationId { get; set; }
    public int FlavorId { get; set; }
    public int ProductId { get; set; }

    [JsonIgnore]
    public Location? Location { get; set; }

    public Flavor? Flavor { get; set; }
    public ProductType? ProductType { get; set; }
}

public class Flavor
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public class ProductType
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}
