using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MediTrack.Api.Dtos;
using MediTrack.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace MediTrack.Api.Services;

public class AuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;

    // UserManager handles user-related operations:
    // creating users, finding users, checking passwords, and assigning roles.
    //
    // RoleManager handles role-related operations:
    // checking if roles exist and creating roles like Admin, Provider, and Patient.
    //
    // IConfiguration lets us read settings from appsettings.Development.json,
    // user-secrets, or environment variables.
    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration
    )
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }

    // Registers a new user account.
    // This method:
    // 1. Validates the role
    // 2. Checks if the email already exists
    // 3. Creates the role if needed
    // 4. Creates the user using ASP.NET Core Identity
    // 5. Assigns the user to the selected role
    // 6. Returns a JWT token
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // NormalizeRole makes sure we store roles consistently as:
        // Admin, Provider, or Patient.
        // This avoids problems like "admin" vs "Admin".
        var normalizedRole = NormalizeRole(request.Role);

        if (normalizedRole is null)
        {
            throw new ArgumentException("Role must be Admin, Provider, or Patient.");
        }

        // Check if a user with this email already exists.
        // We do this before creating a new account to avoid duplicate users.
        var existingUser = await _userManager.FindByEmailAsync(request.Email);

        if (existingUser is not null)
        {
            throw new ArgumentException("A user with this email already exists.");
        }

        // Make sure the role exists in AspNetRoles.
        // If the role does not exist yet, we create it.
        await EnsureRoleExistsAsync(normalizedRole);

        // Create the Identity user object.
        // We use email as both Email and UserName for simpler login.
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            EmailConfirmed = true,
        };

        // CreateAsync hashes the password and saves the user to AspNetUsers.
        // We do not manually store or hash passwords ourselves.
        var createResult = await _userManager.CreateAsync(user, request.Password);

        if (!createResult.Succeeded)
        {
            var errors = string.Join(" ", createResult.Errors.Select(error => error.Description));
            throw new ArgumentException(errors);
        }

        // Assign the newly created user to the selected role.
        // This creates a relationship in AspNetUserRoles.
        var roleResult = await _userManager.AddToRoleAsync(user, normalizedRole);

        if (!roleResult.Succeeded)
        {
            var errors = string.Join(" ", roleResult.Errors.Select(error => error.Description));
            throw new ArgumentException(errors);
        }

        // After registration, return the same response shape as login:
        // token, userId, email, role, and expiry.
        return await CreateAuthResponseAsync(user);
    }

    // Logs in an existing user.
    // This method:
    // 1. Finds the user by email
    // 2. Checks the password using Identity
    // 3. Returns a JWT token if valid
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null)
        {
            // We keep the message generic so attackers cannot know
            // whether the email or password was wrong.
            throw new ArgumentException("Invalid email or password.");
        }

        // Identity checks the raw password against the stored password hash.
        var passwordIsValid = await _userManager.CheckPasswordAsync(user, request.Password);

        if (!passwordIsValid)
        {
            throw new ArgumentException("Invalid email or password.");
        }

        return await CreateAuthResponseAsync(user);
    }

    // Creates the response returned after register/login.
    // This includes the JWT token and basic user information.
    private async Task<AuthResponse> CreateAuthResponseAsync(ApplicationUser user)
    {
        // Get the user's assigned roles from Identity.
        var roles = await _userManager.GetRolesAsync(user);

        // Normalize the role again to make sure the token contains
        // Admin, Provider, or Patient with consistent casing.
        var role = NormalizeRole(roles.FirstOrDefault() ?? "Patient") ?? "Patient";

        // Token expiry.
        // For now, we use 2 hours for development.
        // Later, we could use shorter access tokens with refresh tokens.
        var expiresAtUtc = DateTime.UtcNow.AddHours(2);

        // Generate the signed JWT token.
        var token = GenerateJwtToken(user, role, expiresAtUtc);

        return new AuthResponse
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            Role = role,
            ExpiresAtUtc = expiresAtUtc,
        };
    }

    // Generates a signed JWT token.
    // This token is sent back to the client after login/register.
    // The client later sends it in:
    // Authorization: Bearer <token>
    private string GenerateJwtToken(ApplicationUser user, string role, DateTime expiresAtUtc)
    {
        var jwtIssuer = _configuration["Jwt:Issuer"];
        var jwtAudience = _configuration["Jwt:Audience"];
        var jwtKey = _configuration["Jwt:Key"];

        // If JWT settings are missing, the app should fail clearly.
        // Without these values, we cannot safely generate tokens.
        if (
            string.IsNullOrWhiteSpace(jwtIssuer)
            || string.IsNullOrWhiteSpace(jwtAudience)
            || string.IsNullOrWhiteSpace(jwtKey)
        )
        {
            throw new InvalidOperationException("JWT settings are missing.");
        }

        // Claims are pieces of information stored inside the token.
        // Later, ASP.NET Core reads these claims after validating the token.
        var claims = new List<Claim>
        {
            // Standard JWT subject claim.
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            // Standard JWT email claim.
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            // ASP.NET Core-friendly user ID claim.
            // CurrentUserService reads this value later.
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            // ASP.NET Core-friendly email claim.
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            // Role claim used by [Authorize(Roles = "Admin")].
            new Claim(ClaimTypes.Role, role),
        };

        // Convert the secret key string into bytes and create a signing key.
        // This key is used to sign the token so it cannot be tampered with.
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

        // HS256 means HMAC SHA-256.
        // The same secret key is used to sign and validate the token.
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        // Create the JWT token object with issuer, audience, claims, expiry, and signature.
        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials
        );

        // Convert the token object into the compact string sent to the client.
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Makes sure a role exists in the database.
    // If the role does not exist, create it.
    private async Task EnsureRoleExistsAsync(string role)
    {
        var roleExists = await _roleManager.RoleExistsAsync(role);

        if (!roleExists)
        {
            var result = await _roleManager.CreateAsync(new IdentityRole(role));

            if (!result.Succeeded)
            {
                var errors = string.Join(" ", result.Errors.Select(error => error.Description));
                throw new ArgumentException(errors);
            }
        }
    }

    // Keeps role values controlled and consistent.
    // This prevents storing random role names like "manager" or "doctor"
    // and avoids casing issues like "admin" vs "Admin".
    private static string? NormalizeRole(string role)
    {
        if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return "Admin";
        }

        if (string.Equals(role, "Provider", StringComparison.OrdinalIgnoreCase))
        {
            return "Provider";
        }

        if (string.Equals(role, "Patient", StringComparison.OrdinalIgnoreCase))
        {
            return "Patient";
        }

        return null;
    }
}
