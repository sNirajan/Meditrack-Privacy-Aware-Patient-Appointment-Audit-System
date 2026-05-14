using System.Security.Claims;
using MediTrack.Api.Data;
using MediTrack.Api.Dtos;
using MediTrack.Api.Models;
using MediTrack.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace MediTrack.Tests;

public class AppointmentServiceTests
{
    // Creates a fresh in-memory database for each test.
    // This avoids using the real Azure SQL database during unit testing.
    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    // Creates a fake authenticated user for tests.
    // CurrentUserService normally reads user details from the JWT in HttpContext.
    // In tests, we manually create a fake HttpContext with user claims.
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

    // Creates AppointmentService with its real dependencies.
    // We use the real AuditLogService because it also writes to the in-memory database.
    private static AppointmentService CreateAppointmentService(ApplicationDbContext dbContext)
    {
        var auditLogService = new AuditLogService(dbContext);
        var currentUserService = CreateCurrentUserService();

        return new AppointmentService(dbContext, auditLogService, currentUserService);
    }

    [Fact]
    public async Task CreateAppointmentAsync_Throws_WhenAppointmentDateIsInPast()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var appointmentService = CreateAppointmentService(dbContext);

        var request = new CreateAppointmentRequest
        {
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            AppointmentDateUtc = DateTime.UtcNow.AddDays(-1),
            Reason = "Past appointment test",
        };

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            appointmentService.CreateAppointmentAsync(request)
        );

        // Assert
        Assert.Contains("future", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateAppointmentAsync_Throws_WhenPatientDoesNotExist()
    {
        // Arrange
        await using var dbContext = CreateDbContext();

        var provider = new Provider
        {
            FullName = "Dr. Test Provider",
            Email = "provider@example.com",
            Specialty = "Family Medicine",
        };

        dbContext.Providers.Add(provider);
        await dbContext.SaveChangesAsync();

        var appointmentService = CreateAppointmentService(dbContext);

        var request = new CreateAppointmentRequest
        {
            PatientId = Guid.NewGuid(),
            ProviderId = provider.Id,
            AppointmentDateUtc = DateTime.UtcNow.AddDays(1),
            Reason = "Missing patient test",
        };

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            appointmentService.CreateAppointmentAsync(request)
        );

        // Assert
        Assert.Contains("Patient", exception.Message);
        Assert.Contains("exist", exception.Message);
    }

    [Fact]
    public async Task CreateAppointmentAsync_Throws_WhenProviderDoesNotExist()
    {
        // Arrange
        await using var dbContext = CreateDbContext();

        var patient = new Patient
        {
            FullName = "Test Patient",
            Email = "patient@example.com",
            DateOfBirth = new DateOnly(1995, 1, 10),
            PhoneNumber = "+1 204-555-1111",
        };

        dbContext.Patients.Add(patient);
        await dbContext.SaveChangesAsync();

        var appointmentService = CreateAppointmentService(dbContext);

        var request = new CreateAppointmentRequest
        {
            PatientId = patient.Id,
            ProviderId = Guid.NewGuid(),
            AppointmentDateUtc = DateTime.UtcNow.AddDays(1),
            Reason = "Missing provider test",
        };

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            appointmentService.CreateAppointmentAsync(request)
        );

        // Assert
        Assert.Contains("Provider", exception.Message);
        Assert.Contains("exist", exception.Message);
    }

    [Fact]
    public async Task CreateAppointmentAsync_CreatesScheduledAppointment_WhenRequestIsValid()
    {
        // Arrange
        await using var dbContext = CreateDbContext();

        var patient = new Patient
        {
            FullName = "John Doe",
            Email = "john.doe@example.com",
            DateOfBirth = new DateOnly(1995, 1, 10),
            PhoneNumber = "+1 204-555-1234",
        };

        var provider = new Provider
        {
            FullName = "Dr. Sarah Lee",
            Email = "sarah.lee@example.com",
            Specialty = "Family Medicine",
        };

        dbContext.Patients.Add(patient);
        dbContext.Providers.Add(provider);
        await dbContext.SaveChangesAsync();

        var appointmentService = CreateAppointmentService(dbContext);

        var request = new CreateAppointmentRequest
        {
            PatientId = patient.Id,
            ProviderId = provider.Id,
            AppointmentDateUtc = DateTime.UtcNow.AddDays(1),
            Reason = "Follow-up visit",
        };

        // Act
        var response = await appointmentService.CreateAppointmentAsync(request);

        // Assert
        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Equal(patient.Id, response.PatientId);
        Assert.Equal(provider.Id, response.ProviderId);
        Assert.Equal("Scheduled", response.Status);
        Assert.Equal("Follow-up visit", response.Reason);
        Assert.Equal("John Doe", response.PatientName);
        Assert.Equal("Dr. Sarah Lee", response.ProviderName);

        var appointmentInDatabase = await dbContext.Appointments.SingleAsync();
        Assert.Equal("Scheduled", appointmentInDatabase.Status);
    }

    [Fact]
    public async Task CreateAppointmentAsync_RecordsAuditLog_WhenAppointmentIsCreated()
    {
        // Arrange
        await using var dbContext = CreateDbContext();

        var patient = new Patient
        {
            FullName = "John Doe",
            Email = "john.audit@example.com",
            DateOfBirth = new DateOnly(1995, 1, 10),
            PhoneNumber = "+1 204-555-2222",
        };

        var provider = new Provider
        {
            FullName = "Dr. Sarah Lee",
            Email = "provider.audit@example.com",
            Specialty = "Family Medicine",
        };

        dbContext.Patients.Add(patient);
        dbContext.Providers.Add(provider);
        await dbContext.SaveChangesAsync();

        var appointmentService = CreateAppointmentService(dbContext);

        var request = new CreateAppointmentRequest
        {
            PatientId = patient.Id,
            ProviderId = provider.Id,
            AppointmentDateUtc = DateTime.UtcNow.AddDays(1),
            Reason = "Audit log test",
        };

        // Act
        var response = await appointmentService.CreateAppointmentAsync(request);

        // Assert
        var auditLog = await dbContext.AuditLogs.SingleAsync();

        Assert.Equal("CreatedAppointment", auditLog.Action);
        Assert.Equal("Appointment", auditLog.EntityType);
        Assert.Equal(response.Id, auditLog.EntityId);
        Assert.Equal("test-admin-user-id", auditLog.UserId);
        Assert.Equal("Admin", auditLog.UserRole);
        Assert.Contains("Created appointment", auditLog.Details);
    }
}
