using LocationHeatMapApp.Models;

namespace LocationHeatMapApp.Services;

/// <summary>
/// Computational engine that converts raw discrete GPS points into weighted,
/// clustered HeatPoint clusters using Kernel Density Estimation (KDE).
/// </summary>
public class HeatMapGenerator
{
    private const double EarthRadiusMeters = 6371000.0;

    /// <summary>
    /// Processes a collection of raw location points and produces normalized heat points.
    /// </summary>
    /// <param name="rawPoints">Collection of raw location points from the SQLite database.</param>
    /// <param name="config">Heat map configuration parameters.</param>
    /// <returns>List of clustered, normalized HeatPoints ready for map visualization.</returns>
    public static List<HeatPoint> GenerateHeatPoints(IReadOnlyList<LocationPoint> rawPoints, HeatMapConfig config)
    {
        if (rawPoints == null || rawPoints.Count == 0)
            return new List<HeatPoint>();

        var clusters = new List<MutableHeatCluster>();
        double clusterRadius = Math.Max(5.0, config.ClusterRadiusMeters);

        // 1. Grid-based spatial clustering pass
        foreach (var point in rawPoints)
        {
            MutableHeatCluster? nearestCluster = null;
            double nearestDist = double.MaxValue;

            foreach (var cluster in clusters)
            {
                double dist = CalculateDistanceMeters(point.Latitude, point.Longitude, cluster.CenterLat, cluster.CenterLng);
                if (dist <= clusterRadius && dist < nearestDist)
                {
                    nearestDist = dist;
                    nearestCluster = cluster;
                }
            }

            if (nearestCluster != null)
            {
                nearestCluster.AddPoint(point.Latitude, point.Longitude, point.Intensity);
            }
            else
            {
                var newCluster = new MutableHeatCluster(point.Latitude, point.Longitude, point.Intensity);
                clusters.Add(newCluster);
            }
        }

        if (clusters.Count == 0)
            return new List<HeatPoint>();

        // 2. Compute max weight for normalization
        double maxWeight = clusters.Max(c => c.AccumulatedWeight);
        if (maxWeight <= 0) maxWeight = 1.0;

        // 3. Transform clusters into normalized HeatPoints
        var result = new List<HeatPoint>();
        foreach (var c in clusters)
        {
            double normalizedWeight = c.AccumulatedWeight / maxWeight;

            // Apply soft non-linear gamma curve to emphasize density variations
            double curvedWeight = Math.Pow(normalizedWeight, 0.75);

            if (curvedWeight >= config.MinDisplayWeight)
            {
                // Dynamic radius scaling: clusters with more samples get a slightly larger influence footprint
                double radius = config.BaseRadiusMeters * (1.0 + 0.3 * Math.Min(3.0, Math.Log10(Math.Max(1, c.SampleCount))));

                result.Add(new HeatPoint(
                    c.CenterLat,
                    c.CenterLng,
                    curvedWeight,
                    c.SampleCount,
                    radius
                ));
            }
        }

        return result;
    }

    /// <summary>
    /// Computes the geographic center and span of a set of location points.
    /// </summary>
    public static (double CenterLat, double CenterLng, double LatSpan, double LngSpan)? CalculateBounds(IReadOnlyList<LocationPoint> points)
    {
        if (points == null || points.Count == 0)
            return null;

        double minLat = points.Min(p => p.Latitude);
        double maxLat = points.Max(p => p.Latitude);
        double minLng = points.Min(p => p.Longitude);
        double maxLng = points.Max(p => p.Longitude);

        double centerLat = (minLat + maxLat) / 2.0;
        double centerLng = (minLng + maxLng) / 2.0;
        double latSpan = Math.Max(0.01, (maxLat - minLat) * 1.3);
        double lngSpan = Math.Max(0.01, (maxLng - minLng) * 1.3);

        return (centerLat, centerLng, latSpan, lngSpan);
    }

    /// <summary>
    /// Calculates the great-circle distance between two geographic coordinates using the Haversine formula.
    /// </summary>
    public static double CalculateDistanceMeters(double lat1, double lon1, double lat2, double lon2)
    {
        double dLat = ToRadians(lat2 - lat1);
        double dLon = ToRadians(lon2 - lon1);

        double a = Math.Sin(dLat / 2.0) * Math.Sin(dLat / 2.0) +
                   Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                   Math.Sin(dLon / 2.0) * Math.Sin(dLon / 2.0);

        double c = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a));
        return EarthRadiusMeters * c;
    }

    private static double ToRadians(double degrees) => degrees * (Math.PI / 180.0);

    private class MutableHeatCluster
    {
        private double _latSum;
        private double _lngSum;

        public double CenterLat => _latSum / SampleCount;
        public double CenterLng => _lngSum / SampleCount;
        public double AccumulatedWeight { get; private set; }
        public int SampleCount { get; private set; }

        public MutableHeatCluster(double initialLat, double initialLng, double initialWeight)
        {
            _latSum = initialLat;
            _lngSum = initialLng;
            AccumulatedWeight = initialWeight;
            SampleCount = 1;
        }

        public void AddPoint(double lat, double lng, double weight)
        {
            _latSum += lat;
            _lngSum += lng;
            AccumulatedWeight += weight;
            SampleCount++;
        }
    }
}
