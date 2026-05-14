using MediTrack.Api.Data;
using MediTrack.Api.Dtos;
using MediTrack.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MediTrack.Api.Services;

public class PatientService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly AuditLogService _auditLogService;

    // Gives this service access to the currently authenticated user's ID and role from the JWT.
    private readonly CurrentUserService _currentUserService;

    // ASP.NET Core gives us both ApplicationDbContext and AuditLogService through dependency injection
    // The database context saves patient data, and the audit service records important actions
    public PatientService(
        ApplicationDbContext dbContext,
        AuditLogService auditLogService,
        CurrentUserService currentUserService
    )
    {
        _dbContext = dbContext;
        _auditLogService = auditLogService;
        _currentUserService = currentUserService;
    }

    // Creates a new patient record from the request data
    public async Task<PatientResponse> CreatePatientAsync(CreatePatientRequest request)
    {
        var patient = new Patient
        {
            FullName = request.FullName,
            Email = request.Email,
            DateOfBirth = request.DateOfBirth,
            PhoneNumber = request.PhoneNumber,
        };

        _dbContext.Patients.Add(patient);

        // SaveChangesAsync sends the insert operation to the database
        await _dbContext.SaveChangesAsync();

        // asks to record event (patients actions)
        await _auditLogService.RecordAsync(
            userId: _currentUserService.UserId,
            userRole: _currentUserService.UserRole,
            action: "CreatedPatient",
            entityType: "Patient",
            entityId: patient.Id,
            details: $"Created patient record for {patient.FullName}."
        );

        return MapToResponse(patient);
    }

    // Gets all patients from the database
    public async Task<List<PatientResponse>> GetAllPatientsAsync()
    {
        var patients = await _dbContext.Patients.OrderBy(patient => patient.FullName).ToListAsync();

        return patients.Select(MapToResponse).ToList();
    }

    // Gets one patient by ID, if no patient exists with that ID, return null
    public async Task<PatientResponse?> GetPatientByIdAsync(Guid id)
    {
        var patient = await _dbContext.Patients.FirstOrDefaultAsync(patient => patient.Id == id);

        if (patient is null)
        {
            return null;
        }

        await _auditLogService.RecordAsync(
            userId: _currentUserService.UserId,
            userRole: _currentUserService.UserRole,
            action: "ViewedPatient",
            entityType: "Patient",
            entityId: patient.Id,
            details: $"Viewed patient record for {patient.FullName}."
        );

        return MapToResponse(patient);
    }

    // Converts the database model into the response DTO, this keeps us from exposing the database model directly from the API
    private static PatientResponse MapToResponse(Patient patient)
    {
        return new PatientResponse
        {
            Id = patient.Id,
            FullName = patient.FullName,
            Email = patient.Email,
            DateOfBirth = patient.DateOfBirth,
            PhoneNumber = patient.PhoneNumber,
            CreatedAtUtc = patient.CreatedAtUtc,
        };
    }
}
