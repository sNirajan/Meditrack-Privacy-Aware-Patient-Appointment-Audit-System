using MediTrack.Api.Data;
using MediTrack.Api.Dtos;
using MediTrack.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MediTrack.Api.Services;

public class AppointmentService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly AuditLogService _auditLogService;

    // Gives this service access to the currently authenticated user's ID and role from the JWT.
    private readonly CurrentUserService _currentUserService;

    // ASP.NET Core gives us the database context through dependency injection.
    // This service uses it to check patients, providers, and appointments.

    // database context handles appointment data
    // audit service records important appointment actions
    public AppointmentService(
        ApplicationDbContext dbContext,
        AuditLogService auditLogService,
        CurrentUserService currentUserService
    )
    {
        _dbContext = dbContext;
        _auditLogService = auditLogService;
        _currentUserService = currentUserService;
    }

    // Creates a new appointment after checking the patient, provider and date
    public async Task<AppointmentResponse> CreateAppointmentAsync(CreateAppointmentRequest request)
    {
        if (request.PatientId == Guid.Empty)
        {
            throw new ArgumentException("PatientId is required");
        }

        if (request.ProviderId == Guid.Empty)
        {
            throw new ArgumentException("ProviderId is required");
        }

        if (request.AppointmentDateUtc <= DateTime.UtcNow)
        {
            throw new ArgumentException("Appointment date must be in the future");
        }

        var patient = await _dbContext.Patients.FirstOrDefaultAsync(patient =>
            patient.Id == request.PatientId
        );

        if (patient is null)
        {
            throw new ArgumentException("Patient doesn't exist.");
        }

        var provider = await _dbContext.Providers.FirstOrDefaultAsync(provider =>
            provider.Id == request.ProviderId
        );

        if (provider is null)
        {
            throw new ArgumentException("Provider doesn't exist");
        }

        var appointment = new Appointment
        {
            PatientId = request.PatientId,
            ProviderId = request.ProviderId,
            AppointmentDateUtc = request.AppointmentDateUtc,
            Reason = request.Reason,
            Status = "Scheduled",

            // These navigation properties let us return readable names immediately
            Patient = patient,
            Provider = provider,
        };

        _dbContext.Appointments.Add(appointment);

        // This sends the insert operation to Azure SQL
        await _dbContext.SaveChangesAsync();

        await _auditLogService.RecordAsync(
            userId: _currentUserService.UserId,
            userRole: _currentUserService.UserRole,
            action: "CreatedAppointment",
            entityType: "Appointment",
            entityId: appointment.Id,
            details: $"Created appointment for {patient.FullName} with {provider.FullName}."
        );

        return MapToResponse(appointment);
    }

    // Gets all appointments and includes patient/provider details
    public async Task<List<AppointmentResponse>> GetAllAppointmentsAsync()
    {
        var appointments = await _dbContext
            .Appointments.Include(appointment => appointment.Patient)
            .Include(appointment => appointment.Provider)
            .OrderBy(appointment => appointment.AppointmentDateUtc)
            .ToListAsync();

        return appointments.Select(MapToResponse).ToList();
    }

    // Gets one appointment by ID
    public async Task<AppointmentResponse?> GetAppointmentByIdAsync(Guid id)
    {
        var appointment = await _dbContext
            .Appointments.Include(appointment => appointment.Patient)
            .Include(appointment => appointment.Provider)
            .FirstOrDefaultAsync(appointment => appointment.Id == id);

        if (appointment is null)
        {
            return null;
        }
        return MapToResponse(appointment);
    }

    // Cancels an existing appointment by changing its status
    // We do not delete the appointment because healthcare style systems should keep history
    public async Task<AppointmentResponse?> CancelAppointmentAsync(Guid id)
    {
        var appointment = await _dbContext
            .Appointments.Include(appointment => appointment.Patient)
            .Include(appointment => appointment.Provider)
            .FirstOrDefaultAsync(appointment => appointment.Id == id);

        if (appointment is null)
        {
            return null;
        }

        appointment.Status = "Cancelled";
        await _dbContext.SaveChangesAsync();

        await _auditLogService.RecordAsync(
            userId: _currentUserService.UserId,
            userRole: _currentUserService.UserRole,
            action: "CancelledAppointment",
            entityType: "Appointment",
            entityId: appointment.Id,
            details: $"Cancelled appointment for {appointment.Patient?.FullName} with {appointment.Provider?.FullName}."
        );
        return MapToResponse(appointment);
    }

    // converts the database model into the API response shape
    private static AppointmentResponse MapToResponse(Appointment appointment)
    {
        return new AppointmentResponse
        {
            Id = appointment.Id,
            PatientId = appointment.PatientId,
            ProviderId = appointment.ProviderId,
            AppointmentDateUtc = appointment.AppointmentDateUtc,
            Status = appointment.Status,
            Reason = appointment.Reason,
            CreatedAtUtc = appointment.CreatedAtUtc,
            PatientName = appointment.Patient?.FullName ?? string.Empty,
            ProviderName = appointment.Provider?.FullName ?? string.Empty,
        };
    }
}
