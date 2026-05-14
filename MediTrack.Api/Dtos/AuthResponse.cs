namespace MediTrack.Api.Dtos;

public class AuthResponse
{
    // The JWT token the client will send in Authorization header
    public string Token { get; set; } = string.Empty;

    // User ID from ASP.NET Core identity
    public string UserId { get; set; } = string.Empty;

    // User email
    public string Email { get; set; } = string.Empty;

    // User role, for example: Admin, Provider, Patient
    public string Role { get; set; } = string.Empty;

    // When the token expires
    public DateTime ExpiresAtUtc { get; set; }
}
