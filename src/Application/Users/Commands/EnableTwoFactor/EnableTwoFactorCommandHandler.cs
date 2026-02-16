using SAPFIAI.Application.Common.Interfaces;
using SAPFIAI.Application.Common.Models;
using MediatR;
using System;

namespace SAPFIAI.Application.Users.Commands.EnableTwoFactor;

public class EnableTwoFactorCommandHandler : IRequestHandler<EnableTwoFactorCommand, Result>
{
    private readonly IAuthenticationOperations _authOperations;
    private readonly IAuditLogService _auditLogService;
    private readonly ITwoFactorService _twoFactorService;

    public EnableTwoFactorCommandHandler(
        IAuthenticationOperations authOperations,
        IAuditLogService auditLogService,
        ITwoFactorService twoFactorService)
    {
        _authOperations = authOperations;
        _auditLogService = auditLogService;
        _twoFactorService = twoFactorService;
    }

    public async Task<Result> Handle(EnableTwoFactorCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var (isValid, userId, _) = await _authOperations.VerifyCredentialsAsync(request.Email, request.Password);

            if (!isValid || userId == null || !string.Equals(userId, request.UserId, StringComparison.Ordinal))
            {
                return Result.Failure(new[] { "Credenciales inválidas para el usuario autenticado" });
            }

            var result = request.Enable
                ? await _authOperations.EnableTwoFactorAsync(request.UserId)
                : await _authOperations.DisableTwoFactorAsync(request.UserId);

            if (result)
            {
                if (!request.Enable)
                {
                    await _twoFactorService.ClearTwoFactorCodeAsync(request.UserId);
                }

                await _auditLogService.LogActionAsync(
                    userId: request.UserId,
                    action: request.Enable ? "ENABLE_2FA" : "DISABLE_2FA",
                    details: request.Enable ? "2FA habilitado exitosamente" : "2FA deshabilitado exitosamente",
                    status: "SUCCESS");

                return Result.Success();
            }

            return Result.Failure(new[]
            {
                request.Enable ? "No se pudo habilitar 2FA" : "No se pudo deshabilitar 2FA"
            });
        }
        catch (Exception ex)
        {
            await _auditLogService.LogActionAsync(
                userId: request.UserId,
                action: request.Enable ? "ENABLE_2FA_ERROR" : "DISABLE_2FA_ERROR",
                details: ex.Message,
                status: "ERROR");

            return Result.Failure(new[]
            {
                request.Enable ? "Error al habilitar 2FA" : "Error al deshabilitar 2FA"
            });
        }
    }
}
