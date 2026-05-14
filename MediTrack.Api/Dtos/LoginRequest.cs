using System.ComponentModel.DataAnnotations;

namespace MediTrack.Api.Dtos;

public class LoginRequest
{
    // The email used during registration
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    // The password used during registration
    [Required]
    public string Password { get; set; } = string.Empty;
}
