using LocationHeatMapApp.Models;
using SQLite;

namespace LocationHeatMapApp.Services;

/// <summary>
/// SQLite implementation of <see cref="ILocationDatabase"/> using sqlite-net-pcl.
/// Handles connection lifecycle, background queries, and schema migration.
/// </summary>
public class LocationDatabase : ILocationDatabase
{
    private const string DatabaseFilename = "UserLocations.db3";
    private readonly string _databasePath;
    private SQLiteAsyncConnection? _connection;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public LocationDatabase()
    {
        _databasePath = Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);
    }

    public LocationDatabase(string customPath)
    {
        _databasePath = customPath;
    }

    public async Task InitializeAsync()
    {
        if (_connection is not null)
            return;

        await _semaphore.WaitAsync();
        try
        {
            if (_connection is null)
            {
                var flags = SQLiteOpenFlags.ReadWrite |
                            SQLiteOpenFlags.Create |
                            SQLiteOpenFlags.SharedCache;

                _connection = new SQLiteAsyncConnection(_databasePath, flags);

                // Create the table if it does not already exist
                await _connection.CreateTableAsync<LocationPoint>();
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        if (_connection is null)
        {
            await InitializeAsync();
        }
        return _connection!;
    }

    public async Task<int> SaveLocationAsync(LocationPoint point)
    {
        var db = await GetConnectionAsync();
        return await db.InsertAsync(point);
    }

    public async Task<int> SaveLocationsBatchAsync(IEnumerable<LocationPoint> points)
    {
        var db = await GetConnectionAsync();
        return await db.InsertAllAsync(points);
    }

    public async Task<List<LocationPoint>> GetAllLocationsAsync()
    {
        var db = await GetConnectionAsync();
        return await db.Table<LocationPoint>()
                       .OrderByDescending(p => p.Timestamp)
                       .ToListAsync();
    }

    public async Task<List<LocationPoint>> GetLocationsByDateRangeAsync(DateTime fromUtc, DateTime toUtc)
    {
        var db = await GetConnectionAsync();
        return await db.Table<LocationPoint>()
                       .Where(p => p.Timestamp >= fromUtc && p.Timestamp <= toUtc)
                       .OrderByDescending(p => p.Timestamp)
                       .ToListAsync();
    }

    public async Task<List<LocationPoint>> GetLocationsInBoundsAsync(double minLat, double maxLat, double minLng, double maxLng)
    {
        var db = await GetConnectionAsync();
        return await db.Table<LocationPoint>()
                       .Where(p => p.Latitude >= minLat && p.Latitude <= maxLat &&
                                   p.Longitude >= minLng && p.Longitude <= maxLng)
                       .ToListAsync();
    }

    public async Task<int> GetCountAsync()
    {
        var db = await GetConnectionAsync();
        return await db.Table<LocationPoint>().CountAsync();
    }

    public async Task<int> ClearAllLocationsAsync()
    {
        var db = await GetConnectionAsync();
        return await db.DeleteAllAsync<LocationPoint>();
    }

    public async Task<int> DeleteLocationAsync(int id)
    {
        var db = await GetConnectionAsync();
        return await db.DeleteAsync<LocationPoint>(id);
    }
}
