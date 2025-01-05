using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace MaintenancePointGpsCalc.ViewModels;

public class DefaultViewModel : MasterPageViewModel
{
    // Input
    [Required]
    public string StartGps { get; set; } // Format: "lat,lon"
    [Required]
    public string EndGps { get; set; }   // Format: "lat,lon"
    [Required]
    public double TrackLength { get; set; }
    [Required]
    public double FlightTime { get; set; }
    [Required]
    public double TargetTime { get; set; }
    // Output
    public string TargetGps { get; set; }

    public void CalculateCoordinates()
    {
        try
        {
            // Parse GPS
            var startGpsParts = StartGps.Replace(", ", ",").Split(',');
            double startLat = ParseCoordinate(startGpsParts[0].Trim(), "N", "S");
            double startLon = ParseCoordinate(startGpsParts[1].Trim(), "E", "W");

            var endGpsParts = EndGps.Replace(", ", ",").Split(',');
            double endLat = ParseCoordinate(endGpsParts[0].Trim(), "N", "S");
            double endLon = ParseCoordinate(endGpsParts[1].Trim(), "E", "W");

            // Calculate progress along the track
            double progressRatio = TargetTime / FlightTime;

            // Calculate target GPS
            // P = A + s*t formula
            double targetLat = startLat + (endLat - startLat) * progressRatio;
            double targetLon = startLon + (endLon - startLon) * progressRatio;

            // Format output with correct directions
            TargetGps = $"{FormatCoordinate(targetLat, "N", "S")},{FormatCoordinate(targetLon, "E", "W")}";
        }
        catch (Exception ex)
        {
            TargetGps = $"Error: {ex.Message}";
        }
    }

    private string FormatCoordinate(double coordinate, string positiveDirection, string negativeDirection)
    {
        // Determine the direction (N/S or E/W) and format the coordinate with the correct direction
        if (coordinate >= 0)
            return $"{Math.Abs(coordinate):F6}{positiveDirection}".Replace(',', '.'); // Positive -> N or E
        else
            return $"{Math.Abs(coordinate):F6}{negativeDirection}".Replace(',', '.'); // Negative -> S or W
    }
    private double ParseCoordinate(string coordinate, string positiveDirection, string negativeDirection)
    {
        // Remove the directional character
        string numericPart = coordinate.Substring(0, coordinate.Length - 1).Trim();
        char direction = coordinate[coordinate.Length - 1];

        double result = double.Parse(numericPart, CultureInfo.InvariantCulture);

        // Adjust the sign based on the direction
        if (direction == negativeDirection[0])  // If the direction is negative (S, W)
            result = -result;
        else if (direction != positiveDirection[0])  // If it's not the correct positive direction (N, E)
            throw new FormatException($"Invalid direction in coordinate: {coordinate}");

        return result;
    }
}
