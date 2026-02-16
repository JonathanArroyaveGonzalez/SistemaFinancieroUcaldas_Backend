using SAPFIAI.Application.Common.Interfaces;
using SAPFIAI.Application.Common.Models;
using MediatR;

namespace SAPFIAI.Application.Users.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IAuthenticationOperations _authOperations;
    private readonly ITwoFactorService _twoFactorService;
    private readonly IAuditLogService _auditLogService;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IIdentityService _identityService;

    public LoginCommandHandler(
        IAuthenticationOperations authOperations,
        ITwoFactorService twoFactorService,
        IAuditLogService auditLogService,
        IJwtTokenGenerator jwtTokenGenerator,
        IIdentityService identityService)
    {
        _authOperations = authOperations;
        _twoFactorService = twoFactorService;
        _auditLogService = auditLogService;
        _jwtTokenGenerator = jwtTokenGenerator;
        _identityService = identityService;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Verificar credenciales
        var (isValid, userId, email) = await _authOperations.VerifyCredentialsAsync(request.Email, request.Password);

        if (!isValid || userId == null || email == null)
        {
            await _auditLogService.LogActionAsync(
                userId: request.Email,
                action: "LOGIN_FAILED",
                ipAddress: request.IpAddress ?? "UNKNOWN",
                userAgent: request.UserAgent,
                details: "Credenciales inválidas",
                status: "FAILED");

            return new LoginResponse
            {
                Success = false,
                Message = "Email o contraseña inválidos",
                Errors = new[] { "Credenciales inválidas" }
            };
        }

        // Obtener usuario y roles
        var user = await _authOperations.GetUserByIdAsync(userId);
        if (user == null)
        {
            return new LoginResponse
            {
                Success = false,
                Message = "Usuario no encontrado",
                Errors = new[] { "Usuario no encontrado" }
            };
        }

        var userRoles = await _identityService.GetUserRolesAsync(userId);

        // Verificar si el usuario tiene 2FA habilitado
        var has2FA = await _twoFactorService.IsTwoFactorEnabledAsync(userId);

        if (has2FA)
        {
            // Flujo con 2FA: generar token temporal y enviar código
            var tempToken = _jwtTokenGenerator.GenerateToken(userId, email, userRoles, requiresTwoFactorVerification: true);

            var twoFactorSent = await _twoFactorService.GenerateAndSendTwoFactorCodeAsync(userId);

            if (!twoFactorSent)
            {
                await _auditLogService.LogActionAsync(
                    userId: userId,
                    action: "LOGIN_2FA_SEND_FAILED",
                    ipAddress: request.IpAddress ?? "UNKNOWN",
                    userAgent: request.UserAgent,
                    details: "Error al enviar código 2FA",
                    status: "FAILED");

                return new LoginResponse
                {
                    Success = false,
                    Message = "Error al enviar código de verificación",
                    Errors = new[] { "No se pudo enviar el código 2FA al correo" }
                };
            }

            await _auditLogService.LogActionAsync(
                userId: userId,
                action: "LOGIN_PENDING_2FA",
                ipAddress: request.IpAddress ?? "UNKNOWN",
                userAgent: request.UserAgent,
                details: "Código 2FA enviado, pendiente verificación",
                status: "PENDING");

            return new LoginResponse
            {
                Success = true,
                Token = tempToken,
                User = user,
                Requires2FA = true,
                Message = "Código de verificación enviado a tu correo electrónico"
            };
        }

        // Flujo sin 2FA: login directo con token final
        var token = _jwtTokenGenerator.GenerateToken(userId, email, userRoles, requiresTwoFactorVerification: false);

        await _authOperations.UpdateLastLoginAsync(userId, request.IpAddress);

        await _auditLogService.LogActionAsync(
            userId: userId,
            action: "LOGIN_SUCCESS",
            ipAddress: request.IpAddress ?? "UNKNOWN",
            userAgent: request.UserAgent,
            details: "Login completado sin 2FA",
            status: "SUCCESS");

        return new LoginResponse
        {
            Success = true,
            Token = token,
            User = user,
            Requires2FA = false,
            Message = "Login exitoso"
        };
    }
}
