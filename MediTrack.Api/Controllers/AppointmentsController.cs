using MediTrack.Api.Dtos;
using MediTrack.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace MediTrack.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly AppointmentService _appointmentService;

    // ASP.NET Core gives us AppointmentService through dependency injection.
    // The controller handles HTTP. the service handles the appointment logic

    public AppointmentsController(AppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    // POST /api/appointments
    // Creates a new appointment between an existing patiend and provider
    [HttpPost]
    public async Task<ActionResult<AppointmentResponse>> CreateAppointment(
        [FromBody] CreateAppointmentRequest request
    )
    {
        try
        {
            var createdAppointment = await _appointmentService.CreateAppointmentAsync(request);

            // Returns HTTP 201 Created.
            // Also gives the client a route to fetch the created appointment later.
            return CreatedAtAction(
                nameof(GetAppointmentById),
                new { id = createdAppointment.Id },
                createdAppointment
            );
        }
        catch (ArgumentException ex)
        {
            // If the service rejects the request becuase of invalid business rules,
            // return 400 Bad Request with a readable message
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET /api/appointments
    // Returns all appointments with patient and provider names
    [HttpGet]
    public async Task<ActionResult<List<AppointmentResponse>>> GetAllAppointments()
    {
        var appointments = await _appointmentService.GetAllAppointmentsAsync();
        return Ok(appointments);
    }

    // GET /api/appointments/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<AppointmentResponse>> GetAppointmentById(Guid id)
    {
        var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
        if (appointment is null)
        {
            return NotFound();
        }

        return Ok(appointment);
    }
}
