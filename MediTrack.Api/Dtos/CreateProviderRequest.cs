using System.ComponentModel.DataAnnotations;

namespace MediTrack.Api.Dtos;

public class CreateProviderRequest
{
    // The provider's fullname
    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    // provider's email address
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    // provider's area of work. eg: "family medicine"
    [Required]
    [MaxLength(100)]
    public string Specialty { get; set; } = string.Empty;
}
