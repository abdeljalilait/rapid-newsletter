using FluentValidation;
using NewsletterPlatform.Application.Common;

namespace NewsletterPlatform.Application.Features.Authentication.Commands;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator(AuthOptions auth)
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email is required.")
            .MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(auth.MinPasswordLength).WithMessage($"Password must be at least {auth.MinPasswordLength} characters.")
            .MaximumLength(auth.MaxPasswordLength);

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100);

        RuleFor(x => x.LastName).MaximumLength(100);
    }
}