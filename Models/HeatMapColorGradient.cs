namespace LocationHeatMapApp.Models;

/// <summary>
/// Provides color interpolation algorithms and palettes for heat map density visualizations.
/// </summary>
public class HeatMapColorGradient
{
    public struct GradientStop
    {
        public double Position { get; }
        public Color Color { get; }

        public GradientStop(double position, Color color)
        {
            Position = position;
            Color = color;
        }
    }

    public List<GradientStop> Stops { get; set; } = new();

    /// <summary>
    /// Default standard thermal rainbow gradient (Blue -> Cyan -> Green -> Yellow -> Red).
    /// </summary>
    public static HeatMapColorGradient StandardThermal => new HeatMapColorGradient(new[]
    {
        new GradientStop(0.00, Color.FromRgba(0, 0, 255, 120)),      // Cold Blue
        new GradientStop(0.25, Color.FromRgba(0, 255, 255, 160)),    // Cyan
        new GradientStop(0.50, Color.FromRgba(0, 255, 0, 190)),      // Green
        new GradientStop(0.75, Color.FromRgba(255, 255, 0, 220)),    // Yellow
        new GradientStop(1.00, Color.FromRgba(255, 0, 0, 255))       // Fiery Red
    });

    /// <summary>
    /// Modern Neon / Cyber gradient (Purple -> Pink -> Orange -> Yellow).
    /// </summary>
    public static HeatMapColorGradient NeonGlow => new HeatMapColorGradient(new[]
    {
        new GradientStop(0.00, Color.FromRgba(76, 29, 149, 120)),    // Deep Violet
        new GradientStop(0.30, Color.FromRgba(168, 85, 247, 160)),   // Purple
        new GradientStop(0.60, Color.FromRgba(236, 72, 153, 200)),   // Hot Pink
        new GradientStop(0.85, Color.FromRgba(249, 115, 22, 230)),   // Vibrant Orange
        new GradientStop(1.00, Color.FromRgba(250, 204, 21, 255))    // Electric Yellow
    });

    public HeatMapColorGradient()
    {
        Stops = StandardThermal.Stops;
    }

    public HeatMapColorGradient(IEnumerable<GradientStop> stops)
    {
        Stops = stops.OrderBy(s => s.Position).ToList();
    }

    /// <summary>
    /// Interpolates a color for a given normalized intensity value [0.0, 1.0].
    /// </summary>
    public Color GetColorForIntensity(double intensity, float alphaMultiplier = 1.0f)
    {
        intensity = Math.Clamp(intensity, 0.0, 1.0);

        if (Stops.Count == 0)
            return Color.FromRgba(255, 0, 0, (int)(255 * alphaMultiplier));

        if (intensity <= Stops.First().Position)
        {
            var first = Stops.First().Color;
            return Color.FromRgba(first.Red, first.Green, first.Blue, first.Alpha * alphaMultiplier);
        }

        if (intensity >= Stops.Last().Position)
        {
            var last = Stops.Last().Color;
            return Color.FromRgba(last.Red, last.Green, last.Blue, last.Alpha * alphaMultiplier);
        }

        // Find surrounding stops
        for (int i = 0; i < Stops.Count - 1; i++)
        {
            var lower = Stops[i];
            var upper = Stops[i + 1];

            if (intensity >= lower.Position && intensity <= upper.Position)
            {
                double range = upper.Position - lower.Position;
                double factor = range > 0 ? (intensity - lower.Position) / range : 0;

                float r = (float)(lower.Color.Red + (upper.Color.Red - lower.Color.Red) * factor);
                float g = (float)(lower.Color.Green + (upper.Color.Green - lower.Color.Green) * factor);
                float b = (float)(lower.Color.Blue + (upper.Color.Blue - lower.Color.Blue) * factor);
                float a = (float)(lower.Color.Alpha + (upper.Color.Alpha - lower.Color.Alpha) * factor) * alphaMultiplier;

                return Color.FromRgba(r, g, b, Math.Clamp(a, 0f, 1f));
            }
        }

        return Stops.Last().Color;
    }
}
