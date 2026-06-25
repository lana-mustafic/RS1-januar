namespace Market.Application.Modules.Auth.Commands.Login;

/// <summary>
/// FluentValidation validator for <see cref="LoginCommand"/>.
/// </summary>
public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(5).WithMessage("Password must be at least 6 characters long.");

        // Fingerprint is optional, but if provided, you can limit its length
        RuleFor(x => x.Fingerprint)
            .MaximumLength(256).WithMessage("Fingerprint can be up to 256 characters long.")
            .When(x => x.Fingerprint is not null);
    }
}