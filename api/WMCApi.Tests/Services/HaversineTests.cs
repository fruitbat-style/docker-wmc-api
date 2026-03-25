using WMCApi.Services;

namespace WMCApi.Tests.Services;

public class HaversineTests
{
    [Fact]
    public void HaversineDistance_SamePoint_ReturnsZero()
    {
        var distance = LocationService.HaversineDistance(47.6062, -122.3321, 47.6062, -122.3321);
        Assert.Equal(0, distance, precision: 5);
    }

    [Fact]
    public void HaversineDistance_SeattleToPortland_ApproximatelyCorrect()
    {
        // Seattle to Portland is ~145-175 miles depending on exact points
        var distance = LocationService.HaversineDistance(47.6062, -122.3321, 45.5152, -122.6784);
        Assert.InRange(distance, 140, 180);
    }

    [Fact]
    public void HaversineDistance_SeattleToSanFrancisco_ApproximatelyCorrect()
    {
        // Seattle to SF is ~680 miles
        var distance = LocationService.HaversineDistance(47.6062, -122.3321, 37.7749, -122.4194);
        Assert.InRange(distance, 670, 700);
    }

    [Fact]
    public void HaversineDistance_IsSymmetric()
    {
        var d1 = LocationService.HaversineDistance(47.6062, -122.3321, 37.7749, -122.4194);
        var d2 = LocationService.HaversineDistance(37.7749, -122.4194, 47.6062, -122.3321);
        Assert.Equal(d1, d2, precision: 10);
    }

    [Fact]
    public void HaversineDistance_Antipodal_ReturnsHalfCircumference()
    {
        // North pole to South pole = ~12,430 miles (half Earth circumference)
        var distance = LocationService.HaversineDistance(90, 0, -90, 0);
        Assert.InRange(distance, 12400, 12500);
    }

    [Fact]
    public void HaversineDistance_AcrossDateLine_CorrectResult()
    {
        // Tokyo (35.6762, 139.6503) to Honolulu (21.3069, -157.8583)
        var distance = LocationService.HaversineDistance(35.6762, 139.6503, 21.3069, -157.8583);
        Assert.InRange(distance, 3800, 3900);
    }

    [Fact]
    public void EarthRadiusMiles_IsCorrectConstant()
    {
        Assert.Equal(3958.8, LocationService.EarthRadiusMiles);
    }
}
