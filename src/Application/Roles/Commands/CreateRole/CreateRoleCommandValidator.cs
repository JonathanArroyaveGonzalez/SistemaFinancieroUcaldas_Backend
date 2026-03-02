namespace SAPFIAI.Application.Roles.Commands.CreateRole;

public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del rol es requerido")
            .MaximumLength(256).WithMessage("El nombre no puede exceder 256 caracteres")
            .Matches("^[a-zA-Z0-9_-]+$").WithMessage("Solo se permiten letras, números, guiones y guiones bajos");
    }
}
