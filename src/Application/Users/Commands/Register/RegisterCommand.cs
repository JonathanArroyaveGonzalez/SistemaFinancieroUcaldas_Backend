using SAPFIAI.Application.Common.Models;
using FluentValidation;
using System.Text.Json.Serialization;

namespace SAPFIAI.Application.Users.Commands.Register;

public record RegisterCommand : IRequest<RegisterResponse>
{
    public required string Email { get; init; }

    public required string Password { get; init; }

    public required string ConfirmPassword { get; init; }

    public string? UserName { get; init; }

    public string? PhoneNumber { get; init; }

    [JsonIgnore]
    public string? IpAddress { get; init; }

    [JsonIgnore]
    public string? UserAgent { get; init; }
}

public class RegisterResponse
{
    public bool Success { get; set; }

    public string? UserId { get; set; }

    public string? Message { get; set; }

    public IEnumerable<string>? Errors { get; set; }
}

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es requerido")
            .EmailAddress().WithMessage("El email no es válido");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es requerida")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres")
            .Matches("[A-Z]").WithMessage("La contraseña debe contener al menos una letra mayúscula")
            .Matches("[a-z]").WithMessage("La contraseña debe contener al menos una letra minúscula")
            .Matches("[0-9]").WithMessage("La contraseña debe contener al menos un número")
            .Matches("[^a-zA-Z0-9]").WithMessage("La contraseña debe contener al menos un carácter especial");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Debe confirmar la contraseña")
            .Equal(x => x.Password).WithMessage("Las contraseñas no coinciden");
    }
}
