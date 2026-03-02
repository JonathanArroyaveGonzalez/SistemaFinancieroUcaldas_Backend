namespace SAPFIAI.Application.Common.Models;

public class RoleDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NormalizedName { get; set; }
    public int PermissionCount { get; set; }
}
