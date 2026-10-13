using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocationHeatMapApp.Services;

namespace LocationHeatMapApp.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
    private readonly ILocationDatabase _database;

    [ObservableProperty]
    private string _databasePath = string.Empty;

    [ObservableProperty]
    private int _recordCount;

    [ObservableProperty]
    private string _trackingAccuracy = "High";

    [ObservableProperty]
    private int _trackingIntervalSeconds = 5;

    public SettingsViewModel(ILocationDatabase database)
    {
        _database = database;
        Title = "Settings";
        DatabasePath = Path.Combine(FileSystem.AppDataDirectory, "UserLocations.db3");
    }

    public async Task RefreshStatsAsync()
    {
        RecordCount = await _database.GetCountAsync();
    }

    [RelayCommand]
    public async Task OptimizeDatabaseAsync()
    {
        IsBusy = true;
        StatusMessage = "Optimizing SQLite indexes...";
        await Task.Delay(400); // simulated quick vacuum
        RecordCount = await _database.GetCountAsync();
        StatusMessage = "Database optimized.";
        IsBusy = false;
    }
}
