using bagsisbaku.Domain.Promotions;
using FluentValidation;

namespace bagsisbaku.Application.Promotions.Administration;

public sealed class AdminPromoCodeFilterValidator
    : AbstractValidator<AdminPromoCodeFilter>
{
    public AdminPromoCodeFilterValidator()
    {
        RuleFor(filter =>
                filter.Search)
            .MaximumLength(120)
            .When(filter =>
                !string.IsNullOrWhiteSpace(
                    filter.Search));

        RuleFor(filter =>
                filter.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(filter =>
                filter.PageSize)
            .InclusiveBetween(1, 100);
    }
}

public sealed class CreatePromoCodeCommandValidator
    : AbstractValidator<CreatePromoCodeCommand>
{
    public CreatePromoCodeCommandValidator()
    {
        RuleFor(command =>
                command.Name)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(command =>
                command.Code)
            .NotEmpty()
            .MaximumLength(40)
            .Matches(
                "^[A-Za-z0-9_-]+$")
            .WithMessage(
                "Promo kod yalnız A-Z hərfləri, rəqəmlər, tire və alt xətdən ibarət ola bilər.");

        RuleFor(command =>
                command.DiscountType)
            .Must(value =>
                Enum.IsDefined(
                    typeof(PromotionDiscountType),
                    value))
            .WithMessage(
                "Endirim tipi düzgün deyil.");

        RuleFor(command =>
                command.DiscountValue)
            .GreaterThan(0);

        RuleFor(command =>
                command.DiscountValue)
            .LessThanOrEqualTo(100)
            .When(command =>
                command.DiscountType ==
                (int)PromotionDiscountType.Percentage)
            .WithMessage(
                "Faiz endirimi 100-dən böyük ola bilməz.");

        RuleFor(command =>
                command.MinimumOrderAmount)
            .GreaterThanOrEqualTo(0);

        RuleFor(command =>
                command.MaximumDiscountAmount)
            .Must(value =>
                !value.HasValue ||
                value.Value > 0)
            .WithMessage(
                "Maksimum endirim məbləği sıfırdan böyük olmalıdır.");

        RuleFor(command =>
                command.UsageLimit)
            .Must(value =>
                !value.HasValue ||
                value.Value > 0)
            .WithMessage(
                "Ümumi istifadə limiti sıfırdan böyük olmalıdır.");

        RuleFor(command =>
                command.PerCustomerUsageLimit)
            .Must(value =>
                !value.HasValue ||
                value.Value > 0)
            .WithMessage(
                "Müştəri istifadə limiti sıfırdan böyük olmalıdır.");

        RuleFor(command =>
                command.EndsAtUtc)
            .Must(
                (command, endsAtUtc) =>
                    endsAtUtc >
                    command.StartsAtUtc)
            .WithMessage(
                "Bitmə vaxtı başlama vaxtından sonra olmalıdır.");
    }
}

public sealed class UpdatePromoCodeCommandValidator
    : AbstractValidator<UpdatePromoCodeCommand>
{
    public UpdatePromoCodeCommandValidator()
    {
        RuleFor(command =>
                command.Name)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(command =>
                command.Code)
            .NotEmpty()
            .MaximumLength(40)
            .Matches(
                "^[A-Za-z0-9_-]+$")
            .WithMessage(
                "Promo kod yalnız A-Z hərfləri, rəqəmlər, tire və alt xətdən ibarət ola bilər.");

        RuleFor(command =>
                command.DiscountType)
            .Must(value =>
                Enum.IsDefined(
                    typeof(PromotionDiscountType),
                    value))
            .WithMessage(
                "Endirim tipi düzgün deyil.");

        RuleFor(command =>
                command.DiscountValue)
            .GreaterThan(0);

        RuleFor(command =>
                command.DiscountValue)
            .LessThanOrEqualTo(100)
            .When(command =>
                command.DiscountType ==
                (int)PromotionDiscountType.Percentage)
            .WithMessage(
                "Faiz endirimi 100-dən böyük ola bilməz.");

        RuleFor(command =>
                command.MinimumOrderAmount)
            .GreaterThanOrEqualTo(0);

        RuleFor(command =>
                command.MaximumDiscountAmount)
            .Must(value =>
                !value.HasValue ||
                value.Value > 0)
            .WithMessage(
                "Maksimum endirim məbləği sıfırdan böyük olmalıdır.");

        RuleFor(command =>
                command.UsageLimit)
            .Must(value =>
                !value.HasValue ||
                value.Value > 0)
            .WithMessage(
                "Ümumi istifadə limiti sıfırdan böyük olmalıdır.");

        RuleFor(command =>
                command.PerCustomerUsageLimit)
            .Must(value =>
                !value.HasValue ||
                value.Value > 0)
            .WithMessage(
                "Müştəri istifadə limiti sıfırdan böyük olmalıdır.");

        RuleFor(command =>
                command.EndsAtUtc)
            .Must(
                (command, endsAtUtc) =>
                    endsAtUtc >
                    command.StartsAtUtc)
            .WithMessage(
                "Bitmə vaxtı başlama vaxtından sonra olmalıdır.");
    }
}

public sealed class SetPromoCodeStatusCommandValidator
    : AbstractValidator<SetPromoCodeStatusCommand>
{
}