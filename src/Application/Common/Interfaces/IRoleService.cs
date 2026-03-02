using SAPFIAI.Application.Common.Models;

namespace SAPFIAI.Application.Common.Interfaces;

public interface IRoleService
{
    Task<Result<string>> CreateRoleAsync(string roleName);
    Task<Result> UpdateRoleAsync(string roleId, string newName);
    Task<Result> DeleteRoleAsync(string roleId);
    Task<List<RoleDto>> GetAllRolesAsync();
    Task<RoleDto?> GetRoleByIdAsync(string roleId);
    Task<bool> RoleExistsAsync(string roleId);
}
