using LocationHeatMapApp.ViewModels;

namespace LocationHeatMapApp.Views;

public partial class SettingsView : ContentPage
{
    private readonly SettingsViewModel _viewModel;

    public SettingsView(SettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.RefreshStatsAsync();
    }
}
