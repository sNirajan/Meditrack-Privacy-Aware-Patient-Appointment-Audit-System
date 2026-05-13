using MediTrack.Api.Data;
using MediTrack.Api.Dtos;
using MediTrack.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MediTrack.Api.Services;

public class PatientService
{
    private readonly ApplicationDbContext _dbContext;

    // ASP.NET Core will give ApplicationDbContext through dependency injection
    // This lets the service read and write patient records from the database
    public PatientService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Creates a new patient record from the request data
    public async Task<PatientResponse> CreatePatientAsync(CreatePatientRequest request)
    {
        var patient = new Patient
        {
            FullName = request.FullName,
            Email = request.Email,
            DateOfBirth = request.DateOfBirth,
            PhoneNumber = request.PhoneNumber,
        };

        _dbContext.Patients.Add(patient);

        // SaveChangesAsync sends the insert operation to the database
        await _dbContext.SaveChangesAsync();

        return MapToResponse(patient);
    }

    // Gets all patients from the database
    public async Task<List<PatientResponse>> GetAllPatientsAsync()
    {
        var patients = await _dbContext.Patients.OrderBy(patient => patient.FullName).ToListAsync();

        return patients.Select(MapToResponse).ToList();
    }

    // Gets one patient by ID, if no patient exists with that ID, return null
    public async Task<PatientResponse?> GetPatientByIdAsync(Guid id)
    {
        var patient = await _dbContext.Patients.FirstOrDefaultAsync(patient => patient.Id == id);

        if (patient is null)
        {
            return null;
        }

        return MapToResponse(patient);
    }

    // Converts the database model into the response DTO, this keeps us from exposing the database model directly from the API
    private static PatientResponse MapToResponse(Patient patient)
    {
        return new PatientResponse
        {
            Id = patient.Id,
            FullName = patient.FullName,
            Email = patient.Email,
            DateOfBirth = patient.DateOfBirth,
            PhoneNumber = patient.PhoneNumber,
            CreatedAtUtc = patient.CreatedAtUtc,
        };
    }
}
