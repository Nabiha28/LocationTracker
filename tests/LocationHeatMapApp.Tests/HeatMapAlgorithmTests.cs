using LocationHeatMapApp.Models;
using LocationHeatMapApp.Services;
using Xunit;

namespace LocationHeatMapApp.Tests;

public class HeatMapAlgorithmTests
{
    [Fact]
    public void HaversineDistance_ReturnsZero_ForSameCoordinates()
    {
        double distance = HeatMapGenerator.CalculateDistanceMeters(37.7749, -122.4194, 37.7749, -122.4194);
        Assert.Equal(0.0, distance, precision: 1);
    }

    [Fact]
    public void HaversineDistance_ReturnsAccurateMeters_ForKnownOffset()
    {
        // 0.001 deg latitude is roughly ~111 meters
        double distance = HeatMapGenerator.CalculateDistanceMeters(37.7740, -122.4194, 37.7750, -122.4194);
        Assert.InRange(distance, 110.0, 112.0);
    }

    [Fact]
    public void GenerateHeatPoints_EmptyList_ReturnsEmpty()
    {
        var config = new HeatMapConfig();
        var result = HeatMapGenerator.GenerateHeatPoints(new List<LocationPoint>(), config);
        Assert.Empty(result);
    }

    [Fact]
    public void GenerateHeatPoints_ClustersProximityPoints()
    {
        var config = new HeatMapConfig
        {
            ClusterRadiusMeters = 50.0,
            BaseRadiusMeters = 60.0,
            MinDisplayWeight = 0.01
        };

        // Create 3 points within 10 meters of each other (should collapse to 1 cluster node)
        var points = new List<LocationPoint>
        {
            new() { Latitude = 37.774900, Longitude = -122.419400, Intensity = 1.0 },
            new() { Latitude = 37.774910, Longitude = -122.419410, Intensity = 1.5 },
            new() { Latitude = 37.774920, Longitude = -122.419405, Intensity = 2.0 }
        };

        var heatPoints = HeatMapGenerator.GenerateHeatPoints(points, config);

        Assert.Single(heatPoints);
        Assert.Equal(3, heatPoints[0].SampleCount);
        Assert.Equal(1.0, heatPoints[0].Weight, precision: 2); // Normalized max weight is 1.0
    }

    [Fact]
    public void CalculateBounds_CalculatesValidCenterAndSpan()
    {
        var points = new List<LocationPoint>
        {
            new() { Latitude = 37.70, Longitude = -122.50 },
            new() { Latitude = 37.80, Longitude = -122.40 }
        };

        var bounds = HeatMapGenerator.CalculateBounds(points);

        Assert.NotNull(bounds);
        Assert.Equal(37.75, bounds.Value.CenterLat, precision: 2);
        Assert.Equal(-122.45, bounds.Value.CenterLng, precision: 2);
        Assert.True(bounds.Value.LatSpan > 0.1);
        Assert.True(bounds.Value.LngSpan > 0.1);
    }

    [Fact]
    public void ColorGradient_ReturnsExpectedInterpolation()
    {
        var gradient = HeatMapColorGradient.StandardThermal;

        // At 0.0 should be cold blue
        var cold = gradient.GetColorForIntensity(0.0);
        Assert.True(cold.Blue > cold.Red);

        // At 1.0 should be hot red
        var hot = gradient.GetColorForIntensity(1.0);
        Assert.True(hot.Red > cold.Red);
    }
}
