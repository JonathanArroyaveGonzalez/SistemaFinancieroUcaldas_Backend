namespace SAPFIAI.Domain.Entities;

/// <summary>
/// Entidad para definir permisos del sistema
/// </summary>
public class Permission : BaseEntity
{
    /// <summary>
    /// Nombre único del permiso
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Descripción del permiso
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Módulo al que pertenece el permiso
    /// </summary>
    public string Module { get; set; } = string.Empty;

    /// <summary>
    /// Indica si el permiso está activo
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Fecha de creación
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Relación con roles
    /// </summary>
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
