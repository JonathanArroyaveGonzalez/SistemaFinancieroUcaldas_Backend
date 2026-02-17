using SAPFIAI.Application.Common.Interfaces;
using SAPFIAI.Domain.Entities;

namespace SAPFIAI.Application.Security.Queries.GetBlockedIps;

public class GetBlockedIpsQueryHandler : IRequestHandler<GetBlockedIpsQuery, IEnumerable<IpBlackList>>
{
    private readonly IIpBlackListService _ipBlackListService;

    public GetBlockedIpsQueryHandler(IIpBlackListService ipBlackListService)
    {
        _ipBlackListService = ipBlackListService;
    }

    public async Task<IEnumerable<IpBlackList>> Handle(GetBlockedIpsQuery request, CancellationToken cancellationToken)
    {
        return await _ipBlackListService.GetBlockedIpsAsync(request.ActiveOnly);
    }
}
