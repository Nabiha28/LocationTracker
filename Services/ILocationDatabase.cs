using LocationHeatMapApp.Models;

namespace LocationHeatMapApp.Services;

/// <summary>
/// Contract for persisting and querying GPS location telemetry in SQLite.
/// </summary>
public interface ILocationDatabase
{
    Task InitializeAsync();
    Task<int> SaveLocationAsync(LocationPoint point);
    Task<int> SaveLocationsBatchAsync(IEnumerable<LocationPoint> points);
    Task<List<LocationPoint>> GetAllLocationsAsync();
    Task<List<LocationPoint>> GetLocationsByDateRangeAsync(DateTime fromUtc, DateTime toUtc);
    Task<List<LocationPoint>> GetLocationsInBoundsAsync(double minLat, double maxLat, double minLng, double maxLng);
    Task<int> GetCountAsync();
    Task<int> ClearAllLocationsAsync();
    Task<int> DeleteLocationAsync(int id);
}
