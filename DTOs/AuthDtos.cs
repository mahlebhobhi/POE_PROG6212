using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs;

public class RegisterRequest
{
    [Required, MaxLength(100)] public string FullName { get; set; } = "";
    [Required, EmailAddress, MaxLength(200)] public string Email { get; set; } = "";
    [Required, MinLength(8)] public string Password { get; set; } = "";
    [Required] public string Role { get; set; } = "Participant";
}
public class LoginRequest
{
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required] public string Password { get; set; } = "";
}
public record UserResponse(int Id, string FullName, string Email, string Role, string? ProfileImageUrl = null);
public class UpdateProfileRequest
{
    [Required, MaxLength(100)] public string FullName { get; set; } = "";
    [Required, EmailAddress, MaxLength(200)] public string Email { get; set; } = "";
}
