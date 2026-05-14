namespace MediTrack.Api.Models;

public class Patient
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public DateOnly DateOfBirth { get; set; }

    public string PhoneNumber { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
