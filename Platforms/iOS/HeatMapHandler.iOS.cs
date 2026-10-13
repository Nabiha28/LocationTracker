#if IOS || MACCATALYST
using LocationHeatMapApp.Controls;
using MapKit;
using Microsoft.Maui.Maps.Handlers;

namespace LocationHeatMapApp.Controls;

public partial class HeatMapHandler : MapHandler
{
    private MKMapView? _mapView;

    protected override void ConnectHandler(MKMapView platformView)
    {
        base.ConnectHandler(platformView);
        _mapView = platformView;

        if (VirtualView is HeatMap heatMapView)
        {
            UpdatePlatformHeatMap(heatMapView);
        }
    }

    partial void UpdatePlatformHeatMap(HeatMap view)
    {
        if (_mapView == null) return;

        // Platform-native iOS hook:
        // Developers can add custom MKTileOverlay or MKOverlayRenderer instances here
        // for native Apple Maps hardware rasterization.
        System.Diagnostics.Debug.WriteLine($"[iOS MapKit] Rendering {view.HeatPoints?.Count() ?? 0} heatmap nodes.");
    }
}
#endif
