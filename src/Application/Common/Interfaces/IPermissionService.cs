namespace SAPFIAI.Application.Common.Interfaces;

public interface IPermissionService
{
    Task<List<string>> GetUserPermissionsAsync(string userId);
}
