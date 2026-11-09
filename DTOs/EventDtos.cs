using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs;

public class EventRequest
{
    [Required, MaxLength(120)] public string Name { get; set; } = "";
    [Required, MaxLength(500)] public string Description { get; set; } = "";
    public DateTime EventDate { get; set; }
    [Required, MaxLength(200)] public string Location { get; set; } = "";
    [Range(0.1, 1000)] public double DistanceKm { get; set; }
    [Required] public string EventType { get; set; } = "Run";
}
public class CategoryRequest
{
    [Required, MaxLength(80)] public string Name { get; set; } = "";
    [Range(0, 120)] public int? MinimumAge { get; set; }
    [Range(0, 120)] public int? MaximumAge { get; set; }
    [Range(0.1, 1000)] public double? DistanceKm { get; set; }
}
public class EnrollmentRequest
{
    [Range(1, int.MaxValue)] public int CategoryId { get; set; }
}
public class ResultRequest
{
    [Range(1, int.MaxValue)] public int EnrollmentId { get; set; }
    public TimeSpan FinishTime { get; set; }
    [Range(1, int.MaxValue)] public int FinishingPosition { get; set; }
}
