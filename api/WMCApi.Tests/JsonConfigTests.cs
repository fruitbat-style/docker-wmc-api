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
                {"flavor_id": 1, "flavor_name": "Original", "product_id": 2, "product_name": "Powder"}
            ]
        }
        """;
        var location = JsonSerializer.Deserialize<Location>(json, JsonConfig.SnakeCaseOptions);

        Assert.NotNull(location);
        Assert.Single(location.Items);
        Assert.Equal("Original", location.Items[0].FlavorName);
        Assert.Equal("Powder", location.Items[0].ProductName);
    }
}
