namespace RaceDay.Api.Models;

public class Enrollment
{
    public int Id { get; set; }
    public int ParticipantId { get; set; }
    public User? Participant { get; set; }
    public int EventId { get; set; }
    public RaceEvent? Event { get; set; }
    public int CategoryId { get; set; }
    public EventCategory? Category { get; set; }
    public DateTime EnrolledAtUtc { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Confirmed";
    public RaceResult? Result { get; set; }
}
