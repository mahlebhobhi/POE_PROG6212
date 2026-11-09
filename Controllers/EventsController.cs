using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;
using RaceDay.Api.Security;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController(RaceDayDbContext db, CurrentUser currentUser, BlobStorageService blobs) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetEvents() =>
        Ok(await db.Events.Include(x => x.Categories).OrderBy(x => x.EventDate).ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetEvent(int id)
    {
        var item = await db.Events.Include(x => x.Categories).FirstOrDefaultAsync(x => x.Id == id);
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> CreateEvent(EventRequest request)
    {
        if (!currentUser.IsAuthenticated) return Unauthorized();
        if (!currentUser.IsRole("Organiser")) return Forbid();
        if (!ValidEventType(request.EventType)) return BadRequest(new { message = "EventType must be Run, Walk or Cycle." });
        var item = new RaceEvent {
            Name = request.Name.Trim(), Description = request.Description.Trim(),
            EventDate = request.EventDate, Location = request.Location.Trim(),
            DistanceKm = request.DistanceKm, EventType = request.EventType,
            OrganiserId = currentUser.Id!.Value
        };
        db.Events.Add(item);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetEvent), new { id = item.Id }, item);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateEvent(int id, EventRequest request)
    {
        if (!currentUser.IsAuthenticated) return Unauthorized();
        if (!currentUser.IsRole("Organiser")) return Forbid();
        var item = await db.Events.FindAsync(id);
        if (item == null) return NotFound();
        if (item.OrganiserId != currentUser.Id) return Forbid();
        if (!ValidEventType(request.EventType)) return BadRequest(new { message = "EventType must be Run, Walk or Cycle." });
        item.Name = request.Name.Trim(); item.Description = request.Description.Trim();
        item.EventDate = request.EventDate; item.Location = request.Location.Trim();
        item.DistanceKm = request.DistanceKm; item.EventType = request.EventType;
        await db.SaveChangesAsync();
        return Ok(item);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        if (!currentUser.IsAuthenticated) return Unauthorized();
        if (!currentUser.IsRole("Organiser")) return Forbid();
        var item = await db.Events.FindAsync(id);
        if (item == null) return NotFound();
        if (item.OrganiserId != currentUser.Id) return Forbid();
        db.Events.Remove(item);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{eventId:int}/categories")]
    public async Task<IActionResult> AddCategory(int eventId, CategoryRequest request)
    {
        if (!currentUser.IsAuthenticated) return Unauthorized();
        if (!currentUser.IsRole("Organiser")) return Forbid();
        var item = await db.Events.FindAsync(eventId);
        if (item == null) return NotFound();
        if (item.OrganiserId != currentUser.Id) return Forbid();
        if (request.MinimumAge.HasValue && request.MaximumAge.HasValue && request.MinimumAge > request.MaximumAge)
            return BadRequest(new { message = "MinimumAge cannot exceed MaximumAge." });
        var category = new EventCategory { EventId = eventId, Name = request.Name.Trim(),
            MinimumAge = request.MinimumAge, MaximumAge = request.MaximumAge, DistanceKm = request.DistanceKm };
        db.Categories.Add(category);
        await db.SaveChangesAsync();
        return Created($"/api/events/{eventId}", category);
    }

    [HttpGet("{eventId:int}/categories")]
    public async Task<IActionResult> GetCategories(int eventId)
    {
        if (!await db.Events.AnyAsync(x => x.Id == eventId)) return NotFound();
        return Ok(await db.Categories.Where(x => x.EventId == eventId).ToListAsync());
    }


    [HttpPost("{id:int}/banner")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadBanner(int id, IFormFile file, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated) return Unauthorized();
        if (!currentUser.IsRole("Organiser")) return Forbid();
        if (file == null) return BadRequest(new { message = "An image file is required." });
        var item = await db.Events.FindAsync(id);
        if (item == null) return NotFound();
        if (item.OrganiserId != currentUser.Id) return Forbid();
        try { item.BannerImageUrl = await blobs.UploadAsync(file, "events", cancellationToken); await db.SaveChangesAsync(); return Ok(new { url = item.BannerImageUrl }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    private static bool ValidEventType(string value) =>
        new[] { "Run", "Walk", "Cycle" }.Contains(value, StringComparer.OrdinalIgnoreCase);
}
