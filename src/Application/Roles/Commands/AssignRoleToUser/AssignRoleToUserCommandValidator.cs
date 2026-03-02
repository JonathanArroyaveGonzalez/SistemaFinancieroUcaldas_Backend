namespace SAPFIAI.Application.Roles.Commands.AssignRoleToUser;

public class AssignRoleToUserCommandValidator : AbstractValidator<AssignRoleToUserCommand>
{
    public AssignRoleToUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("El ID del usuario es requerido");

        RuleFor(x => x.RoleName)
            .NotEmpty().WithMessage("El nombre del rol es requerido")
            .MaximumLength(256).WithMessage("El nombre no puede exceder 256 caracteres");
    }
}
