using SAPFIAI.Application.Common.Models;
using SAPFIAI.Domain.Enums;

namespace SAPFIAI.Application.Security.Commands.BlockIp;

public record BlockIpCommand : IRequest<Result>
{
    public required string IpAddress { get; init; }
    
    public required string Reason { get; init; }
    
    public BlackListReason BlackListReason { get; init; } = BlackListReason.ManualBlock;
    
    public DateTime? ExpiryDate { get; init; }
    
    public string? Notes { get; init; }
}
