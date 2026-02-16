using SAPFIAI.Application.Common.Interfaces;
using MediatR;

namespace SAPFIAI.Application.Users.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponse>
{
    private readonly IIdentityService _identityService;
    private readonly IAuditLogService _auditLogService;
    private readonly IEmailService _emailService;

    public RegisterCommandHandler(
        IIdentityService identityService,
        IAuditLogService auditLogService,
        IEmailService emailService)
    {
        _identityService = identityService;
        _auditLogService = auditLogService;
        _emailService = emailService;
    }

    public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Crear usuario
        var (result, userId) = await _identityService.CreateUserAsync(request.Email, request.Password);

        if (!result.Succeeded)
        {
            await _auditLogService.LogActionAsync(
                userId: request.Email,
                action: "REGISTER_FAILED",
                ipAddress: request.IpAddress ?? "UNKNOWN",
                userAgent: request.UserAgent,
                details: string.Join(", ", result.Errors),
                status: "FAILED");

            return new RegisterResponse
            {
                Success = false,
                Message = "Error al crear el usuario",
                Errors = result.Errors
            };
        }

        // Registrar auditoría
        await _auditLogService.LogActionAsync(
            userId: userId,
            action: "REGISTER_SUCCESS",
            ipAddress: request.IpAddress ?? "UNKNOWN",
            userAgent: request.UserAgent,
            details: $"Usuario registrado: {request.Email}",
            status: "SUCCESS");

        // Enviar email de confirmación (no bloquea el registro si falla)
        try
        {
            await _emailService.SendRegistrationConfirmationAsync(
                request.Email,
                request.UserName ?? request.Email);
        }
        catch
        {
            // Log pero no falla el registro
        }

        return new RegisterResponse
        {
            Success = true,
            UserId = userId,
            Message = "Usuario registrado exitosamente"
        };
    }
}
