using MediTrack.Api.Data;
using MediTrack.Api.Dtos;
using MediTrack.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace MediTrack.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    // ASP.NET Core gives us AuthService through dependency injection.
    // The controller exposes register/login endpoints, while AuthService handles the real logic.

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    // POST /api/auth/register
    // Creates a new user account, assigns a role, and returns a JWT token
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var response = await _authService.RegisterAsync(request);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // POST /api/auth/login
    // Checks the user's email/password and retuns a jwt token if valid
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
