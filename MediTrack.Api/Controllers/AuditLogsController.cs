using MediTrack.Api.Dtos;
using MediTrack.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediTrack.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AuditLogsController : ControllerBase
{
    private readonly AuditLogService _auditLogService;

    // ASP.NET core gives us AuditLogService through dependency injection
    // This controller exposees audit log records through API endpoints
    public AuditLogsController(AuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    // GET /api/auditlogs
    // Return recent audit log records
    // Only Admin users should be able to view audit logs
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<List<AuditLogResponse>>> GetRecentAuditLogs()
    {
        var auditLogs = await _auditLogService.GetRecentAuditLogsAsync();

        return Ok(auditLogs);
    }
}
