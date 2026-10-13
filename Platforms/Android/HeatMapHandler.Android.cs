#if ANDROID
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using LocationHeatMapApp.Controls;
using Microsoft.Maui.Maps.Handlers;

namespace LocationHeatMapApp.Controls;

public partial class HeatMapHandler : MapHandler
{
    private GoogleMap? _googleMap;

    protected override void ConnectHandler(MapView platformView)
    {
        base.ConnectHandler(platformView);
        platformView.GetMapAsync(new OnMapReadyCallback(map =>
        {
            _googleMap = map;
            if (VirtualView is HeatMap heatMapView)
            {
                UpdatePlatformHeatMap(heatMapView);
            }
        }));
    }

    partial void UpdatePlatformHeatMap(HeatMap view)
    {
        if (_googleMap == null) return;

        // Platform-native hook: In addition to cross-platform vector circles,
        // developers can attach Android HeatmapTileProvider (com.google.maps.android.heatmaps.HeatmapTileProvider)
        // or customize native ground overlays here.
        System.Diagnostics.Debug.WriteLine($"[Android Native Map] Rendering {view.HeatPoints?.Count() ?? 0} heatmap nodes.");
    }

    private class OnMapReadyCallback : Java.Lang.Object, IOnMapReadyCallback
    {
        private readonly Action<GoogleMap> _callback;

        public OnMapReadyCallback(Action<GoogleMap> callback)
        {
            _callback = callback;
        }

        public void OnMapReady(GoogleMap googleMap)
        {
            _callback(googleMap);
        }
    }
}
#endif
