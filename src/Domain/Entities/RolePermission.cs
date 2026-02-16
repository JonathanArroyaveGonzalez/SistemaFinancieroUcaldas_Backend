namespace SAPFIAI.Domain.Entities;

/// <summary>
/// Entidad de relación entre roles y permisos
/// </summary>
public class RolePermission : BaseEntity
{
    /// <summary>
    /// Identificador del rol
    /// </summary>
    public string RoleId { get; set; } = string.Empty;

    /// <summary>
    /// Identificador del permiso
    /// </summary>
    public int PermissionId { get; set; }

    /// <summary>
    /// Fecha de asignación
    /// </summary>
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Usuario que asignó el permiso
    /// </summary>
    public string? AssignedBy { get; set; }

    /// <summary>
    /// Navegación al permiso
    /// </summary>
    public Permission Permission { get; set; } = null!;
}
