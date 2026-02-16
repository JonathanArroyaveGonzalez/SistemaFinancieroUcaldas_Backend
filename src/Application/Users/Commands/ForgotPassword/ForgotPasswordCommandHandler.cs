using SAPFIAI.Application.Common.Interfaces;
using SAPFIAI.Application.Common.Models;
using MediatR;

namespace SAPFIAI.Application.Users.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly IEmailService _emailService;
    private readonly IAuditLogService _auditLogService;

    public ForgotPasswordCommandHandler(
        IIdentityService identityService,
        IEmailService emailService,
        IAuditLogService auditLogService)
    {
        _identityService = identityService;
        _emailService = emailService;
        _auditLogService = auditLogService;
    }

    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        // Generar token de reset
        var (success, resetToken) = await _identityService.GeneratePasswordResetTokenAsync(request.Email);

        if (!success || string.IsNullOrEmpty(resetToken))
        {
            // Por seguridad, no revelamos si el email existe o no
            return Result.Success();
        }

        // Enviar email con el token
        var emailSent = await _emailService.SendPasswordResetAsync(
            request.Email,
            request.Email,
            resetToken);

        await _auditLogService.LogActionAsync(
            userId: request.Email,
            action: "PASSWORD_RESET_REQUESTED",
            ipAddress: request.IpAddress ?? "UNKNOWN",
            userAgent: request.UserAgent,
            details: emailSent ? "Email de reset enviado" : "Error al enviar email",
            status: emailSent ? "SUCCESS" : "WARNING");

        return Result.Success();
    }
}
