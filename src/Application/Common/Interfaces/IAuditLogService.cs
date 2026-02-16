using SAPFIAI.Application.Common.Models;

namespace SAPFIAI.Application.Common.Interfaces;

/// <summary>
/// Interfaz para servicio de auditoría del sistema
/// </summary>
public interface IAuditLogService
{
    /// <summary>
    /// Registra una acción en el audit log
    /// </summary>
    Task LogActionAsync(
        string userId,
        string action,
        string? ipAddress = null,
        string? userAgent = null,
        string? details = null,
        string status = "SUCCESS",
        string? errorMessage = null,
        string? resourceId = null,
        string? resourceType = null);

    /// <summary>
    /// Registra un login exitoso
    /// </summary>
    Task LogLoginAsync(string userId, string ipAddress, string? userAgent = null);

    /// <summary>
    /// Registra un intento fallido de login
    /// </summary>
    Task LogFailedLoginAsync(string userId, string ipAddress, string? userAgent = null, string? error = null);

    /// <summary>
    /// Obtiene los logs de auditoría de un usuario
    /// </summary>
    Task<List<AuditLogDto>> GetUserAuditLogsAsync(string userId, int skip = 0, int take = 50);

    /// <summary>
    /// Obtiene todos los logs de auditoría (admin)
    /// </summary>
    Task<List<AuditLogDto>> GetAllAuditLogsAsync(int skip = 0, int take = 100);
}

