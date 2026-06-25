namespace Market.Application.Modules.Auth.Commands.Logout;

/// <summary>
/// FluentValidation validator for <see cref="LogoutCommand"/>.
/// </summary>
public sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}