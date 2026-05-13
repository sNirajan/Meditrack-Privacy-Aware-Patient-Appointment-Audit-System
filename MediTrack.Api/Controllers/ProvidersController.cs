using MediTrack.Api.Dtos;
using MediTrack.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace MediTrack.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProvidersController : ControllerBase
{
    private readonly ProviderService _providerService;

    // ASP.NET Core gives us ProviderService through dependency injection.
    // The controller uses the service instead of talking to the database directly.
    public ProvidersController(ProviderService providerService)
    {
        _providerService = providerService;
    }

    // POST /api/providers
    // Creates a new healthcare provider record.
    [HttpPost]
    public async Task<ActionResult<ProviderResponse>> CreateProvider(
        [FromBody] CreateProviderRequest request
    )
    {
        var createdProvider = await _providerService.CreateProviderAsync(request);

        // Returns HTTP 201 Created.
        // Also gives the client a route to fetch the created provider later.
        return CreatedAtAction(
            nameof(GetProviderById),
            new { id = createdProvider.Id },
            createdProvider
        );
    }

    // GET /api/providers
    // Returns all provider records.
    [HttpGet]
    public async Task<ActionResult<List<ProviderResponse>>> GetAllProviders()
    {
        var providers = await _providerService.GetAllProvidersAsync();

        return Ok(providers);
    }

    // GET /api/providers/{id}
    // Returns one provider by ID.
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProviderResponse>> GetProviderById(Guid id)
    {
        var provider = await _providerService.GetProviderByIdAsync(id);

        if (provider is null)
        {
            return NotFound();
        }

        return Ok(provider);
    }
}
