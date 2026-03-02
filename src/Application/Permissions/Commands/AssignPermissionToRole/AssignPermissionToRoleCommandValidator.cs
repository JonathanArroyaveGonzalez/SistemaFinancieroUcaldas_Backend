namespace SAPFIAI.Application.Permissions.Commands.AssignPermissionToRole;

public class AssignPermissionToRoleCommandValidator : AbstractValidator<AssignPermissionToRoleCommand>
{
    public AssignPermissionToRoleCommandValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("El ID del rol es requerido");

        RuleFor(x => x.PermissionId)
            .GreaterThan(0).WithMessage("El ID del permiso debe ser mayor a 0");
    }
}
