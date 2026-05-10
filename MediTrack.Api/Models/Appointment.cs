namespace MediTrack.Api.Models;

public class Appointment
{
    // unique id for this appointment
    public Guid Id {get; set;} = Guid.NewGuid();
    
    // this connects the appointment to patient, later EF Core will treat this as a foreign key
    public Guid PatientId {get; set;}

    // this connects the appointment to provider, later EF Core will treat this as a foreign key
    public Guid ProviderId {get; set;}

    // the date and time when the appointment is scheduled, we use UTC to avoid timezone confusion in the database
    public DateTime AppointmentDateUtc {get; set;} 

    // the current state of the appointment
    // examples: "Scheduled", "Completed" , "Cancelled"
    public string Status {get; set;} = "Scheduled";

    // A simple reason for the visit, eg: "Follow-up visit"/ "Annual checkup"
    public string Reason {get; set;} = string.Empty;

    // When this appointment record was created
    public DateTime CreatedAtUtc {get; set;} = DateTime.UtcNow;

    // Navigation property, this lets C# access the related Patient object from an Appointment
    // Example later: appointment.Patient.FullName
    public Patient? Patient {get; set;}

    // Navigation property again
    // Example later: appointment.Provider.FullName
    public Provider? Provider {get; set;}
}