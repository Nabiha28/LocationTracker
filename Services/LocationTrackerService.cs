using LocationHeatMapApp.Models;

namespace LocationHeatMapApp.Services;

/// <summary>
/// Service that interfaces with device GPS hardware via MAUI Geolocation
/// and persists fixes to SQLite. Also features an authentic route & dwell simulator.
/// </summary>
public class LocationTrackerService : ILocationTracker
{
    private readonly ILocationDatabase _database;
    private CancellationTokenSource? _cancelTokenSource;
    private IDispatcherTimer? _simulationTimer;
    private readonly Random _random = new();

    private double _simLat = 37.7749; // Default San Francisco downtown
    private double _simLng = -122.4194;
    private double _simHeading = 45.0;

    public bool IsTracking { get; private set; }
    public bool IsSimulating { get; private set; }
    public LocationPoint? LastRecordedPoint { get; private set; }

    public event EventHandler<LocationPoint>? LocationUpdated;
    public event EventHandler<string>? TrackingStatusChanged;

    public LocationTrackerService(ILocationDatabase database)
    {
        _database = database;
    }

    public async Task<bool> StartTrackingAsync(TimeSpan interval, GeolocationAccuracy accuracy)
    {
        if (IsTracking)
            return true;

        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    TrackingStatusChanged?.Invoke(this, "Location permission was denied by user.");
                    return false;
                }
            }

            _cancelTokenSource = new CancellationTokenSource();
            IsTracking = true;
            TrackingStatusChanged?.Invoke(this, "Active GPS Tracking Started");

            // Start background polling loop
            _ = Task.Run(async () =>
            {
                while (IsTracking && !_cancelTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        var request = new GeolocationRequest(accuracy, TimeSpan.FromSeconds(10));
                        var location = await Geolocation.Default.GetLocationAsync(request, _cancelTokenSource.Token);

                        if (location != null)
                        {
                            var point = new LocationPoint
                            {
                                Latitude = location.Latitude,
                                Longitude = location.Longitude,
                                Altitude = location.Altitude,
                                Accuracy = location.Accuracy,
                                Speed = location.Speed,
                                Heading = location.Course,
                                Timestamp = location.Timestamp.UtcDateTime,
                                Intensity = 1.0,
                                SessionId = "live_session"
                            };

                            await _database.SaveLocationAsync(point);
                            LastRecordedPoint = point;

                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                LocationUpdated?.Invoke(this, point);
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[LocationTracker] Error getting fix: {ex.Message}");
                    }

                    await Task.Delay(interval, _cancelTokenSource.Token);
                }
            }, _cancelTokenSource.Token);

            return true;
        }
        catch (Exception ex)
        {
            TrackingStatusChanged?.Invoke(this, $"Failed to start GPS: {ex.Message}");
            IsTracking = false;
            return false;
        }
    }

    public Task StopTrackingAsync()
    {
        if (!IsTracking)
            return Task.CompletedTask;

        _cancelTokenSource?.Cancel();
        _cancelTokenSource?.Dispose();
        _cancelTokenSource = null;

        IsTracking = false;
        TrackingStatusChanged?.Invoke(this, "GPS Tracking Stopped");
        return Task.CompletedTask;
    }

    public async Task<LocationPoint?> GetSingleLocationAsync()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                    return null;
            }

            var location = await Geolocation.Default.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(5)));
            if (location == null) return null;

            return new LocationPoint
            {
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                Altitude = location.Altitude,
                Accuracy = location.Accuracy,
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[LocationTracker] GetSingleLocationAsync error: {ex.Message}");
            return null;
        }
    }

    public void StartSimulation(double startLat = 37.7749, double startLng = -122.4194)
    {
        if (IsSimulating) return;

        _simLat = startLat;
        _simLng = startLng;
        IsSimulating = true;
        TrackingStatusChanged?.Invoke(this, "GPS Simulator Active (Walking & Dwell simulation)");

        _simulationTimer = Application.Current?.Dispatcher.CreateTimer();
        if (_simulationTimer == null) return;

        _simulationTimer.Interval = TimeSpan.FromSeconds(1.5);
        _simulationTimer.Tick += async (s, e) =>
        {
            if (!IsSimulating) return;

            // Simulate walking path with random turns and occasional pauses (dwell clusters)
            bool isDwell = _random.NextDouble() < 0.35; // 35% chance to linger in the same spot
            if (!isDwell)
            {
                _simHeading += (_random.NextDouble() - 0.5) * 40.0;
                double speedMps = 1.2 + _random.NextDouble() * 0.8; // ~3 mph walking speed
                double distanceStep = speedMps * 1.5;

                // Move coordinates according to heading
                double rad = _simHeading * (Math.PI / 180.0);
                double dLat = (distanceStep * Math.Cos(rad)) / 111111.0;
                double dLng = (distanceStep * Math.Sin(rad)) / (111111.0 * Math.Cos(_simLat * (Math.PI / 180.0)));

                _simLat += dLat;
                _simLng += dLng;
            }
            else
            {
                // Slight GPS jitter during dwell
                _simLat += (_random.NextDouble() - 0.5) * 0.00004;
                _simLng += (_random.NextDouble() - 0.5) * 0.00004;
            }

            var simPoint = new LocationPoint
            {
                Latitude = _simLat,
                Longitude = _simLng,
                Altitude = 15.0 + (_random.NextDouble() * 3.0),
                Accuracy = 5.0,
                Speed = isDwell ? 0.0 : 1.4,
                Heading = _simHeading,
                Timestamp = DateTime.UtcNow,
                Intensity = isDwell ? 2.5 : 1.0,
                SessionId = "simulated_session"
            };

            await _database.SaveLocationAsync(simPoint);
            LastRecordedPoint = simPoint;

            LocationUpdated?.Invoke(this, simPoint);
        };

        _simulationTimer.Start();
    }

    public void StopSimulation()
    {
        if (!IsSimulating) return;

        _simulationTimer?.Stop();
        _simulationTimer = null;
        IsSimulating = false;
        TrackingStatusChanged?.Invoke(this, "GPS Simulator Stopped");
    }

    public async Task SeedSampleDataAsync(int count = 150)
    {
        // Generate realistic multi-hotspot synthetic telemetry
        // Centers: Home (high dwell), Coffee Shop (medium), Park (spread), Office (high dwell)
        var hotspots = new[]
        {
            (Lat: 37.7749, Lng: -122.4194, Weight: 40),  // Center / Coffee Shop
            (Lat: 37.7790, Lng: -122.4220, Weight: 60),  // Work Office (very hot)
            (Lat: 37.7710, Lng: -122.4130, Weight: 35),  // Park
            (Lat: 37.7765, Lng: -122.4110, Weight: 50)   // Home (hot)
        };

        var batch = new List<LocationPoint>();
        var now = DateTime.UtcNow;

        foreach (var hotspot in hotspots)
        {
            for (int i = 0; i < hotspot.Weight; i++)
            {
                // Box-Muller Gaussian jitter around hotspot
                double u1 = 1.0 - _random.NextDouble();
                double u2 = 1.0 - _random.NextDouble();
                double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);

                double latOffset = (randStdNormal * 0.00035);
                double lngOffset = ((_random.NextDouble() - 0.5) * 0.0006);

                batch.Add(new LocationPoint
                {
                    Latitude = hotspot.Lat + latOffset,
                    Longitude = hotspot.Lng + lngOffset,
                    Altitude = 12.0,
                    Accuracy = 4.0,
                    Timestamp = now.AddMinutes(-_random.Next(10, 1440)),
                    Intensity = 1.0 + _random.NextDouble() * 1.5,
                    SessionId = "sample_seed"
                });
            }
        }

        // Add connecting trail points between hotspots
        for (int i = 0; i < 30; i++)
        {
            double t = i / 30.0;
            double trailLat = hotspots[0].Lat + (hotspots[1].Lat - hotspots[0].Lat) * t;
            double trailLng = hotspots[0].Lng + (hotspots[1].Lng - hotspots[0].Lng) * t;

            batch.Add(new LocationPoint
            {
                Latitude = trailLat + (_random.NextDouble() - 0.5) * 0.0001,
                Longitude = trailLng + (_random.NextDouble() - 0.5) * 0.0001,
                Altitude = 14.0,
                Accuracy = 6.0,
                Timestamp = now.AddMinutes(-_random.Next(50, 200)),
                Intensity = 0.8,
                SessionId = "sample_trail"
            });
        }

        await _database.SaveLocationsBatchAsync(batch);
    }
}
