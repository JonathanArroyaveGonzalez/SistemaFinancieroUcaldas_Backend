using SAPFIAI.Application.Common.Models;

namespace SAPFIAI.Application.Security.Commands.UnblockIp;

public record UnblockIpCommand : IRequest<Result>
{
    public required string IpAddress { get; init; }
}
