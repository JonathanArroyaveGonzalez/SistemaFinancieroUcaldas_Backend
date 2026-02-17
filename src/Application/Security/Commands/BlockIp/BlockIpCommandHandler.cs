using SAPFIAI.Application.Common.Interfaces;
using SAPFIAI.Application.Common.Models;

namespace SAPFIAI.Application.Security.Commands.BlockIp;

public class BlockIpCommandHandler : IRequestHandler<BlockIpCommand, Result>
{
    private readonly IIpBlackListService _ipBlackListService;
    private readonly IAuditLogService _auditLogService;
    private readonly IUser _user;

    public BlockIpCommandHandler(
        IIpBlackListService ipBlackListService,
        IAuditLogService auditLogService,
        IUser user)
    {
        _ipBlackListService = ipBlackListService;
        _auditLogService = auditLogService;
        _user = user;
    }

    public async Task<Result> Handle(BlockIpCommand request, CancellationToken cancellationToken)
    {
        var blockedBy = _user.Id ?? "SYSTEM";

        var ipBlock = await _ipBlackListService.BlockIpAsync(
            request.IpAddress,
            request.Reason,
            request.BlackListReason,
            blockedBy,
            request.ExpiryDate,
            request.Notes);

        await _auditLogService.LogActionAsync(
            userId: blockedBy,
            action: "IP_BLOCKED",
            ipAddress: "SYSTEM",
            userAgent: null,
            details: $"IP {request.IpAddress} bloqueada. Razón: {request.Reason}",
            status: "SUCCESS");

        return Result.Success();
    }
}
