using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Security;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(RaceDayDbContext db, CurrentUser currentUser, BlobStorageService blobs) : ControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        if (!currentUser.IsAuthenticated) return Unauthorized();
        var user = await db.Users.FindAsync(currentUser.Id);
        if (user == null) return Unauthorized();
        return Ok(new UserResponse(user.Id, user.FullName, user.Email, user.Role, user.ProfileImageUrl));
    }


    [HttpPost("me/picture")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadPicture(IFormFile file, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated) return Unauthorized();
        if (file == null) return BadRequest(new { message = "An image file is required." });
        var user = await db.Users.FindAsync(currentUser.Id);
        if (user == null) return Unauthorized();
        try { user.ProfileImageUrl = await blobs.UploadAsync(file, "profiles", cancellationToken); await db.SaveChangesAsync(); return Ok(new { url = user.ProfileImageUrl }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe(UpdateProfileRequest request)
    {
        if (!currentUser.IsAuthenticated) return Unauthorized();
        var user = await db.Users.FindAsync(currentUser.Id);
        if (user == null) return Unauthorized();
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(x => x.Email == email && x.Id != user.Id))
            return Conflict(new { message = "That email is already in use." });
        user.FullName = request.FullName.Trim();
        user.Email = email;
        await db.SaveChangesAsync();
        return Ok(new UserResponse(user.Id, user.FullName, user.Email, user.Role, user.ProfileImageUrl));
    }
}
