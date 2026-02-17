using SAPFIAI.Application.Common.Models;
using System.Text.Json.Serialization;

namespace SAPFIAI.Application.Users.Commands.RevokeToken;

public record RevokeTokenCommand : IRequest<Result>
{
    public required string RefreshToken { get; init; }

    public string Reason { get; init; } = "Revoked by user";

    [JsonIgnore]
    public string? IpAddress { get; init; }

    [JsonIgnore]
    public string? UserAgent { get; init; }
}
