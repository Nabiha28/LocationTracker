#if ANDROID || IOS || MACCATALYST || WINDOWS
using Microsoft.Maui.Maps.Handlers;
#endif

namespace LocationHeatMapApp.Controls;

/// <summary>
/// Custom Handler extending .NET MAUI MapHandler to provide platform-specific native hooks
/// for high-performance HeatMap tile overlays, shaders, or native map renderers.
/// </summary>
public partial class HeatMapHandler
#if ANDROID || IOS || MACCATALYST || WINDOWS
    : MapHandler
#endif
{
    public static PropertyMapper<HeatMap, HeatMapHandler> HeatMapMapper = new(Mapper)
    {
        [nameof(HeatMap.HeatPoints)] = MapHeatPoints,
        [nameof(HeatMap.IsHeatMapVisible)] = MapIsHeatMapVisible
    };

    public HeatMapHandler() : base(HeatMapMapper)
    {
    }

    public static void MapHeatPoints(HeatMapHandler handler, HeatMap view)
    {
        handler.UpdatePlatformHeatMap(view);
    }

    public static void MapIsHeatMapVisible(HeatMapHandler handler, HeatMap view)
    {
        handler.UpdatePlatformHeatMap(view);
    }

    partial void UpdatePlatformHeatMap(HeatMap view);
}
