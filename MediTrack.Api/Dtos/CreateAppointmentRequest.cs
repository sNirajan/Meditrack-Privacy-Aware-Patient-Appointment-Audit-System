using System.ComponentModel.DataAnnotations;

namespace MediTrack.Api.Dtos;

public class CreateAppointmentRequest
{
    // THe patient who is booking or receving the appointment
    // We will check in the service that this ID actually exists
    [Required]
    public Guid PatientId { get; set; }

    // The healthcare provider assigned to the appointment
    // We will check in the service that this ID actually exists
    [Required]
    public Guid ProviderId { get; set; }

    // The scheduled appointment date and time
    // We use UTC so the backend stores time consistently
    [Required]
    public DateTime AppointmentDateUtc { get; set; }

    // A short reason for the visit
    // Eg: Annual checkup or follow up visit
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}
