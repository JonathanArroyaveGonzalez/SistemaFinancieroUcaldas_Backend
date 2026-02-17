using SAPFIAI.Domain.Entities;
using SAPFIAI.Domain.Enums;

namespace SAPFIAI.Application.Common.Interfaces;

public interface IIpBlackListService
{
    Task<bool> IsIpBlockedAsync(string ipAddress);
    
    Task<IpBlackList> BlockIpAsync(string ipAddress, string reason, BlackListReason blackListReason, string? blockedBy, DateTime? expiryDate = null, string? notes = null);
    
    Task<bool> UnblockIpAsync(string ipAddress, string unblockedBy);
    
    Task<IEnumerable<IpBlackList>> GetBlockedIpsAsync(bool activeOnly = true);
    
    Task<int> CleanupExpiredBlocksAsync();
    
    Task<IpBlackList?> GetBlockInfoAsync(string ipAddress);
}
