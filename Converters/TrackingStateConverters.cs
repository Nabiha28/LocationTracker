using System.Globalization;

namespace LocationHeatMapApp.Converters;

public class BooleanToTrackingTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isTracking)
        {
            return isTracking ? "Stop GPS Tracking" : "Start GPS Tracking";
        }
        return "Start GPS Tracking";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class BooleanToSimulationTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isSimulating)
        {
            return isSimulating ? "Stop Simulation" : "Simulate Movement";
        }
        return "Simulate Movement";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class BooleanToColorConverter : IValueConverter
{
    public Color TrueColor { get; set; } = Color.FromArgb("#EF4444"); // Red / Rose
    public Color FalseColor { get; set; } = Color.FromArgb("#10B981"); // Green / Emerald

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
        {
            return b ? TrueColor : FalseColor;
        }
        return FalseColor;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}
