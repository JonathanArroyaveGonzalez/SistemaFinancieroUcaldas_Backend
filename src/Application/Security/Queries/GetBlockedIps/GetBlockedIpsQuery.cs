using SAPFIAI.Domain.Entities;

namespace SAPFIAI.Application.Security.Queries.GetBlockedIps;

public record GetBlockedIpsQuery : IRequest<IEnumerable<IpBlackList>>
{
    public bool ActiveOnly { get; init; } = true;
}
