using SAPFIAI.Application.Common.Interfaces;
using SAPFIAI.Application.Common.Models;

namespace SAPFIAI.Application.Users.Commands.RevokeToken;

public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, Result>
{
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IAuditLogService _auditLogService;
    private readonly IUser _user;

    public RevokeTokenCommandHandler(
        IRefreshTokenService refreshTokenService,
        IAuditLogService auditLogService,
        IUser user)
    {
        _refreshTokenService = refreshTokenService;
        _auditLogService = auditLogService;
        _user = user;
    }

    public async Task<Result> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        var ipAddress = request.IpAddress ?? "UNKNOWN";
        var userId = _user.Id ?? "UNKNOWN";

        var wasRevoked = await _refreshTokenService.RevokeRefreshTokenAsync(
            request.RefreshToken,
            ipAddress,
            request.Reason);

        if (!wasRevoked)
        {
            await _auditLogService.LogActionAsync(
                userId: userId,
                action: "REVOKE_TOKEN_FAILED",
                ipAddress: ipAddress,
                userAgent: request.UserAgent,
                details: "Intento de revocar token inválido o ya revocado",
                status: "FAILED");

            return Result.Failure(new[] { "Token inválido o ya revocado" });
        }

        await _auditLogService.LogActionAsync(
            userId: userId,
            action: "TOKEN_REVOKED",
            ipAddress: ipAddress,
            userAgent: request.UserAgent,
            details: $"Token revocado manualmente. Razón: {request.Reason}",
            status: "SUCCESS");

        return Result.Success();
    }
}
