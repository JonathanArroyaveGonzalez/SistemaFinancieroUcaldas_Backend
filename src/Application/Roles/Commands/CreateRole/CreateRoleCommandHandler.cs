using SAPFIAI.Application.Common.Interfaces;
using SAPFIAI.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace SAPFIAI.Application.Roles.Commands.CreateRole;

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, Result<string>>
{
    private readonly IRoleService _roleService;
    private readonly ILogger<CreateRoleCommandHandler> _logger;

    public CreateRoleCommandHandler(IRoleService roleService, ILogger<CreateRoleCommandHandler> logger)
    {
        _roleService = roleService;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating role: {RoleName}", request.Name);
        
        var result = await _roleService.CreateRoleAsync(request.Name);
        
        if (result.Succeeded)
            _logger.LogInformation("Role created successfully: {RoleId}", result.Data);
        else
            _logger.LogWarning("Failed to create role {RoleName}: {Error}", request.Name, result.Error.Description);
        
        return result;
    }
}
