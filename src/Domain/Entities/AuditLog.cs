namespace SAPFIAI.Domain.Entities;

/// <summary>
/// Entidad para registrar auditoría de acciones y accesos del sistema
/// </summary>
public class AuditLog : BaseEntity
{
    /// <summary>
    /// Identificador del usuario que realizó la acción
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de acción realizada (LOGIN, CREATE_TODO, ENABLE_2FA, etc.)
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Dirección IP de origen
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User Agent del cliente
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Marca de tiempo de la acción
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Detalles adicionales en formato JSON
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Estado de la acción (SUCCESS, FAILED, PENDING)
    /// </summary>
    public string Status { get; set; } = "SUCCESS";

    /// <summary>
    /// Mensaje de error (si aplica)
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Recurso afectado (opcional: ID de entidad modificada)
    /// </summary>
    public string? ResourceId { get; set; }

    /// <summary>
    /// Tipo de recurso afectado (TodoList, TodoItem, etc.)
    /// </summary>
    public string? ResourceType { get; set; }
}
