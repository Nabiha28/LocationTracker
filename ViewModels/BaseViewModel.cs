using CommunityToolkit.Mvvm.ComponentModel;

namespace LocationHeatMapApp.ViewModels;

public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;
}
