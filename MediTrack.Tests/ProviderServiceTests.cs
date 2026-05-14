using System.Security.Claims;
using MediTrack.Api.Data;
using MediTrack.Api.Dtos;
using MediTrack.Api.Models;
using MediTrack.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace MediTrack.Tests;

public class ProviderServiceTests
{
    // Creates a fresh in-memory database for each test.
    // This keeps tests isolated and avoids touching Azure SQL.
    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    // Creates a fake authenticated Admin user for audit log testing.
    // In the real app, this information comes from the JWT token.
    private static CurrentUserService CreateCurrentUserService()
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "test-admin-user-id"),
            new Claim(ClaimTypes.Role, "Admin"),
        };

        var identity = new ClaimsIdentity(claims, authenticationType: "TestAuth");
        var user = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = user };

        var httpContextAccessor = new HttpContextAccessor { HttpContext = httpContext };

        return new CurrentUserService(httpContextAccessor);
    }

    // Builds ProviderService with real dependencies using the in-memory database.
    private static ProviderService CreateProviderService(ApplicationDbContext dbContext)
    {
        var auditLogService = new AuditLogService(dbContext);
        var currentUserService = CreateCurrentUserService();

        return new ProviderService(dbContext, auditLogService, currentUserService);
    }

    [Fact]
    public async Task CreateProviderAsync_CreatesProvider_WhenRequestIsValid()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var providerService = CreateProviderService(dbContext);

        var request = new CreateProviderRequest
        {
            FullName = "Dr. Sarah Lee",
            Email = "sarah.lee@example.com",
            Specialty = "Family Medicine",
        };

        // Act
        var response = await providerService.CreateProviderAsync(request);

        // Assert
        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Equal("Dr. Sarah Lee", response.FullName);
        Assert.Equal("sarah.lee@example.com", response.Email);
        Assert.Equal("Family Medicine", response.Specialty);

        var providerInDatabase = await dbContext.Providers.SingleAsync();

        Assert.Equal(response.Id, providerInDatabase.Id);
        Assert.Equal("Dr. Sarah Lee", providerInDatabase.FullName);
    }

    [Fact]
    public async Task CreateProviderAsync_RecordsAuditLog_WhenProviderIsCreated()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var providerService = CreateProviderService(dbContext);

        var request = new CreateProviderRequest
        {
            FullName = "Dr. Audit Provider",
            Email = "audit.provider@example.com",
            Specialty = "Internal Medicine",
        };

        // Act
        var response = await providerService.CreateProviderAsync(request);

        // Assert
        var auditLog = await dbContext.AuditLogs.SingleAsync(auditLog =>
            auditLog.Action == "CreatedProvider" && auditLog.EntityType == "Provider"
        );

        Assert.Equal(response.Id, auditLog.EntityId);
        Assert.Equal("test-admin-user-id", auditLog.UserId);
        Assert.Equal("Admin", auditLog.UserRole);
        Assert.Contains("Created provider", auditLog.Details);
    }

    [Fact]
    public async Task GetProviderByIdAsync_ReturnsProvider_WhenProviderExists()
    {
        // Arrange
        await using var dbContext = CreateDbContext();

        var provider = new Provider
        {
            FullName = "Dr. View Provider",
            Email = "view.provider@example.com",
            Specialty = "Cardiology",
        };

        dbContext.Providers.Add(provider);
        await dbContext.SaveChangesAsync();

        var providerService = CreateProviderService(dbContext);

        // Act
        var response = await providerService.GetProviderByIdAsync(provider.Id);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(provider.Id, response.Id);
        Assert.Equal("Dr. View Provider", response.FullName);
        Assert.Equal("Cardiology", response.Specialty);
    }

    [Fact]
    public async Task GetProviderByIdAsync_ReturnsNull_WhenProviderDoesNotExist()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var providerService = CreateProviderService(dbContext);

        // Act
        var response = await providerService.GetProviderByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(response);
    }
}
