namespace MediTrack.Api.Dtos;

public class AuditLogResponse
{
    // The unique ID of this audit log entry
    public Guid Id { get; set; }

    // The user who performed the action
    public string UserId { get; set; } = string.Empty;

    // THe role of the user who performed the action
    // eg: Patient, Provider, Admin
    public string UserRole { get; set; } = string.Empty;

    // What happened
    // Eg: CreatedPatient, ViewewdPatient, CreatedAppointment
    public string Action { get; set; } = string.Empty;

    // What type of record was affected
    // Example: Patient, Provider, Appointment
    public string EntityType { get; set; } = string.Empty;

    // The ID of the affected record, if there is one
    public Guid? EntityId { get; set; }

    // When the action happened
    public DateTime TimestampUtc { get; set; }

    // A readable explanation of the event
    public string Details { get; set; } = string.Empty;
}
