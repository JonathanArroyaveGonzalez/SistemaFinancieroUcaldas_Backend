using SAPFIAI.Application.Common.Models;
using FluentValidation;
using System.Text.Json.Serialization;

namespace SAPFIAI.Application.Users.Commands.ForgotPassword;

public record ForgotPasswordCommand : IRequest<Result>
{
    public required string Email { get; init; }

    [JsonIgnore]
    public string? IpAddress { get; init; }

    [JsonIgnore]
    public string? UserAgent { get; init; }
}

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es requerido")
            .EmailAddress().WithMessage("El email no es válido");
    }
}
