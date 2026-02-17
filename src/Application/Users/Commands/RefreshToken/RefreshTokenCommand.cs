using SAPFIAI.Application.Users.Commands.Login;
using System.Text.Json.Serialization;

namespace SAPFIAI.Application.Users.Commands.RefreshToken;

public record RefreshTokenCommand : IRequest<LoginResponse>
{
    public required string RefreshToken { get; init; }

    [JsonIgnore]
    public string? IpAddress { get; init; }

    [JsonIgnore]
    public string? UserAgent { get; init; }
}
