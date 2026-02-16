namespace SAPFIAI.Application.Common.Interfaces;

/// <summary>
/// Interfaz para servicios de autenticación de dos factores
/// </summary>
public interface ITwoFactorService
{
    /// <summary>
    /// Genera un código de 2FA y lo envía al email del usuario
    /// </summary>
    Task<bool> GenerateAndSendTwoFactorCodeAsync(string userId);

    /// <summary>
    /// Valida el código de 2FA proporcionado por el usuario
    /// </summary>
    Task<bool> ValidateTwoFactorCodeAsync(string userId, string code);

    /// <summary>
    /// Limpia el código de 2FA después de usarlo
    /// </summary>
    Task ClearTwoFactorCodeAsync(string userId);

    /// <summary>
    /// Verifica si el usuario tiene 2FA habilitado
    /// </summary>
    Task<bool> IsTwoFactorEnabledAsync(string userId);
}
