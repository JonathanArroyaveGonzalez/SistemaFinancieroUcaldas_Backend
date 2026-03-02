namespace SAPFIAI.Application.Permissions.Commands.CreatePermission;

public class CreatePermissionCommandValidator : AbstractValidator<CreatePermissionCommand>
{
    public CreatePermissionCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del permiso es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres")
            .Matches("^[a-z0-9._-]+$").WithMessage("Use formato: modulo.accion (ej: users.create)");

        RuleFor(x => x.Module)
            .NotEmpty().WithMessage("El módulo es requerido")
            .MaximumLength(50).WithMessage("El módulo no puede exceder 50 caracteres");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
