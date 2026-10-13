using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocationHeatMapApp.Models;
using LocationHeatMapApp.Services;

namespace LocationHeatMapApp.ViewModels;

public partial class HistoryViewModel : BaseViewModel
{
    private readonly ILocationDatabase _database;

    [ObservableProperty]
    private ObservableCollection<LocationPoint> _points = new();

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private string _filterOption = "All";

    public HistoryViewModel(ILocationDatabase database)
    {
        _database = database;
        Title = "Location History";
    }

    public async Task LoadHistoryAsync()
    {
        IsBusy = true;
        StatusMessage = "Loading recorded positions...";

        List<LocationPoint> list;
        if (FilterOption == "Today")
        {
            var startOfDay = DateTime.UtcNow.Date;
            var endOfDay = startOfDay.AddDays(1);
            list = await _database.GetLocationsByDateRangeAsync(startOfDay, endOfDay);
        }
        else if (FilterOption == "Past 7 Days")
        {
            var past7 = DateTime.UtcNow.AddDays(-7);
            list = await _database.GetLocationsByDateRangeAsync(past7, DateTime.UtcNow);
        }
        else
        {
            list = await _database.GetAllLocationsAsync();
        }

        Points.Clear();
        foreach (var p in list)
        {
            Points.Add(p);
        }

        TotalCount = Points.Count;
        StatusMessage = $"{TotalCount} location fixes found.";
        IsBusy = false;
    }

    [RelayCommand]
    public async Task DeletePointAsync(LocationPoint point)
    {
        if (point == null) return;

        await _database.DeleteLocationAsync(point.Id);
        Points.Remove(point);
        TotalCount = Points.Count;
    }

    [RelayCommand]
    public async Task ClearAllAsync()
    {
        bool confirm = await Shell.Current.DisplayAlert(
            "Wipe History",
            "Clear all location telemetry from database?",
            "Yes, Delete",
            "Cancel");

        if (confirm)
        {
            await _database.ClearAllLocationsAsync();
            Points.Clear();
            TotalCount = 0;
            StatusMessage = "Database cleared.";
        }
    }

    [RelayCommand]
    public async Task SetFilterAsync(string filter)
    {
        FilterOption = filter;
        await LoadHistoryAsync();
    }
}
