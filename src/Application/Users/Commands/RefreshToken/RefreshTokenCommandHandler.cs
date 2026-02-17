using SAPFIAI.Application.Common.Interfaces;
using SAPFIAI.Application.Users.Commands.Login;

namespace SAPFIAI.Application.Users.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, LoginResponse>
{
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IIpBlackListService _ipBlackListService;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IIdentityService _identityService;
    private readonly IAuthenticationOperations _authOperations;
    private readonly IAuditLogService _auditLogService;

    public RefreshTokenCommandHandler(
        IRefreshTokenService refreshTokenService,
        IIpBlackListService ipBlackListService,
        IJwtTokenGenerator jwtTokenGenerator,
        IIdentityService identityService,
        IAuthenticationOperations authOperations,
        IAuditLogService auditLogService)
    {
        _refreshTokenService = refreshTokenService;
        _ipBlackListService = ipBlackListService;
        _jwtTokenGenerator = jwtTokenGenerator;
        _identityService = identityService;
        _authOperations = authOperations;
        _auditLogService = auditLogService;
    }

    public async Task<LoginResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var ipAddress = request.IpAddress ?? "UNKNOWN";

        // 1. Verificar que la IP no esté bloqueada
        var isIpBlocked = await _ipBlackListService.IsIpBlockedAsync(ipAddress);
        if (isIpBlocked)
        {
            await _auditLogService.LogActionAsync(
                userId: "UNKNOWN",
                action: "REFRESH_TOKEN_BLOCKED_IP",
                ipAddress: ipAddress,
                userAgent: request.UserAgent,
                details: "Intento de refresh token desde IP bloqueada",
                status: "BLOCKED");

            return new LoginResponse
            {
                Success = false,
                Message = "Acceso denegado. Tu IP ha sido bloqueada.",
                Errors = new[] { "IP bloqueada por razones de seguridad" }
            };
        }

        // 2. Validar el refresh token
        var (isValid, userId, email) = await _refreshTokenService.ValidateRefreshTokenAsync(
            request.RefreshToken, 
            ipAddress);

        if (!isValid || userId == null || email == null)
        {
            await _auditLogService.LogActionAsync(
                userId: "UNKNOWN",
                action: "REFRESH_TOKEN_INVALID",
                ipAddress: ipAddress,
                userAgent: request.UserAgent,
                details: "Refresh token inválido o expirado",
                status: "FAILED");

            return new LoginResponse
            {
                Success = false,
                Message = "Token de actualización inválido o expirado",
                Errors = new[] { "Por favor, inicia sesión nuevamente" }
            };
        }

        // 3. Obtener roles del usuario
        var userRoles = await _identityService.GetUserRolesAsync(userId);

        // 4. Generar nuevo access token
        var newAccessToken = _jwtTokenGenerator.GenerateToken(userId, email, userRoles);

        // 5. Generar nuevo refresh token y revocar el anterior
        var newRefreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(userId, ipAddress);
        await _refreshTokenService.RevokeRefreshTokenAsync(
            request.RefreshToken, 
            ipAddress, 
            "Replaced by new token");

        // 6. Obtener información del usuario
        var user = await _authOperations.GetUserByIdAsync(userId);

        // 7. Auditar la acción
        await _auditLogService.LogActionAsync(
            userId: userId,
            action: "REFRESH_TOKEN_SUCCESS",
            ipAddress: ipAddress,
            userAgent: request.UserAgent,
            details: "Token actualizado exitosamente",
            status: "SUCCESS");

        return new LoginResponse
        {
            Success = true,
            Token = newAccessToken,
            RefreshToken = newRefreshToken.Token,
            RefreshTokenExpiry = newRefreshToken.ExpiryDate,
            User = user,
            Message = "Token actualizado exitosamente"
        };
    }
}
