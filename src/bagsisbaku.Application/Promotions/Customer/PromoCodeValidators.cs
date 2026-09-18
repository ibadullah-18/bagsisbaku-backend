using FluentValidation;

namespace bagsisbaku.Application.Promotions.Customer;

public sealed class ValidatePromoCodeCommandValidator
    : AbstractValidator<ValidatePromoCodeCommand>
{
    public ValidatePromoCodeCommandValidator()
    {
        RuleFor(command =>
                command.Code)
            .NotEmpty()
            .MaximumLength(40)
            .Matches(
                "^[A-Za-z0-9_-]+$")
            .WithMessage(
                "Promo kod yalnız A-Z hərfləri, rəqəmlər, tire və alt xətdən ibarət ola bilər.");
    }
}