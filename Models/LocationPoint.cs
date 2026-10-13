using SQLite;

namespace LocationHeatMapApp.Models;

/// <summary>
/// Represents a recorded geographic location stored in the SQLite database.
/// </summary>
[Table("LocationHistory")]
public class LocationPoint
{
    [PrimaryKey, AutoIncrement]
    [Column("id")]
    public int Id { get; set; }

    [Column("latitude")]
    [Indexed]
    public double Latitude { get; set; }

    [Column("longitude")]
    [Indexed]
    public double Longitude { get; set; }

    [Column("altitude")]
    public double? Altitude { get; set; }

    [Column("accuracy")]
    public double? Accuracy { get; set; }

    [Column("speed")]
    public double? Speed { get; set; }

    [Column("heading")]
    public double? Heading { get; set; }

    [Column("timestamp")]
    [Indexed]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Weight or intensity factor of this location (e.g., dwell duration or custom metric).
    /// Defaults to 1.0.
    /// </summary>
    [Column("intensity")]
    public double Intensity { get; set; } = 1.0;

    /// <summary>
    /// Optional session identifier to distinguish separate trips or recording runs.
    /// </summary>
    [Column("session_id")]
    [Indexed]
    public string? SessionId { get; set; }

    [Ignore]
    public string DisplayCoordinates => $"{Latitude:F5}, {Longitude:F5}";

    [Ignore]
    public string FormattedTime => Timestamp.ToLocalTime().ToString("MMM dd, yyyy HH:mm:ss");
}
