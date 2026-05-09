namespace MediTrack.Api.Models;

public class Provider
{
   // unique ID for this provider
   // We use Guid instead of int because it is harder to guess from the outside 
   public Guid Id { get; set; }

    // provider's fullname like "Dr. Tim Horton"
    public string FullName { get; set; } = string.Empty;

    // email address, later this could connect to login/account information
    public string Email { get; set; } = string.Empty;

    // provider's area od work, eg: "Family Medicine"/ "Cardiology"
    public string Speciality { get; set; } = string.Empty;

    // we store timestamps in UTC so the system is consistent accorss time zones
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
