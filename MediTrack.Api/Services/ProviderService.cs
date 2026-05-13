using MediTrack.Api.Data;
using MediTrack.Api.Dtos;
using MediTrack.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MediTrack.Api.Services;

public class ProviderService
{
    private readonly ApplicationDbContext _dbContext;

    // ASP.Net Core gives us ApplicationDbContext through dependency injection
    // This service uses it to read and write provider records

    public ProviderService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Creates a new provider record from the request data
    public async Task<ProviderResponse> CreateProviderAsync(CreateProviderRequest request)
    {
        var provider = new Provider
        {
            FullName = request.FullName,
            Email = request.Email,
            Specialty = request.Specialty,
        };

        _dbContext.Providers.Add(provider);

        // This sends the insert operation to Azure SQL
        await _dbContext.SaveChangesAsync();

        return MapToResponse(provider);
    }

    // Gets all providers from the database
    public async Task<List<ProviderResponse>> GetAllProvidersAsync()
    {
        var providers = await _dbContext
            .Providers.OrderBy(provider => provider.FullName)
            .ToListAsync();

        return providers.Select(MapToResponse).ToList();
    }

    // Gets one provider by ID
    // Returns null of the provider doesn't exist

    public async Task<ProviderResponse?> GetProviderByIdAsync(Guid id)
    {
        var provider = await _dbContext.Providers.FirstOrDefaultAsync(provider =>
            provider.Id == id
        );

        if (provider is null)
        {
            return null;
        }
        return MapToResponse(provider);
    }

    // Converts the database model into the response DTO
    // This keeps the API response seperate from the database entity
    private static ProviderResponse MapToResponse(Provider provider)
    {
        return new ProviderResponse
        {
            Id = provider.Id,
            FullName = provider.FullName,
            Email = provider.Email,
            Specialty = provider.Specialty,
            CreatedAtUtc = provider.CreatedAtUtc,
        };
    }
}
