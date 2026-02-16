using System.Runtime.InteropServices;
using SAPFIAI.Domain.Constants;
using SAPFIAI.Domain.Entities;
using SAPFIAI.Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SAPFIAI.Infrastructure.Data;

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.InitialiseAsync();

        await initialiser.SeedAsync();
    }
}

public class ApplicationDbContextInitialiser
{
    private readonly ILogger<ApplicationDbContextInitialiser> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            // Check if we can connect and if there are pending migrations
            var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
            
            if (pendingMigrations.Any())
            {
                _logger.LogInformation("Applying {Count} pending migrations...", pendingMigrations.Count());
                await _context.Database.MigrateAsync();
            }
            else
            {
                _logger.LogInformation("Database is up to date. No pending migrations.");
            }
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 2714)
        {
            // Error 2714: Object already exists in database
            _logger.LogWarning("Database tables already exist. Skipping migration.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        // Default roles
        var administratorRole = new IdentityRole(Roles.Administrator);
        var userRole = new IdentityRole("User");
        var managerRole = new IdentityRole("Manager");

        if (_roleManager.Roles.All(r => r.Name != administratorRole.Name))
        {
            await _roleManager.CreateAsync(administratorRole);
        }

        if (_roleManager.Roles.All(r => r.Name != userRole.Name))
        {
            await _roleManager.CreateAsync(userRole);
        }

        if (_roleManager.Roles.All(r => r.Name != managerRole.Name))
        {
            await _roleManager.CreateAsync(managerRole);
        }

        // Default permissions
        if (!_context.Permissions.Any())
        {
            var permissions = new List<Permission>
            {
                new() { Name = "users.read", Description = "Ver usuarios", Module = "Users" },
                new() { Name = "users.create", Description = "Crear usuarios", Module = "Users" },
                new() { Name = "users.update", Description = "Actualizar usuarios", Module = "Users" },
                new() { Name = "users.delete", Description = "Eliminar usuarios", Module = "Users" },
                new() { Name = "roles.read", Description = "Ver roles", Module = "Roles" },
                new() { Name = "roles.manage", Description = "Gestionar roles", Module = "Roles" },
                new() { Name = "audit.read", Description = "Ver auditoría", Module = "Audit" },
                new() { Name = "system.admin", Description = "Administración del sistema", Module = "System" }
            };

            _context.Permissions.AddRange(permissions);
            await _context.SaveChangesAsync();
        }

        // Default users
        var administrator = new ApplicationUser { UserName = "administrator@localhost", Email = "administrator@localhost" };

        if (_userManager.Users.All(u => u.UserName != administrator.UserName))
        {
            await _userManager.CreateAsync(administrator, "Administrator1!");
            if (!string.IsNullOrWhiteSpace(administratorRole.Name))
            {
                await _userManager.AddToRolesAsync(administrator, new [] { administratorRole.Name });
            }
        }
    }
}
