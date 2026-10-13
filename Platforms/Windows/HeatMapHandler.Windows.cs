#if WINDOWS
using LocationHeatMapApp.Controls;
using Microsoft.Maui.Maps.Handlers;

namespace LocationHeatMapApp.Controls;

public partial class HeatMapHandler : MapHandler
{
    partial void UpdatePlatformHeatMap(HeatMap view)
    {
        // Windows Desktop MapHandler hook
        System.Diagnostics.Debug.WriteLine($"[Windows Map] Rendering {view.HeatPoints?.Count() ?? 0} heatmap nodes.");
    }
}
#endif
