using System.ComponentModel.DataAnnotations;

namespace MediTrack.Api.Dtos;

public class CreatePatientRequest
{
    // patient name is required because a patient record shouldn't exist without a name
    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    // Email is required and must follow a valid email like format
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    // DOB is required for patient's identity/context

    public DateOnly DateOfBirth { get; set; }

    // phone number stays as string because phone numbers can include +, spaces,a adn dashes, stays optional for now, shouldn't be unlimited text
    [MaxLength(30)]
    public string PhoneNumber { get; set; } = string.Empty;
}
