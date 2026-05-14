using MediTrack.Api.Data;
using MediTrack.Api.Dtos;
using MediTrack.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MediTrack.Api.Services;

public class AuditLogService
{
    private readonly ApplicationDbContext _dbContext;

    // ASP.NET Core gives us ApplicationDbContext through dependency injection
    // This service writes and reads audit log records

    public AuditLogService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Records an audit event in the database
    public async Task RecordAsync(
        string userId,
        string userRole,
        string action,
        string entityType,
        Guid? entityId,
        string details
    )
    {
        var auditLog = new AuditLog
        {
            UserId = userId,
            UserRole = userRole,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details,
            TimestampUtc = DateTime.UtcNow,
        };

        _dbContext.AuditLogs.Add(auditLog);

        await _dbContext.SaveChangesAsync();
    }

    // Gets recent audit logs, newest first
    public async Task<List<AuditLogResponse>> GetRecentAuditLogsAsync()
    {
        var auditLogs = await _dbContext
            .AuditLogs.OrderByDescending(auditLog => auditLog.TimestampUtc)
            .Take(100)
            .ToListAsync();

        return auditLogs.Select(MapToResponse).ToList();
    }

    private static AuditLogResponse MapToResponse(AuditLog auditLog)
    {
        return new AuditLogResponse
        {
            Id = auditLog.Id,
            UserId = auditLog.UserId,
            UserRole = auditLog.UserRole,
            Action = auditLog.Action,
            EntityType = auditLog.EntityType,
            EntityId = auditLog.EntityId,
            TimestampUtc = auditLog.TimestampUtc,
            Details = auditLog.Details,
        };
    }
}
