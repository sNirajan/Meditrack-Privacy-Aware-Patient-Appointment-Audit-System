using System.Security.Claims;
using MediTrack.Api.Data;
using MediTrack.Api.Dtos;
using MediTrack.Api.Models;
using MediTrack.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace MediTrack.Tests;

public class PatientServiceTests
{
    // Creates a fresh in-memory database for each test.
    // This keeps tests isolated and avoids touching the real Azure SQL database.
    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    // Creates a fake authenticated Admin user for testing audit logs.
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

    // Builds PatientService with real dependencies using the in-memory database.
    private static PatientService CreatePatientService(ApplicationDbContext dbContext)
    {
        var auditLogService = new AuditLogService(dbContext);
        var currentUserService = CreateCurrentUserService();

        return new PatientService(dbContext, auditLogService, currentUserService);
    }

    [Fact]
    public async Task CreatePatientAsync_CreatesPatient_WhenRequestIsValid()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var patientService = CreatePatientService(dbContext);

        var request = new CreatePatientRequest
        {
            FullName = "Jane Doe",
            Email = "jane.doe@example.com",
            DateOfBirth = new DateOnly(1996, 3, 15),
            PhoneNumber = "+1 204-555-1234",
        };

        // Act
        var response = await patientService.CreatePatientAsync(request);

        // Assert
        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Equal("Jane Doe", response.FullName);
        Assert.Equal("jane.doe@example.com", response.Email);
        Assert.Equal(new DateOnly(1996, 3, 15), response.DateOfBirth);

        var patientInDatabase = await dbContext.Patients.SingleAsync();

        Assert.Equal(response.Id, patientInDatabase.Id);
        Assert.Equal("Jane Doe", patientInDatabase.FullName);
    }

    [Fact]
    public async Task CreatePatientAsync_RecordsAuditLog_WhenPatientIsCreated()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var patientService = CreatePatientService(dbContext);

        var request = new CreatePatientRequest
        {
            FullName = "Audit Patient",
            Email = "audit.patient@example.com",
            DateOfBirth = new DateOnly(1994, 7, 20),
            PhoneNumber = "+1 204-555-2222",
        };

        // Act
        var response = await patientService.CreatePatientAsync(request);

        // Assert
        var auditLog = await dbContext.AuditLogs.SingleAsync(auditLog =>
            auditLog.Action == "CreatedPatient" && auditLog.EntityType == "Patient"
        );

        Assert.Equal(response.Id, auditLog.EntityId);
        Assert.Equal("test-admin-user-id", auditLog.UserId);
        Assert.Equal("Admin", auditLog.UserRole);
        Assert.Contains("Created patient", auditLog.Details);
    }

    [Fact]
    public async Task GetPatientByIdAsync_ReturnsPatient_WhenPatientExists()
    {
        // Arrange
        await using var dbContext = CreateDbContext();

        var patient = new Patient
        {
            FullName = "View Test Patient",
            Email = "view.patient@example.com",
            DateOfBirth = new DateOnly(1990, 1, 1),
            PhoneNumber = "+1 204-555-3333",
        };

        dbContext.Patients.Add(patient);
        await dbContext.SaveChangesAsync();

        var patientService = CreatePatientService(dbContext);

        // Act
        var response = await patientService.GetPatientByIdAsync(patient.Id);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(patient.Id, response.Id);
        Assert.Equal("View Test Patient", response.FullName);
    }

    [Fact]
    public async Task GetPatientByIdAsync_ReturnsNull_WhenPatientDoesNotExist()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var patientService = CreatePatientService(dbContext);

        // Act
        var response = await patientService.GetPatientByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(response);
    }

    [Fact]
    public async Task GetPatientByIdAsync_RecordsAuditLog_WhenPatientIsViewed()
    {
        // Arrange
        await using var dbContext = CreateDbContext();

        var patient = new Patient
        {
            FullName = "Viewed Patient",
            Email = "viewed.patient@example.com",
            DateOfBirth = new DateOnly(1988, 5, 10),
            PhoneNumber = "+1 204-555-4444",
        };

        dbContext.Patients.Add(patient);
        await dbContext.SaveChangesAsync();

        var patientService = CreatePatientService(dbContext);

        // Act
        var response = await patientService.GetPatientByIdAsync(patient.Id);

        // Assert
        Assert.NotNull(response);

        var auditLog = await dbContext.AuditLogs.SingleAsync(auditLog =>
            auditLog.Action == "ViewedPatient" && auditLog.EntityType == "Patient"
        );

        Assert.Equal(patient.Id, auditLog.EntityId);
        Assert.Equal("test-admin-user-id", auditLog.UserId);
        Assert.Equal("Admin", auditLog.UserRole);
        Assert.Contains("Viewed patient", auditLog.Details);
    }
}
