using Microsoft.AspNetCore.Identity;
using SAPFIAI.Domain.Entities;

namespace SAPFIAI.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// Indica si el usuario tiene 2FA habilitado
    /// </summary>
    public bool IsTwoFactorEnabled { get; set; } = false;

    /// <summary>
    /// Último acceso del usuario
    /// </summary>
    public DateTime? LastLoginDate { get; set; }

    /// <summary>
    /// Última dirección IP registrada
    /// </summary>
    public string? LastLoginIp { get; set; }

    /// <summary>
    /// Relación con logs de auditoría
    /// </summary>
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    /// <summary>
    /// Número de intentos fallidos de login
    /// </summary>
    public int FailedLoginAttempts { get; set; } = 0;

    /// <summary>
    /// Fecha del último intento fallido
    /// </summary>
    public DateTime? LastFailedLoginAttempt { get; set; }
}
