using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SAPFIAI.Application.Common.Interfaces;
using SAPFIAI.Application.Common.Models;

namespace SAPFIAI.Infrastructure.Services;

public class RoleService : IRoleService
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IApplicationDbContext _context;

    public RoleService(RoleManager<IdentityRole> roleManager, IApplicationDbContext context)
    {
        _roleManager = roleManager;
        _context = context;
    }

    public async Task<Result<string>> CreateRoleAsync(string roleName)
    {
        var existingRole = await _roleManager.FindByNameAsync(roleName);
        if (existingRole != null)
        {
            return Result.Failure<string>(new Error("RoleExists", "El rol ya existe"));
        }

        var role = new IdentityRole(roleName);
        var result = await _roleManager.CreateAsync(role);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Failure<string>(new Error("CreateRoleFailed", errors));
        }

        return Result.Success(role.Id);
    }

    public async Task<Result> UpdateRoleAsync(string roleId, string newName)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        if (role == null)
        {
            return Result.Failure(new Error("RoleNotFound", "Rol no encontrado"));
        }

        var existingRole = await _roleManager.FindByNameAsync(newName);
        if (existingRole != null && existingRole.Id != roleId)
        {
            return Result.Failure(new Error("RoleExists", "Ya existe un rol con ese nombre"));
        }

        role.Name = newName;
        var result = await _roleManager.UpdateAsync(role);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Failure(new Error("UpdateRoleFailed", errors));
        }

        return Result.Success();
    }

    public async Task<Result> DeleteRoleAsync(string roleId)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        if (role == null)
        {
            return Result.Failure(new Error("RoleNotFound", "Rol no encontrado"));
        }

        var rolePermissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync();

        _context.RolePermissions.RemoveRange(rolePermissions);
        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _roleManager.DeleteAsync(role);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Failure(new Error("DeleteRoleFailed", errors));
        }

        return Result.Success();
    }

    public async Task<List<RoleDto>> GetAllRolesAsync()
    {
        var roles = await _roleManager.Roles.ToListAsync();

        var roleDtos = new List<RoleDto>();
        foreach (var role in roles)
        {
            var permissionCount = await _context.RolePermissions
                .Where(rp => rp.RoleId == role.Id)
                .CountAsync();

            roleDtos.Add(new RoleDto
            {
                Id = role.Id,
                Name = role.Name!,
                NormalizedName = role.NormalizedName,
                PermissionCount = permissionCount
            });
        }

        return roleDtos;
    }

    public async Task<RoleDto?> GetRoleByIdAsync(string roleId)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        if (role == null) return null;

        var permissionCount = await _context.RolePermissions
            .Where(rp => rp.RoleId == role.Id)
            .CountAsync();

        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name!,
            NormalizedName = role.NormalizedName,
            PermissionCount = permissionCount
        };
    }

    public async Task<bool> RoleExistsAsync(string roleId)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        return role != null;
    }
}
