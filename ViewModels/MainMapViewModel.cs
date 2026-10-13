using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocationHeatMapApp.Models;
using LocationHeatMapApp.Services;

namespace LocationHeatMapApp.ViewModels;

public partial class MainMapViewModel : BaseViewModel
{
    private readonly ILocationDatabase _database;
    private readonly ILocationTracker _tracker;

    [ObservableProperty]
    private bool _isTracking;

    [ObservableProperty]
    private bool _isSimulating;

    [ObservableProperty]
    private int _totalRecordedPoints;

    [ObservableProperty]
    private string _currentStatus = "Ready";

    [ObservableProperty]
    private double _heatRadius = 60.0;

    [ObservableProperty]
    private double _heatOpacity = 0.70;

    [ObservableProperty]
    private double _clusterRadius = 40.0;

    [ObservableProperty]
    private bool _isHeatMapVisible = true;

    [ObservableProperty]
    private string _lastLocationText = "No GPS fix yet";

    [ObservableProperty]
    private string _activePaletteName = "Thermal";

    [ObservableProperty]
    private ObservableCollection<HeatPoint> _heatPoints = new();

    public HeatMapConfig MapConfig { get; } = new();

    public event Action? RequestMapZoomToFit;
    public event Action<double, double>? RequestCenterMap;

    public MainMapViewModel(ILocationDatabase database, ILocationTracker tracker)
    {
        _database = database;
        _tracker = tracker;
        Title = "Location HeatMap";

        // Subscribe to tracker events
        _tracker.LocationUpdated += OnLocationReceived;
        _tracker.TrackingStatusChanged += (s, msg) => CurrentStatus = msg;
    }

    public async Task InitializeAsync()
    {
        await _database.InitializeAsync();
        await ReloadHeatMapDataAsync();
    }

    private async void OnLocationReceived(object? sender, LocationPoint point)
    {
        LastLocationText = $"Lat: {point.Latitude:F4}, Lng: {point.Longitude:F4} ({point.Timestamp.ToLocalTime():T})";
        TotalRecordedPoints++;

        // Refresh heat map incrementally
        await ReloadHeatMapDataAsync(centerOnFix: false);
    }

    [RelayCommand]
    public async Task ToggleTrackingAsync()
    {
        if (IsTracking)
        {
            await _tracker.StopTrackingAsync();
            IsTracking = false;
            CurrentStatus = "Tracking Paused";
        }
        else
        {
            CurrentStatus = "Requesting GPS...";
            bool started = await _tracker.StartTrackingAsync(TimeSpan.FromSeconds(5), GeolocationAccuracy.High);
            IsTracking = started;
            if (started)
            {
                CurrentStatus = "Tracking Active (5s interval)";
            }
        }
    }

    [RelayCommand]
    public void ToggleSimulation()
    {
        if (IsSimulating)
        {
            _tracker.StopSimulation();
            IsSimulating = false;
            CurrentStatus = "Simulation Stopped";
        }
        else
        {
            _tracker.StartSimulation();
            IsSimulating = true;
            CurrentStatus = "Simulation Running (Walking route)";
        }
    }

    [RelayCommand]
    public async Task SeedSampleDataAsync()
    {
        IsBusy = true;
        CurrentStatus = "Generating 150 synthetic hotspot points...";
        await _tracker.SeedSampleDataAsync(150);
        await ReloadHeatMapDataAsync(centerOnFix: true);
        IsBusy = false;
        CurrentStatus = "Sample data loaded successfully!";
    }

    [RelayCommand]
    public async Task ClearAllDataAsync()
    {
        bool confirm = await Shell.Current.DisplayAlert(
            "Clear Database",
            "Are you sure you want to delete all saved GPS points? This action cannot be undone.",
            "Delete All",
            "Cancel");

        if (confirm)
        {
            IsBusy = true;
            await _database.ClearAllLocationsAsync();
            TotalRecordedPoints = 0;
            HeatPoints.Clear();
            LastLocationText = "No GPS fix yet";
            CurrentStatus = "Database cleared";
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task ReloadHeatMapDataAsync(bool centerOnFix = false)
    {
        var rawList = await _database.GetAllLocationsAsync();
        TotalRecordedPoints = rawList.Count;

        MapConfig.BaseRadiusMeters = HeatRadius;
        MapConfig.GlobalOpacity = (float)HeatOpacity;
        MapConfig.ClusterRadiusMeters = ClusterRadius;

        var computed = await Task.Run(() => HeatMapGenerator.GenerateHeatPoints(rawList, MapConfig));

        HeatPoints.Clear();
        foreach (var p in computed)
        {
            HeatPoints.Add(p);
        }

        if (centerOnFix && rawList.Count > 0)
        {
            RequestMapZoomToFit?.Invoke();
        }
    }

    [RelayCommand]
    public void TogglePalette()
    {
        if (ActivePaletteName == "Thermal")
        {
            ActivePaletteName = "Neon Glow";
            MapConfig.Gradient = HeatMapColorGradient.NeonGlow;
        }
        else
        {
            ActivePaletteName = "Thermal";
            MapConfig.Gradient = HeatMapColorGradient.StandardThermal;
        }

        // Trigger rerender
        _ = ReloadHeatMapDataAsync();
    }

    [RelayCommand]
    public async Task CenterOnUserLocationAsync()
    {
        var loc = await _tracker.GetSingleLocationAsync();
        if (loc != null)
        {
            RequestCenterMap?.Invoke(loc.Latitude, loc.Longitude);
        }
        else
        {
            RequestMapZoomToFit?.Invoke();
        }
    }

    partial void OnHeatRadiusChanged(double value) => _ = ReloadHeatMapDataAsync();
    partial void OnHeatOpacityChanged(double value) => _ = ReloadHeatMapDataAsync();
    partial void OnClusterRadiusChanged(double value) => _ = ReloadHeatMapDataAsync();
}
