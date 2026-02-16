using SAPFIAI.Application.Common.Models;

namespace SAPFIAI.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);

    Task<bool> IsInRoleAsync(string userId, string role);

    Task<bool> AuthorizeAsync(string userId, string policyName);

    Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password);

    Task<Result> DeleteUserAsync(string userId);

    Task<IList<string>> GetUserRolesAsync(string userId);

    Task<(bool Success, string? Token)> GeneratePasswordResetTokenAsync(string email);

    Task<Result> ResetPasswordAsync(string email, string token, string newPassword);

    Task<Result> ChangePasswordAsync(string userId, string currentPassword, string newPassword);

    Task<Result> AssignRoleAsync(string userId, string role);

    Task<Result> RemoveRoleAsync(string userId, string role);
}
