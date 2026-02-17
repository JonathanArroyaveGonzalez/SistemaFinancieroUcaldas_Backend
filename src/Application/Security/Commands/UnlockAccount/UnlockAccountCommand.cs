using SAPFIAI.Application.Common.Models;

namespace SAPFIAI.Application.Security.Commands.UnlockAccount;

public record UnlockAccountCommand : IRequest<Result>
{
    public required string UserId { get; init; }
}
