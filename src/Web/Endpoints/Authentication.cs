using SAPFIAI.Application.Common.Interfaces;
using SAPFIAI.Application.Common.Models;
using SAPFIAI.Application.Users.Commands.EnableTwoFactor;
using SAPFIAI.Application.Users.Commands.ForgotPassword;
using SAPFIAI.Application.Users.Commands.Login;
using SAPFIAI.Application.Users.Commands.Logout;
using SAPFIAI.Application.Users.Commands.RefreshToken;
using SAPFIAI.Application.Users.Commands.Register;
using SAPFIAI.Application.Users.Commands.ResetPassword;
using SAPFIAI.Application.Users.Commands.RevokeToken;
using SAPFIAI.Application.Users.Commands.ValidateTwoFactor;
using SAPFIAI.Application.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace SAPFIAI.Web.Endpoints;

public class Authentication : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this)
            .WithName("Authentication")
            .WithOpenApi();

        // Endpoints públicos (sin autenticación)
        group.MapPost("/register", Register)
            .WithName("Register")
            .WithOpenApi()
            .Produces<RegisterResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .AllowAnonymous();

        group.MapPost("/login", Login)
            .WithName("Login")
            .WithOpenApi()
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .AllowAnonymous();

        group.MapPost("/verify-2fa", VerifyTwoFactor)
            .WithName("Verify2FA")
            .WithOpenApi()
            .Produces<ValidateTwoFactorResponse>(StatusCodes.Status200OK)
            .AllowAnonymous();


        group.MapPost("/forgot-password", ForgotPassword)
            .WithName("ForgotPassword")
            .WithOpenApi()
            .Produces<Result>(StatusCodes.Status200OK)
            .AllowAnonymous();

        group.MapPost("/reset-password", ResetPassword)
            .WithName("ResetPassword")
            .WithOpenApi()
            .Produces<Result>(StatusCodes.Status200OK)
            .AllowAnonymous();

        group.MapPost("/refresh-token", RefreshToken)
            .WithName("RefreshToken")
            .WithOpenApi()
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .AllowAnonymous();

        // Endpoints protegidos (requieren autenticación)
        group.MapPost("/logout", Logout)
            .WithName("Logout")
            .WithOpenApi()
            .Produces<Result>(StatusCodes.Status200OK)
            .RequireAuthorization();

        group.MapPost("/revoke-token", RevokeToken)
            .WithName("RevokeToken")
            .WithOpenApi()
            .Produces<Result>(StatusCodes.Status200OK)
            .RequireAuthorization();

        group.MapPost("/enable-2fa", EnableTwoFactor)
            .WithName("EnableTwoFactor")
            .WithOpenApi()
            .Produces<Result>(StatusCodes.Status200OK)
            .RequireAuthorization();

        group.MapGet("/audit-logs", GetAuditLogs)
            .WithName("GetAuditLogs")
            .Produces<IEnumerable<AuditLogDto>>(StatusCodes.Status200OK)
            .WithOpenApi()
            .RequireAuthorization("CanPurge");

        group.MapGet("/audit-logs/user/{userId}", GetUserAuditLogs)
            .WithName("GetUserAuditLogs")
            .Produces<IEnumerable<AuditLogDto>>(StatusCodes.Status200OK)
            .WithOpenApi()
            .RequireAuthorization();
    }

    private static async Task<RegisterResponse> Register(
        [FromBody] RegisterCommand command,
        IMediator mediator,
        HttpContext httpContext)
    {
        command = command with
        {
            IpAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = httpContext.Request.Headers.UserAgent.ToString()
        };

        return await mediator.Send(command);
    }

    private static async Task<LoginResponse> Login(
        [FromBody] LoginCommand command,
        IMediator mediator,
        HttpContext httpContext)
    {
        command = command with
        {
            IpAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = httpContext.Request.Headers.UserAgent.ToString()
        };

        return await mediator.Send(command);
    }

    private static async Task<ValidateTwoFactorResponse> VerifyTwoFactor(
        [FromBody] ValidateTwoFactorCommand command,
        IMediator mediator,
        HttpContext httpContext)
    {
        command = command with
        {
            IpAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = httpContext.Request.Headers.UserAgent.ToString()
        };

        return await mediator.Send(command);
    }


    private static async Task<Result> ForgotPassword(
        [FromBody] ForgotPasswordCommand command,
        IMediator mediator,
        HttpContext httpContext)
    {
        command = command with
        {
            IpAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = httpContext.Request.Headers.UserAgent.ToString()
        };

        return await mediator.Send(command);
    }

    private static async Task<Result> ResetPassword(
        [FromBody] ResetPasswordCommand command,
        IMediator mediator,
        HttpContext httpContext)
    {
        command = command with
        {
            IpAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = httpContext.Request.Headers.UserAgent.ToString()
        };

        return await mediator.Send(command);
    }

    private static async Task<LoginResponse> RefreshToken(
        [FromBody] RefreshTokenCommand command,
        IMediator mediator,
        HttpContext httpContext)
    {
        command = command with
        {
            IpAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = httpContext.Request.Headers.UserAgent.ToString()
        };

        return await mediator.Send(command);
    }

    private static async Task<Result> Logout(
        IMediator mediator,
        IUser user,
        HttpContext httpContext)
    {
        var command = new LogoutCommand
        {
            UserId = user.Id ?? string.Empty,
            IpAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = httpContext.Request.Headers.UserAgent.ToString()
        };

        return await mediator.Send(command);
    }

    private static async Task<Result> RevokeToken(
        [FromBody] RevokeTokenCommand command,
        IMediator mediator,
        HttpContext httpContext)
    {
        command = command with
        {
            IpAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = httpContext.Request.Headers.UserAgent.ToString()
        };

        return await mediator.Send(command);
    }

    private static async Task<Result> EnableTwoFactor(
        [FromBody] EnableTwoFactorCommand command,
        IMediator mediator,
        IUser user)
    {
        command = command with
        {
            UserId = user.Id ?? string.Empty
        };

        return await mediator.Send(command);
    }

    private static async Task<IEnumerable<AuditLogDto>> GetAuditLogs(
        IMediator mediator,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? action = null)
    {
        var query = new GetAuditLogsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Action = action
        };

        return await mediator.Send(query);
    }

    private static async Task<IEnumerable<AuditLogDto>> GetUserAuditLogs(
        IMediator mediator,
        [FromRoute] string userId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetAuditLogsQuery
        {
            UserId = userId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return await mediator.Send(query);
    }
}
