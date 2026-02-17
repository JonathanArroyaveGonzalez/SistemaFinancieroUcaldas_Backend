using SAPFIAI.Application.Common.Interfaces;

namespace SAPFIAI.Web.Middleware;

public class IpBlockingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<IpBlockingMiddleware> _logger;

    public IpBlockingMiddleware(RequestDelegate next, ILogger<IpBlockingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IIpBlackListService ipBlackListService)
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();

        if (!string.IsNullOrEmpty(ipAddress))
        {
            var isBlocked = await ipBlackListService.IsIpBlockedAsync(ipAddress);

            if (isBlocked)
            {
                var blockInfo = await ipBlackListService.GetBlockInfoAsync(ipAddress);

                _logger.LogWarning("Intento de acceso desde IP bloqueada: {IpAddress}", ipAddress);

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.Headers["X-Block-Reason"] = blockInfo?.Reason ?? "IP bloqueada";

                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Acceso denegado",
                    message = "Tu dirección IP ha sido bloqueada",
                    reason = blockInfo?.Reason,
                    blockedDate = blockInfo?.BlockedDate,
                    expiryDate = blockInfo?.ExpiryDate
                });

                return;
            }
        }

        await _next(context);
    }
}
