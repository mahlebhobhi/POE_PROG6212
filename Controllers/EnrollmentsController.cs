using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;
using RaceDay.Api.Security;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/enrollments")]
public class EnrollmentsController(RaceDayDbContext db, CurrentUser currentUser) : ControllerBase
{
    [HttpPost("events/{eventId:int}")]
    public async Task<IActionResult> Enrol(int eventId, EnrollmentRequest request)
    {
        if (!currentUser.IsAuthenticated) return Unauthorized();
        if (!currentUser.IsRole("Participant")) return Forbid();
        var category = await db.Categories.FirstOrDefaultAsync(x => x.Id == request.CategoryId && x.EventId == eventId);
        if (category == null) return BadRequest(new { message = "Category does not belong to this event." });
        if (await db.Enrollments.AnyAsync(x => x.ParticipantId == currentUser.Id && x.EventId == eventId && x.CategoryId == category.Id))
            return Conflict(new { message = "You are already enrolled in this category." });
        var enrollment = new Enrollment { ParticipantId = currentUser.Id!.Value, EventId = eventId, CategoryId = category.Id };
        db.Enrollments.Add(enrollment);
        await db.SaveChangesAsync();
        return Created($"/api/enrollments/{enrollment.Id}", enrollment);
    }

    [HttpGet("mine")]
    public async Task<IActionResult> Mine()
    {
        if (!currentUser.IsAuthenticated) return Unauthorized();
        if (!currentUser.IsRole("Participant")) return Forbid();
        return Ok(await db.Enrollments.Include(x => x.Event).Include(x => x.Category)
            .Where(x => x.ParticipantId == currentUser.Id)
            .Select(x => new { x.Id, x.EventId, x.CategoryId, x.EnrolledAtUtc, x.Status, Category = x.Category!.Name,
                Event = new { x.Event!.Id, x.Event.Name, x.Event.EventDate, x.Event.Location, x.Event.DistanceKm, x.Event.EventType, x.Event.BannerImageUrl } })
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
        return Ok(await db.Enrollments.Include(x => x.Participant).Include(x => x.Category)
            .Where(x => x.EventId == eventId).Select(x => new {
                x.Id, x.EventId, x.CategoryId, x.Status, Category = x.Category!.Name,
                ParticipantId = x.ParticipantId, ParticipantName = x.Participant!.FullName,
                x.EnrolledAtUtc
            }).ToListAsync());
    }
}
