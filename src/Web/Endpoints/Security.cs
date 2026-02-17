using SAPFIAI.Application.Common.Models;
using SAPFIAI.Application.Security.Commands.BlockIp;
using SAPFIAI.Application.Security.Commands.UnblockIp;
using SAPFIAI.Application.Security.Commands.UnlockAccount;
using SAPFIAI.Application.Security.Queries.GetBlockedIps;
using SAPFIAI.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace SAPFIAI.Web.Endpoints;

public class Security : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this)
            .WithName("Security")
            .WithOpenApi()
            .RequireAuthorization("CanPurge"); // Solo administradores

        group.MapGet("/blocked-ips", GetBlockedIps)
            .WithName("GetBlockedIps")
            .Produces<IEnumerable<IpBlackList>>(StatusCodes.Status200OK)
            .WithOpenApi();

        group.MapPost("/block-ip", BlockIp)
            .WithName("BlockIp")
            .Produces<Result>(StatusCodes.Status200OK)
            .WithOpenApi();

        group.MapPost("/unblock-ip", UnblockIp)
            .WithName("UnblockIp")
            .Produces<Result>(StatusCodes.Status200OK)
            .WithOpenApi();

        group.MapPost("/unlock-account", UnlockAccount)
            .WithName("UnlockAccount")
            .Produces<Result>(StatusCodes.Status200OK)
            .WithOpenApi();
    }

    private static async Task<IEnumerable<IpBlackList>> GetBlockedIps(
        IMediator mediator,
        [FromQuery] bool activeOnly = true)
    {
        var query = new GetBlockedIpsQuery { ActiveOnly = activeOnly };
        return await mediator.Send(query);
    }

    private static async Task<Result> BlockIp(
        [FromBody] BlockIpCommand command,
        IMediator mediator)
    {
        return await mediator.Send(command);
    }

    private static async Task<Result> UnblockIp(
        [FromBody] UnblockIpCommand command,
        IMediator mediator)
    {
        return await mediator.Send(command);
    }

    private static async Task<Result> UnlockAccount(
        [FromBody] UnlockAccountCommand command,
        IMediator mediator)
    {
        return await mediator.Send(command);
    }
}
