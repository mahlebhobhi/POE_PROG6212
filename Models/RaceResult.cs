using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

public class RaceResult
{
    public int Id { get; set; }
    public int EnrollmentId { get; set; }
    public Enrollment? Enrollment { get; set; }
    public TimeSpan FinishTime { get; set; }
    [Range(1, int.MaxValue)] public int FinishingPosition { get; set; }
    public DateTime RecordedAtUtc { get; set; } = DateTime.UtcNow;
}
