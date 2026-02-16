using System.Security.Cryptography;
using System.Text;
using SAPFIAI.Application.Common.Interfaces;
using SAPFIAI.Infrastructure.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SAPFIAI.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de autenticación de dos factores
/// </summary>
public class TwoFactorService : ITwoFactorService
{
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<TwoFactorService> _logger;
    private readonly IMemoryCache _cache;
    private const string CacheKeyPrefix = "TwoFactorCode";

    public TwoFactorService(
        IEmailService emailService,
        IConfiguration configuration,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment environment,
        ILogger<TwoFactorService> logger,
        IMemoryCache cache)
    {
        _emailService = emailService;
        _configuration = configuration;
        _userManager = userManager;
        _environment = environment;
        _logger = logger;
        _cache = cache;
    }

    /// <summary>
    /// Obtiene una variable de configuración desde IConfiguration o Environment
    /// </summary>
    private string? GetConfigValue(string key)
    {
        return _configuration[key] ?? Environment.GetEnvironmentVariable(key);
    }

    public async Task<bool> GenerateAndSendTwoFactorCodeAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            // Generar código de 6 dígitos
            var code = GenerateRandomCode(6);

            // Configurar expiración (por defecto 10 minutos)
            var expirationConfig = GetConfigValue("TWO_FACTOR_EXPIRATION_MINUTES");
            var expirationMinutes = int.Parse(expirationConfig ?? "10");

            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expirationMinutes)
            };

            _cache.Set(GetCacheKey(userId), code, cacheOptions);

            // En desarrollo, mostrar el código en la consola
            if (_environment.IsDevelopment())
            {
                _logger.LogWarning("═══════════════════════════════════════════════════════════");
                _logger.LogWarning("  🔐 CÓDIGO 2FA (SOLO DESARROLLO): {Code}", code);
                _logger.LogWarning("  📧 Usuario: {Email}", user.Email);
                _logger.LogWarning("  ⏰ Expira en: {Minutes} minutos", expirationMinutes);
                _logger.LogWarning("═══════════════════════════════════════════════════════════");
            }

            // Intentar enviar email
            var emailSent = await _emailService.SendTwoFactorCodeAsync(user.Email!, code, user.UserName ?? user.Email!);

            // En desarrollo, retornar true incluso si el email falla
            if (_environment.IsDevelopment() && !emailSent)
            {
                _logger.LogWarning("⚠️ Email no enviado, pero en desarrollo se permite continuar con el código mostrado arriba.");
                return true;
            }

            return emailSent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating 2FA code");
            return false;
        }
    }

    public async Task<bool> ValidateTwoFactorCodeAsync(string userId, string code)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            if (!_cache.TryGetValue(GetCacheKey(userId), out string? cachedCode) || string.IsNullOrEmpty(cachedCode))
            {
                return false;
            }

            // Comparación timing-safe para evitar timing attacks
            var expectedBytes = Encoding.UTF8.GetBytes(cachedCode);
            var providedBytes = Encoding.UTF8.GetBytes(code.Trim());

            if (expectedBytes.Length != providedBytes.Length ||
                !CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes))
            {
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating 2FA code");
            return false;
        }
    }

    public Task ClearTwoFactorCodeAsync(string userId)
    {
        try
        {
            _cache.Remove(GetCacheKey(userId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing 2FA code");
        }

        return Task.CompletedTask;
    }

    public async Task<bool> IsTwoFactorEnabledAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user?.IsTwoFactorEnabled ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking 2FA status");
            return false;
        }
    }

    /// <summary>
    /// Genera un código numérico aleatorio usando un generador criptográficamente seguro
    /// </summary>
    private static string GenerateRandomCode(int length)
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[length];
        rng.GetBytes(bytes);
        return string.Concat(bytes.Select(b => (b % 10).ToString()));
    }

    private static string GetCacheKey(string userId) => $"{CacheKeyPrefix}:{userId}";
}
