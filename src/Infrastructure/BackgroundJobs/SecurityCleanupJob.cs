using SAPFIAI.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SAPFIAI.Infrastructure.BackgroundJobs;

public class SecurityCleanupJob : BackgroundService
{
    private readonly ILogger<SecurityCleanupJob> _logger;
    private readonly IServiceProvider _serviceProvider;

    public SecurityCleanupJob(
        ILogger<SecurityCleanupJob> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Security Cleanup Job iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken); // Ejecutar cada 1 hora

                using var scope = _serviceProvider.CreateScope();

                var refreshTokenService = scope.ServiceProvider.GetRequiredService<IRefreshTokenService>();
                var ipBlackListService = scope.ServiceProvider.GetRequiredService<IIpBlackListService>();
                var loginAttemptService = scope.ServiceProvider.GetRequiredService<ILoginAttemptService>();

                _logger.LogInformation("Ejecutando limpieza de seguridad...");

                // Limpiar refresh tokens expirados
                var tokensDeleted = await refreshTokenService.CleanupExpiredTokensAsync();
                _logger.LogInformation("Refresh tokens expirados eliminados: {Count}", tokensDeleted);

                // Limpiar bloqueos de IP expirados
                var blocksDeleted = await ipBlackListService.CleanupExpiredBlocksAsync();
                _logger.LogInformation("Bloqueos de IP expirados eliminados: {Count}", blocksDeleted);

                // Limpiar intentos de login antiguos (más de 30 días)
                var attemptsDeleted = await loginAttemptService.CleanupOldAttemptsAsync(30);
                _logger.LogInformation("Intentos de login antiguos eliminados: {Count}", attemptsDeleted);

                _logger.LogInformation("Limpieza de seguridad completada");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la limpieza de seguridad");
            }
        }
    }
}
