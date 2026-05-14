using System.Security.Claims;

namespace MediTrack.Api.Services;

// CurrentUserService reads the user identity after ASP.NET Core has already validated the JWT.
public class CurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    // IHttpContextAccessor gives us access to the current HTTP request.
    // We use it to read the logged-in user's claims from the JWT token.
    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // Gets the logged-in user's ID from the JWT token.
    // This value comes from the ClaimTypes.NameIdentifier claim we added when generating the token.
    // If the request is not authenticated, we fall back to "system" so audit logging still works.
    public string UserId
    {
        get
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            return string.IsNullOrWhiteSpace(userId) ? "system" : userId;
        }
    }

    // Gets the logged-in user's role from the JWT token.
    // Examples: Admin, Provider, Patient.
    // If the request is not authenticated, we fall back to "System".
    public string UserRole
    {
        get
        {
            var userRole = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role);

            return string.IsNullOrWhiteSpace(userRole) ? "System" : userRole;
        }
    }

    // Simple helper for places where we want to check admin access manually
    // Most of the time, [Authorize(Roles= "Admin")] is better for controller endpoints
    public bool IsAdmin()
    {
        return string.Equals(UserRole, "Admin", StringComparison.OrdinalIgnoreCase);
    }
}
