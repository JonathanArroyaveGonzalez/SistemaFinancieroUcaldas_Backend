using SAPFIAI.Application.Common.Interfaces;
using SAPFIAI.Application.Common.Models;
using SAPFIAI.Domain.Entities;
using SAPFIAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SAPFIAI.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de auditoría del sistema
/// </summary>
public class AuditLogService : IAuditLogService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(ApplicationDbContext dbContext, ILogger<AuditLogService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task LogActionAsync(
        string userId,
        string action,
        string? ipAddress = null,
        string? userAgent = null,
        string? details = null,
        string status = "SUCCESS",
        string? errorMessage = null,
        string? resourceId = null,
        string? resourceType = null)
    {
        try
        {
            var auditLog = new AuditLog
            {
                UserId = userId,
                Action = action,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Details = details,
                Timestamp = DateTime.UtcNow,
                Status = status,
                ErrorMessage = errorMessage,
                ResourceId = resourceId,
                ResourceType = resourceType
            };

            _dbContext.Set<AuditLog>().Add(auditLog);
            var saved = await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation("📝 Audit Log guardado: {Action} - Usuario: {UserId} - Status: {Status} (Registros: {Count})", 
                action, userId, status, saved);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error guardando audit log: {Action} - {UserId}", action, userId);
        }
    }

    public async Task LogLoginAsync(string userId, string ipAddress, string? userAgent = null)
    {
        await LogActionAsync(
            userId,
            "LOGIN",
            ipAddress,
            userAgent,
            status: "SUCCESS"
        );
    }

    public async Task LogFailedLoginAsync(string userId, string ipAddress, string? userAgent = null, string? error = null)
    {
        await LogActionAsync(
            userId,
            "FAILED_LOGIN",
            ipAddress,
            userAgent,
            status: "FAILED",
            errorMessage: error
        );
    }

    public async Task<List<AuditLogDto>> GetUserAuditLogsAsync(string userId, int skip = 0, int take = 50)
    {
        try
        {
            var logs = await _dbContext.Set<AuditLog>()
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.Timestamp)
                .Skip(skip)
                .Take(take)
                .AsNoTracking()
                .Select(l => new AuditLogDto
                {
                    Id = l.Id,
                    UserId = l.UserId,
                    Action = l.Action,
                    IpAddress = l.IpAddress,
                    UserAgent = l.UserAgent,
                    Timestamp = l.Timestamp,
                    Details = l.Details,
                    Status = l.Status,
                    ErrorMessage = l.ErrorMessage,
                    ResourceId = l.ResourceId,
                    ResourceType = l.ResourceType
                })
                .ToListAsync();

            return logs;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error retrieving user audit logs: {ex.Message}");
            return new List<AuditLogDto>();
        }
    }

    public async Task<List<AuditLogDto>> GetAllAuditLogsAsync(int skip = 0, int take = 100)
    {
        try
        {
            var logs = await _dbContext.Set<AuditLog>()
                .OrderByDescending(l => l.Timestamp)
                .Skip(skip)
                .Take(take)
                .AsNoTracking()
                .Select(l => new AuditLogDto
                {
                    Id = l.Id,
                    UserId = l.UserId,
                    Action = l.Action,
                    IpAddress = l.IpAddress,
                    UserAgent = l.UserAgent,
                    Timestamp = l.Timestamp,
                    Details = l.Details,
                    Status = l.Status,
                    ErrorMessage = l.ErrorMessage,
                    ResourceId = l.ResourceId,
                    ResourceType = l.ResourceType
                })
                .ToListAsync();

            return logs;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error retrieving all audit logs: {ex.Message}");
            return new List<AuditLogDto>();
        }
    }
}
