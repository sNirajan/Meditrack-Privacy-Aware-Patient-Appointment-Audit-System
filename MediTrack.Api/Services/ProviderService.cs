using MediTrack.Api.Data;
using MediTrack.Api.Dtos;
using MediTrack.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MediTrack.Api.Services;

public class ProviderService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly AuditLogService _auditLogService;

    // Gives this service access to the currently authenticated user's ID and role from the JWT.
    private readonly CurrentUserService _currentUserService;

    // ASP.Net Core gives us ApplicationDbContext through dependency injection
    // database context handles appoinment data
    // audit service records important appointment actions

    public ProviderService(
        ApplicationDbContext dbContext,
        AuditLogService auditLogService,
        CurrentUserService currentUserService
    )
    {
        _dbContext = dbContext;
        _auditLogService = auditLogService;
        _currentUserService = currentUserService;
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

        await _auditLogService.RecordAsync(
            userId: _currentUserService.UserId,
            userRole: _currentUserService.UserRole,
            action: "CreatedProvider",
            entityType: "Provider",
            entityId: provider.Id,
            details: $"Created provider record for {provider.FullName}."
        );

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
