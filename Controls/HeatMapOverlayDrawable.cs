using LocationHeatMapApp.Models;

namespace LocationHeatMapApp.Controls;

/// <summary>
/// High-performance hardware-accelerated 2D canvas overlay using Microsoft.Maui.Graphics.
/// Can be superimposed on top of the Map for smooth real-time rendering of large point clouds.
/// </summary>
public class HeatMapOverlayDrawable : IDrawable
{
    public IReadOnlyList<PointF> ScreenPoints { get; set; } = new List<PointF>();
    public IReadOnlyList<float> Weights { get; set; } = new List<float>();
    public HeatMapConfig Config { get; set; } = new();

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (ScreenPoints.Count == 0) return;

        canvas.SaveState();

        float baseRadius = (float)Config.BaseRadiusMeters;
        if (baseRadius < 15f) baseRadius = 15f;

        for (int i = 0; i < ScreenPoints.Count; i++)
        {
            var pt = ScreenPoints[i];
            float weight = i < Weights.Count ? Weights[i] : 0.5f;

            // Draw radial multi-stop gradient
            int rings = Config.ConcentricRingsCount;
            for (int r = rings; r >= 1; r--)
            {
                float factor = (float)r / rings;
                float currentRadius = baseRadius * factor;

                var color = Config.Gradient.GetColorForIntensity(weight * (1f - (factor * 0.4f)), Config.GlobalOpacity * (1f - factor * 0.5f));
                canvas.FillColor = color;
                canvas.FillCircle(pt.X, pt.Y, currentRadius);
            }
        }

        canvas.RestoreState();
    }
}
