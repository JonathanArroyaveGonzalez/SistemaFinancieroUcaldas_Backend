using SAPFIAI.Application.Common.Interfaces;
using SAPFIAI.Application.Common.Models;

namespace SAPFIAI.Application.Users.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IAuditLogService _auditLogService;

    public LogoutCommandHandler(
        IRefreshTokenService refreshTokenService,
        IAuditLogService auditLogService)
    {
        _refreshTokenService = refreshTokenService;
        _auditLogService = auditLogService;
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var ipAddress = request.IpAddress ?? "UNKNOWN";

        // Revocar todos los refresh tokens del usuario
        var revokedCount = await _refreshTokenService.RevokeAllUserTokensAsync(
            request.UserId,
            ipAddress,
            "User logout");

        // Auditar logout
        await _auditLogService.LogActionAsync(
            userId: request.UserId,
            action: "LOGOUT",
            ipAddress: ipAddress,
            userAgent: request.UserAgent,
            details: $"Logout exitoso. {revokedCount} tokens revocados",
            status: "SUCCESS");

        return Result.Success();
    }
}
