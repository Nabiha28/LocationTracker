namespace LocationHeatMapApp.Models;

/// <summary>
/// Represents a consolidated point for heat map visualization with computed weight and density.
/// </summary>
public class HeatPoint
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    /// <summary>
    /// Normalized intensity between 0.0 (cold) and 1.0 (hot).
    /// </summary>
    public double Weight { get; set; }

    /// <summary>
    /// Number of raw location samples clustered into this heat center.
    /// </summary>
    public int SampleCount { get; set; } = 1;

    /// <summary>
    /// Calculated visual radius in meters on the map.
    /// </summary>
    public double RadiusMeters { get; set; } = 50.0;

    public HeatPoint() { }

    public HeatPoint(double latitude, double longitude, double weight, int sampleCount = 1, double radiusMeters = 50.0)
    {
        Latitude = latitude;
        Longitude = longitude;
        Weight = Math.Clamp(weight, 0.0, 1.0);
        SampleCount = sampleCount;
        RadiusMeters = radiusMeters;
    }
}
