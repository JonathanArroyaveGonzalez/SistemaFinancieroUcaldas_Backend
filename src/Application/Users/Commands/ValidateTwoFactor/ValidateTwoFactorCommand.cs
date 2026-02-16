using SAPFIAI.Application.Common.Models;
using FluentValidation;
using System.Text.Json.Serialization;

namespace SAPFIAI.Application.Users.Commands.ValidateTwoFactor;

public record ValidateTwoFactorCommand : IRequest<ValidateTwoFactorResponse>
{
    public required string Code { get; init; }

    public string? Token { get; init; }

    [JsonIgnore]
    public string? IpAddress { get; init; }

    [JsonIgnore]
    public string? UserAgent { get; init; }
}

public class ValidateTwoFactorResponse
{
    public bool Success { get; set; }

    public string? Token { get; set; }

    public UserDto? User { get; set; }

    public string? Message { get; set; }

    public IEnumerable<string>? Errors { get; set; }
}

public class ValidateTwoFactorCommandValidator : AbstractValidator<ValidateTwoFactorCommand>
{
    public ValidateTwoFactorCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("El código es requerido")
            .Length(6).WithMessage("El código debe tener 6 dígitos")
            .Matches("^[0-9]+$").WithMessage("El código debe contener solo números");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("El token es requerido");
    }
}

