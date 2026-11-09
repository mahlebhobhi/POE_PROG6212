using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

public class RaceEvent
{
    public int Id { get; set; }
    [Required, MaxLength(120)] public string Name { get; set; } = "";
    [Required, MaxLength(500)] public string Description { get; set; } = "";
    public DateTime EventDate { get; set; }
    [Required, MaxLength(200)] public string Location { get; set; } = "";
    public double DistanceKm { get; set; }
    [Required, MaxLength(20)] public string EventType { get; set; } = "Run";
    public int OrganiserId { get; set; }
    [MaxLength(1000)] public string? BannerImageUrl { get; set; }
    public User? Organiser { get; set; }
    public List<EventCategory> Categories { get; set; } = new();
    public List<Enrollment> Enrollments { get; set; } = new();
}
