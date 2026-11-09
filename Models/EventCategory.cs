using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

public class EventCategory
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public RaceEvent? Event { get; set; }
    [Required, MaxLength(80)] public string Name { get; set; } = "";
    public int? MinimumAge { get; set; }
    public int? MaximumAge { get; set; }
    public double? DistanceKm { get; set; }
    public List<Enrollment> Enrollments { get; set; } = new();
}
