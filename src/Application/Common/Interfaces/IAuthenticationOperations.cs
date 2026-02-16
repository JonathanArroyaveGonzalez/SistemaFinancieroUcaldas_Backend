using SAPFIAI.Application.Common.Models;

namespace SAPFIAI.Application.Common.Interfaces;

/// <summary>
/// Interfaz para operaciones de autenticación de usuarios
/// </summary>
public interface IAuthenticationOperations
{
    /// <summary>
    /// Verifica las credenciales de un usuario
    /// </summary>
    Task<(bool IsValid, string? UserId, string? Email)> VerifyCredentialsAsync(string email, string password);

    /// <summary>
    /// Obtiene un usuario por ID
    /// </summary>
    Task<UserDto?> GetUserByIdAsync(string userId);

    /// <summary>
    /// Obtiene un usuario por email
    /// </summary>
    Task<UserDto?> GetUserByEmailAsync(string email);

    /// <summary>
    /// Actualiza los datos de última conexión del usuario
    /// </summary>
    Task UpdateLastLoginAsync(string userId, string? ipAddress);

    /// <summary>
    /// Verifica si el usuario tiene 2FA habilitado
    /// </summary>
    Task<bool> Has2FAEnabledAsync(string userId);

    /// <summary>
    /// Habilita 2FA para un usuario
    /// </summary>
    Task<bool> EnableTwoFactorAsync(string userId);

    /// <summary>
    /// Deshabilita 2FA para un usuario
    /// </summary>
    Task<bool> DisableTwoFactorAsync(string userId);
}
