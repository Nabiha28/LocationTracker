using System.Collections.Specialized;
using LocationHeatMapApp.Models;
using LocationHeatMapApp.Services;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace LocationHeatMapApp.Controls;

/// <summary>
/// Custom extended .NET MAUI Map that visualizes location density as a heat map.
/// Extends <see cref="Microsoft.Maui.Controls.Maps.Map"/> with bindable heat properties,
/// multi-ring Gaussian falloff rendering, and spatial density overlays.
/// </summary>
public class HeatMap : Microsoft.Maui.Controls.Maps.Map
{
    public static readonly BindableProperty HeatPointsProperty =
        BindableProperty.Create(
            nameof(HeatPoints),
            typeof(IEnumerable<HeatPoint>),
            typeof(HeatMap),
            null,
            propertyChanged: OnHeatPointsChanged);

    public static readonly BindableProperty RawLocationsProperty =
        BindableProperty.Create(
            nameof(RawLocations),
            typeof(IEnumerable<LocationPoint>),
            typeof(HeatMap),
            null,
            propertyChanged: OnRawLocationsChanged);

    public static readonly BindableProperty ConfigProperty =
        BindableProperty.Create(
            nameof(Config),
            typeof(HeatMapConfig),
            typeof(HeatMap),
            new HeatMapConfig(),
            propertyChanged: OnConfigChanged);

    public static readonly BindableProperty IsHeatMapVisibleProperty =
        BindableProperty.Create(
            nameof(IsHeatMapVisible),
            typeof(bool),
            typeof(HeatMap),
            true,
            propertyChanged: OnIsHeatMapVisibleChanged);

    public IEnumerable<HeatPoint>? HeatPoints
    {
        get => (IEnumerable<HeatPoint>?)GetValue(HeatPointsProperty);
        set => SetValue(HeatPointsProperty, value);
    }

    public IEnumerable<LocationPoint>? RawLocations
    {
        get => (IEnumerable<LocationPoint>?)GetValue(RawLocationsProperty);
        set => SetValue(RawLocationsProperty, value);
    }

    public HeatMapConfig Config
    {
        get => (HeatMapConfig)GetValue(ConfigProperty);
        set => SetValue(ConfigProperty, value);
    }

    public bool IsHeatMapVisible
    {
        get => (bool)GetValue(IsHeatMapVisibleProperty);
        set => SetValue(IsHeatMapVisibleProperty, value);
    }

    public HeatMap()
    {
        // Standard Map configuration
        IsShowingUser = true;
        MapType = MapType.Street;
    }

    private static void OnHeatPointsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is HeatMap map)
        {
            if (oldValue is INotifyCollectionChanged oldColl)
                oldColl.CollectionChanged -= map.OnCollectionChanged;

            if (newValue is INotifyCollectionChanged newColl)
                newColl.CollectionChanged += map.OnCollectionChanged;

            map.RenderHeatMap();
        }
    }

    private static void OnRawLocationsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is HeatMap map && newValue is IEnumerable<LocationPoint> rawPoints)
        {
            var pointsList = rawPoints.ToList();
            var processed = HeatMapGenerator.GenerateHeatPoints(pointsList, map.Config);
            map.HeatPoints = processed;
        }
    }

    private static void OnConfigChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is HeatMap map)
        {
            // If raw locations exist, recalculate with the new configuration
            if (map.RawLocations != null)
            {
                var processed = HeatMapGenerator.GenerateHeatPoints(map.RawLocations.ToList(), map.Config);
                map.HeatPoints = processed;
            }
            else
            {
                map.RenderHeatMap();
            }
        }
    }

    private static void OnIsHeatMapVisibleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is HeatMap map)
        {
            map.RenderHeatMap();
        }
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RenderHeatMap();
    }

    /// <summary>
    /// Renders or clears the heat map overlays using concentric Gaussian-weighted vector circles.
    /// </summary>
    public void RenderHeatMap()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            MapElements.Clear();

            if (!IsHeatMapVisible || HeatPoints == null)
                return;

            var points = HeatPoints.ToList();
            if (points.Count == 0)
                return;

            int rings = Math.Max(2, Config.ConcentricRingsCount);
            float baseOpacity = Config.GlobalOpacity;

            // Render concentric rings to form smooth gradient falloff
            foreach (var point in points)
            {
                var center = new Location(point.Latitude, point.Longitude);
                double baseRadius = point.RadiusMeters > 0 ? point.RadiusMeters : Config.BaseRadiusMeters;

                // Draw outer rings to inner rings so intense centers blend nicely on top
                for (int ring = rings; ring >= 1; ring--)
                {
                    double ringFactor = (double)ring / rings;
                    double ringRadius = baseRadius * ringFactor;

                    // Compute gradient intensity for this ring
                    // Outer ring uses lower weight, inner ring approaches full weight
                    double ringWeight = point.Weight * (1.0 - (ringFactor * 0.45));

                    // Opacity falls off quadratically towards outer perimeter
                    float falloff = (float)Math.Pow(1.0 - (ringFactor * 0.6), 1.5);
                    float ringAlpha = baseOpacity * falloff * 0.55f;

                    var color = Config.Gradient.GetColorForIntensity(ringWeight, ringAlpha);

                    var circle = new Microsoft.Maui.Controls.Maps.Circle
                    {
                        Center = center,
                        Radius = Distance.FromMeters(ringRadius),
                        StrokeColor = Colors.Transparent,
                        StrokeWidth = 0f,
                        FillColor = color
                    };

                    MapElements.Add(circle);
                }
            }
        });
    }

    /// <summary>
    /// Smoothly pans and zooms the camera view to encompass all heat map points.
    /// </summary>
    public void ZoomToFitHeatMap()
    {
        if (HeatPoints == null) return;
        var pointsList = HeatPoints.ToList();
        if (pointsList.Count == 0) return;

        double minLat = pointsList.Min(p => p.Latitude);
        double maxLat = pointsList.Max(p => p.Latitude);
        double minLng = pointsList.Min(p => p.Longitude);
        double maxLng = pointsList.Max(p => p.Longitude);

        double centerLat = (minLat + maxLat) / 2.0;
        double centerLng = (minLng + maxLng) / 2.0;
        double latSpan = Math.Max(0.008, (maxLat - minLat) * 1.4);
        double lngSpan = Math.Max(0.008, (maxLng - minLng) * 1.4);

        MoveToRegion(new MapSpan(new Location(centerLat, centerLng), latSpan, lngSpan));
    }
}
