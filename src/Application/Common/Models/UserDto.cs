namespace SAPFIAI.Application.Common.Models;

public class UserDto
{
    public required string Id { get; set; }

    public required string Email { get; set; }

    public string? UserName { get; set; }

    public string? PhoneNumber { get; set; }

    public bool TwoFactorEnabled { get; set; }

    public DateTime? LastLoginDate { get; set; }

    public string? LastLoginIp { get; set; }
}
