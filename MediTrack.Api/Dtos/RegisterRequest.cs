using System.ComponentModel.DataAnnotations;

namespace MediTrack.Api.Dtos;

public class RegisterRequest
{
    // Email will be used as both the email and username for login
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    // password is required for the account
    // Identity will hash it before saving it to the database
    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    // Role decides what the user is allowed to do
    // Expected values: Admin, Provider, Patient
    [Required]
    public string Role { get; set; } = string.Empty;
}
