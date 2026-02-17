using SAPFIAI.Application.Common.Models;
using System.Text.Json.Serialization;

namespace SAPFIAI.Application.Users.Commands.Logout;

public record LogoutCommand : IRequest<Result>
{
    [JsonIgnore]
    public required string UserId { get; init; }

    [JsonIgnore]
    public string? IpAddress { get; init; }

    [JsonIgnore]
    public string? UserAgent { get; init; }
}
