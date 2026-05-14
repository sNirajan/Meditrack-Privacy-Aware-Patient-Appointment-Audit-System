using MediTrack.Api.Dtos;
using MediTrack.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediTrack.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")] // means this class is controller
// means this class can handle HTTP API requests
public class PatientsController : ControllerBase
{
    // gives this class access to PatientService
    private readonly PatientService _patientService;

    // ASP.NET Core gives us PatientService through dependency injection.
    // The controller uses the service instead of talking to the database directly.
    public PatientsController(PatientService patientService)
    {
        _patientService = patientService;
    }

    // POST /api/patients
    // Creates a new patient record.
    [HttpPost]
    public async Task<ActionResult<PatientResponse>> CreatePatient(
        [FromBody] CreatePatientRequest request
    )
    {
        var createdPatient = await _patientService.CreatePatientAsync(request);

        // Returns HTTP 201 Created.
        // Also tells the client where the new patient can be found.
        return CreatedAtAction(
            nameof(GetPatientById),
            new { id = createdPatient.Id },
            createdPatient
        );
    }

    // GET /api/patients
    // Returns all patient records.
    [HttpGet]
    public async Task<ActionResult<List<PatientResponse>>> GetAllPatients()
    {
        var patients = await _patientService.GetAllPatientsAsync();

        return Ok(patients);
    }

    // GET /api/patients/{id}
    // Returns one patient by ID.
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PatientResponse>> GetPatientById(Guid id)
    {
        var patient = await _patientService.GetPatientByIdAsync(id);

        if (patient is null)
        {
            return NotFound();
        }

        return Ok(patient);
    }
}
