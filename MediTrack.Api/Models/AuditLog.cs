namespace MediTrack.Api.Models;

public class AuditLog
{
    // Unique ID for this audit log entry
    public Guid Id { get; set; } = Guid.NewGuid();

    // The ID of the user who performed the action
    // FOr now, this can come from a fake request header
    // Later, this can come from the logged in user
    public string UserId { get; set; } = string.Empty;

    // The role of the user who performed the action
    // Examples: Patient, Provider, Admin
    public string UserRole { get; set; } = string.Empty;

    // What action happened
    // examples: CreatedPatient, ViewedPatient, CreatedAppointment
    public string Action { get; set; } = string.Empty;

    // What kind of record the action happened to
    // Examples: Patient, Appointment, AuditLog
    public string EntityType { get; set; } = string.Empty;

    // The ID of the specific record that was affected
    // We keep it nullable because some actions may not target one specific record
    public Guid? EntityId { get; set; }

    // When the action happened
    // Audit timestamps to be stored in UTC
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    // Extra readable information about the event
    // Example: "Provider viewed patient profile"
    public string Details { get; set; } = string.Empty;

}