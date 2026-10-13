using LocationHeatMapApp.ViewModels;
using Microsoft.Maui.Maps;

namespace LocationHeatMapApp.Views;

public partial class MainMapView : ContentPage
{
    private readonly MainMapViewModel _viewModel;

    public MainMapView(MainMapViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;

        _viewModel.RequestMapZoomToFit += () => AppMap.ZoomToFitHeatMap();
        _viewModel.RequestCenterMap += (lat, lng) =>
        {
            AppMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(lat, lng), Distance.FromKilometers(1.0)));
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();

        // Default initial camera frame (e.g., San Francisco / current area)
        AppMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(37.7749, -122.4194), Distance.FromKilometers(2.5)));
    }

    private void OnZoomFitClicked(object sender, EventArgs e)
    {
        AppMap.ZoomToFitHeatMap();
    }

    private void OnToggleHeatMapClicked(object sender, EventArgs e)
    {
        AppMap.IsHeatMapVisible = !AppMap.IsHeatMapVisible;
        _viewModel.IsHeatMapVisible = AppMap.IsHeatMapVisible;
    }
}
