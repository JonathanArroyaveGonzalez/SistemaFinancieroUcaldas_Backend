using Microsoft.EntityFrameworkCore;
using SAPFIAI.Application.Common.Interfaces;
using SAPFIAI.Domain.Entities;
using SAPFIAI.Domain.Enums;

namespace SAPFIAI.Infrastructure.Services;

public class IpBlackListService : IIpBlackListService
{
    private readonly IApplicationDbContext _context;

    public IpBlackListService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsIpBlockedAsync(string ipAddress)
    {
        return await _context.IpBlackLists
            .AnyAsync(ib => ib.IpAddress == ipAddress && 
                           (ib.ExpiryDate == null || ib.ExpiryDate > DateTime.UtcNow));
    }

    public async Task<IpBlackList> BlockIpAsync(
        string ipAddress, 
        string reason, 
        BlackListReason blackListReason, 
        string? blockedBy, 
        DateTime? expiryDate = null, 
        string? notes = null)
    {
        // Verificar si ya existe un bloqueo activo
        var existingBlock = await _context.IpBlackLists
            .FirstOrDefaultAsync(ib => ib.IpAddress == ipAddress && 
                                      (ib.ExpiryDate == null || ib.ExpiryDate > DateTime.UtcNow));

        if (existingBlock != null)
        {
            // No se puede modificar propiedades de solo lectura directamente.
            // Alternativa: eliminar el bloqueo anterior y crear uno nuevo actualizado.
            _context.IpBlackLists.Remove(existingBlock);
            var updatedBlock = IpBlackList.Block(
                ipAddress,
                reason,
                expiryDate,
                blockedBy,
                notes,
                blackListReason
            );
            _context.IpBlackLists.Add(updatedBlock);
            await _context.SaveChangesAsync(default);
            return updatedBlock;
        }

        var ipBlackList = IpBlackList.Block(
            ipAddress,
            reason,
            expiryDate,
            blockedBy,
            notes,
            blackListReason
        );

        _context.IpBlackLists.Add(ipBlackList);
        await _context.SaveChangesAsync(default);

        return ipBlackList;
    }

    public async Task<bool> UnblockIpAsync(string ipAddress, string unblockedBy)
    {
        var blockedIps = await _context.IpBlackLists
            .Where(ib => ib.IpAddress == ipAddress)
            .ToListAsync();

        if (!blockedIps.Any())
            return false;

        // Establecer fecha de expiraci�n en el pasado para desactivar el bloqueo
        foreach (var blockedIp in blockedIps)
        {
            blockedIp.Unblock();
            // No se puede modificar Notes porque es de solo lectura
        }

        await _context.SaveChangesAsync(default);

        return true;
    }

    public async Task<IEnumerable<IpBlackList>> GetBlockedIpsAsync(bool activeOnly = true)
    {
        var query = _context.IpBlackLists.AsQueryable();

        if (activeOnly)
        {
            query = query.Where(ib => ib.ExpiryDate == null || ib.ExpiryDate > DateTime.UtcNow);
        }

        return await query
            .OrderByDescending(ib => ib.BlockedDate)
            .ToListAsync();
    }

    public async Task<int> CleanupExpiredBlocksAsync()
    {
        var expiredBlocks = await _context.IpBlackLists
            .Where(ib => ib.ExpiryDate != null && ib.ExpiryDate < DateTime.UtcNow.AddDays(-30))
            .ToListAsync();

        _context.IpBlackLists.RemoveRange(expiredBlocks);

        return await _context.SaveChangesAsync(default);
    }

    public async Task<IpBlackList?> GetBlockInfoAsync(string ipAddress)
    {
        return await _context.IpBlackLists
            .Where(ib => ib.IpAddress == ipAddress && 
                        (ib.ExpiryDate == null || ib.ExpiryDate > DateTime.UtcNow))
            .OrderByDescending(ib => ib.BlockedDate)
            .FirstOrDefaultAsync();
    }
}
