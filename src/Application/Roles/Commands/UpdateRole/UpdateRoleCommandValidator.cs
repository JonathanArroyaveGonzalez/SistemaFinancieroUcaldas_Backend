namespace SAPFIAI.Application.Roles.Commands.UpdateRole;

public class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleCommandValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("El ID del rol es requerido");

        RuleFor(x => x.NewName)
            .NotEmpty().WithMessage("El nuevo nombre es requerido")
            .MaximumLength(256).WithMessage("El nombre no puede exceder 256 caracteres")
            .Matches("^[a-zA-Z0-9_-]+$").WithMessage("Solo se permiten letras, números, guiones y guiones bajos");
    }
}
