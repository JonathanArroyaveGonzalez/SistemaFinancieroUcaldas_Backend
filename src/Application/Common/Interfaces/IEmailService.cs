namespace SAPFIAI.Application.Common.Interfaces;

/// <summary>
/// Interfaz para servicio de envío de emails
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Envía código de 2FA al email del usuario
    /// </summary>
    Task<bool> SendTwoFactorCodeAsync(string email, string code, string userName);

    /// <summary>
    /// Envía confirmación de login
    /// </summary>
    Task<bool> SendLoginConfirmationAsync(string email, string userName, string ipAddress, DateTime loginDate);

    /// <summary>
    /// Envía alerta de acceso inusual
    /// </summary>
    Task<bool> SendSecurityAlertAsync(string email, string userName, string action, string ipAddress);

    /// <summary>
    /// Envía confirmación de registro
    /// </summary>
    Task<bool> SendRegistrationConfirmationAsync(string email, string userName);

    /// <summary>
    /// Envía email de restablecimiento de contraseña
    /// </summary>
    Task<bool> SendPasswordResetAsync(string email, string userName, string resetToken);
}
