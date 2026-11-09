using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;
using RaceDay.Api.Security;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/results")]
public class ResultsController(RaceDayDbContext db, CurrentUser currentUser) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Record(ResultRequest request)
    {
        if (!currentUser.IsAuthenticated) return Unauthorized();
        if (!currentUser.IsRole("Organiser")) return Forbid();
        if (request.FinishTime < TimeSpan.Zero) return BadRequest(new { message = "Finish time cannot be negative." });
        var enrollment = await db.Enrollments.Include(x => x.Event).FirstOrDefaultAsync(x => x.Id == request.EnrollmentId);
        if (enrollment == null) return NotFound(new { message = "Enrollment not found." });
        if (enrollment.Event!.OrganiserId != currentUser.Id) return Forbid();
        if (await db.Results.AnyAsync(x => x.EnrollmentId == enrollment.Id))
            return Conflict(new { message = "A result already exists for this enrollment." });
        var result = new RaceResult { EnrollmentId = enrollment.Id, FinishTime = request.FinishTime, FinishingPosition = request.FinishingPosition };
        db.Results.Add(result);
        await db.SaveChangesAsync();
        return Created($"/api/results/{result.Id}", result);
    }

    [HttpGet("mine")]
    public async Task<IActionResult> Mine()
    {
        if (!currentUser.IsAuthenticated) return Unauthorized();
        if (!currentUser.IsRole("Participant")) return Forbid();
        return Ok(await db.Results.Include(x => x.Enrollment).ThenInclude(x => x!.Event)
            .Include(x => x.Enrollment).ThenInclude(x => x!.Category)
            .Where(x => x.Enrollment!.ParticipantId == currentUser.Id)
            .Select(x => new { x.Id, EventName = x.Enrollment!.Event!.Name, EventDate = x.Enrollment.Event.EventDate, Category = x.Enrollment.Category!.Name, x.FinishTime, x.FinishingPosition, TotalFinishers = db.Results.Count(r => r.Enrollment!.EventId == x.Enrollment.EventId) })
            .ToListAsync());
    }

    [HttpGet("events/{eventId:int}")]
    public async Task<IActionResult> ForEvent(int eventId)
    {
        if (!currentUser.IsAuthenticated) return Unauthorized();
        if (!currentUser.IsRole("Organiser")) return Forbid();
        var item = await db.Events.FindAsync(eventId);
        if (item == null) return NotFound();
        if (item.OrganiserId != currentUser.Id) return Forbid();
        return Ok(await db.Results.Include(x => x.Enrollment).ThenInclude(x => x!.Participant)
            .Where(x => x.Enrollment!.EventId == eventId)
            .OrderBy(x => x.FinishingPosition)
            .Select(x => new { x.Id, Participant = x.Enrollment!.Participant!.FullName, x.FinishTime, x.FinishingPosition })
            .ToListAsync());
    }
}
