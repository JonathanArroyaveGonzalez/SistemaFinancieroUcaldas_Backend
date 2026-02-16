using SAPFIAI.Application.Common.Models;
using FluentValidation;
using MediatR;
using System.Text.Json.Serialization;

namespace SAPFIAI.Application.Users.Commands.EnableTwoFactor;

public record EnableTwoFactorCommand : IRequest<Result>
{
    [JsonIgnore]
    public string UserId { get; init; } = string.Empty;

    public required string Email { get; init; }

    public required string Password { get; init; }

    public bool Enable { get; init; } = true;
}

public class EnableTwoFactorCommandValidator : AbstractValidator<EnableTwoFactorCommand>
{
    public EnableTwoFactorCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es requerido")
            .EmailAddress().WithMessage("El email no es válido");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es requerida")
            .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres");
    }
}
