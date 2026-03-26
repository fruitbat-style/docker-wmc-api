using System.Text.Json;

namespace WMCApi.Tests;

public class JsonConfigTests
{
    [Fact]
    public void SnakeCaseOptions_UsesSnakeCaseLowerPolicy()
    {
        Assert.Equal(JsonNamingPolicy.SnakeCaseLower, JsonConfig.SnakeCaseOptions.PropertyNamingPolicy);
    }

    [Fact]
    public void SnakeCaseOptions_SerializesPascalCaseToSnakeCase()
    {
        var obj = new { WebsiteUrl = "https://example.com", PhotoUrl = "img.png" };
        var json = JsonSerializer.Serialize(obj, JsonConfig.SnakeCaseOptions);

        Assert.Contains("\"website_url\"", json);
        Assert.Contains("\"photo_url\"", json);
        Assert.DoesNotContain("WebsiteUrl", json);
    }

    [Fact]
    public void SnakeCaseOptions_DeserializesSnakeCaseToPascalCase()
    {
        var json = """{"name":"Test Shop","website_url":"https://example.com","photo_url":"img.png"}""";
        var location = JsonSerializer.Deserialize<Location>(json, JsonConfig.SnakeCaseOptions);

        Assert.NotNull(location);
        Assert.Equal("Test Shop", location.Name);
        Assert.Equal("https://example.com", location.WebsiteUrl);
        Assert.Equal("img.png", location.PhotoUrl);
    }

    [Fact]
    public void SnakeCaseOptions_DeserializesLocationWithItems()
    {
        var json = """
        {
            "name": "Test",
            "items": [
                {"flavor_id": 1, "product_id": 2}
            ]
        }
        """;
        var location = JsonSerializer.Deserialize<Location>(json, JsonConfig.SnakeCaseOptions);

        Assert.NotNull(location);
        Assert.Single(location.Items);
        Assert.Equal(1, location.Items[0].FlavorId);
        Assert.Equal(2, location.Items[0].ProductId);
    }

    [Fact]
    public void SnakeCaseOptions_SerializesFlavorToSnakeCase()
    {
        var item = new LocationItem
        {
            Id = 1,
            FlavorId = 1,
            ProductId = 2,
            Flavor = new Flavor { Id = 1, Name = "Original" },
            ProductType = new ProductType { Id = 2, Name = "Powder" },
        };
        var json = JsonSerializer.Serialize(item, JsonConfig.SnakeCaseOptions);

        Assert.Contains("\"flavor_id\"", json);
        Assert.Contains("\"product_id\"", json);
        Assert.Contains("\"flavor\"", json);
        Assert.Contains("\"product_type\"", json);
    }
}
