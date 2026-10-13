namespace LocationHeatMapApp.Models;

/// <summary>
/// Configuration options for the heat map algorithm and rendering pipeline.
/// </summary>
public class HeatMapConfig
{
    /// <summary>
    /// Base influence radius in meters for each heat point.
    /// Default is 60 meters.
    /// </summary>
    public double BaseRadiusMeters { get; set; } = 60.0;

    /// <summary>
    /// Overall opacity multiplier for rendered heat elements [0.1 to 1.0].
    /// </summary>
    public float GlobalOpacity { get; set; } = 0.65f;

    /// <summary>
    /// Grid / spatial clustering distance in meters to group nearby points.
    /// </summary>
    public double ClusterRadiusMeters { get; set; } = 40.0;

    /// <summary>
    /// Number of concentric multi-ring gradients generated per heat point
    /// to simulate smooth Gaussian falloff on vector maps.
    /// </summary>
    public int ConcentricRingsCount { get; set; } = 4;

    /// <summary>
    /// Color gradient palette used for heat level transitions.
    /// </summary>
    public HeatMapColorGradient Gradient { get; set; } = HeatMapColorGradient.StandardThermal;

    /// <summary>
    /// Minimum density weight required to display a heat node.
    /// </summary>
    public double MinDisplayWeight { get; set; } = 0.05;
}
