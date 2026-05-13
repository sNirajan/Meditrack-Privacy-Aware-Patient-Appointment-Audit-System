using MediTrack.Api.Models; // model classes
using Microsoft.EntityFrameworkCore;

namespace MediTrack.Api.Data; // data classes

public class ApplicationDbContext : DbContext
{
    // standard EF core setup, base(options) means pass those options to the parent DbContext class.

    // This constructor lets ASP.NET Core inject the database settings into this class.
    // We will register those settings in Program.cs.
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    // Model is singular, table like Patients is plural

    // Creates a database table for Patient records
    // Patient.cs becomes the Patients table.
    public DbSet<Patient> Patients => Set<Patient>();

    // Creates a database table for Providers records
    // Provider.cs becomes the Providers table.
    public DbSet<Provider> Providers => Set<Provider>();

    // Creates a db table for Appointment records
    // Appointment.cs becomes the Appointments table.
    public DbSet<Appointment> Appointments => Set<Appointment>();

    // Creates a db table for AuditLogd records
    // AuditLog.cs becomes the AuditLogs table.
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // configuring the Patients table
        modelBuilder.Entity<Patient>(entity =>
        {
            // FullName is required because a patient record should not exist without a name
            entity.Property(patient => patient.FullName).IsRequired().HasMaxLength(150);

            // Email is required and capped to a normal safe database length
            entity.Property(patient => patient.Email).IsRequired().HasMaxLength(255);

            // Phone number is optional in real systems, but our model currently uses an empty string
            // We cap the length because phone numbers shouldn't be unlimited text
            entity.Property(patient => patient.PhoneNumber).HasMaxLength(30);
        });

        // configuring the Providers table
        modelBuilder.Entity<Provider>(entity =>
        {
            // provider should always have a name
            entity.Property(provider => provider.FullName).IsRequired().HasMaxLength(150);

            // Email is required and capped to a normal safe database length
            entity.Property(provider => provider.Email).IsRequired().HasMaxLength(255);

            // Specialty describes the provider's area of work, like Family Medicine or Cardiology
            entity.Property(provider => provider.Specialty).IsRequired().HasMaxLength(100);
        });

        // configuring the Appointments table
        modelBuilder.Entity<Appointment>(entity =>
        {
            // Status tells us where the appointment is in its lifecycle
            // Examples: Scheduled, Completed, Cancelled
            entity.Property(appointment => appointment.Status).IsRequired().HasMaxLength(50);

            // Reason is short note about why appt was booked, we cap it
            entity.Property(appointment => appointment.Reason).HasMaxLength(500);

            // One appointment belongs to one patient, PatientId is the foreign key that points to the Patients table
            entity
                .HasOne(appointment => appointment.Patient)
                .WithMany()
                .HasForeignKey(appointment => appointment.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // One appointment belongs to one provider, ProviderId is the foreign key that points to the Providers table
            entity
                .HasOne(appointment => appointment.Provider)
                .WithMany()
                .HasForeignKey(appointment => appointment.ProviderId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            // UserId identifies who performed the action
            // For now, this may come from request headers. Later, it can come from real authentication
            entity.Property(auditLog => auditLog.UserId).IsRequired().HasMaxLength(100);

            // UserRole tells us what type of user performed the action
            // Examples: Patient, Provider, Admin
            entity.Property(auditLog => auditLog.UserRole).IsRequired().HasMaxLength(50);

            // Action describes what happened, eg: CreatedPatient, ViewedPatient, CreatedAppointment
            entity.Property(auditLog => auditLog.Action).IsRequired().HasMaxLength(100);

            // EntityType tells us what kind of record was affected, eg: Patient, Appointment, Provider
            entity.Property(auditLog => auditLog.EntityType).IsRequired().HasMaxLength(100);

            // Details gives us a readable explanation of the event, we cap it so audit logs stay controlled and searchable
            entity.Property(auditLog => auditLog.Details).HasMaxLength(1000);
        });

        // one appointment belongs to one patient
        // PatientId is the foreign key that points to the Patients table
        modelBuilder
            .Entity<Appointment>()
            .HasOne(appointment => appointment.Patient)
            .WithMany()
            .HasForeignKey(appointment => appointment.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        // one appointment belongs to one provider
        // ProviderId is the foreign key that points to the Providers table
        modelBuilder
            .Entity<Appointment>()
            .HasOne(appointment => appointment.Provider)
            .WithMany()
            .HasForeignKey(appointment => appointment.ProviderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
