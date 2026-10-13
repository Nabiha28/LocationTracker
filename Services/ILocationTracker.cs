using LocationHeatMapApp.Models;

namespace LocationHeatMapApp.Services;

/// <summary>
/// Interface for real-time and background GPS location tracking.
/// </summary>
public interface ILocationTracker
{
    bool IsTracking { get; }
    bool IsSimulating { get; }
    LocationPoint? LastRecordedPoint { get; }

    event EventHandler<LocationPoint>? LocationUpdated;
    event EventHandler<string>? TrackingStatusChanged;

    Task<bool> StartTrackingAsync(TimeSpan interval, GeolocationAccuracy accuracy);
    Task StopTrackingAsync();
    Task<LocationPoint?> GetSingleLocationAsync();

    void StartSimulation(double startLat = 37.7749, double startLng = -122.4194);
    void StopSimulation();
    Task SeedSampleDataAsync(int count = 150);
}
