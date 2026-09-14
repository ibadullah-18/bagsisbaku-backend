using FluentValidation;

namespace bagsisbaku.Application.Customers.Administration;

public sealed class AdminCustomerFilterValidator
    : AbstractValidator<AdminCustomerFilter>
{
    public AdminCustomerFilterValidator()
    {
        RuleFor(filter => filter.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage(
                "Səhifə nömrəsi minimum 1 olmalıdır.");

        RuleFor(filter => filter.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage(
                "Səhifə ölçüsü 1 və 100 arasında olmalıdır.");

        RuleFor(filter => filter.Search)
            .MaximumLength(200)
            .WithMessage(
                "Axtarış mətni 200 simvoldan çox ola bilməz.")
            .When(filter =>
                !string.IsNullOrWhiteSpace(
                    filter.Search));

        RuleFor(filter => filter)
            .Must(filter =>
                !filter.CreatedFromUtc.HasValue ||
                !filter.CreatedToUtc.HasValue ||
                filter.CreatedFromUtc.Value <=
                filter.CreatedToUtc.Value)
            .WithName(nameof(
                AdminCustomerFilter.CreatedToUtc))
            .WithMessage(
                "Başlanğıc tarixi bitmə tarixindən sonra ola bilməz.");
    }
}

public sealed class SetCustomerStatusCommandValidator
    : AbstractValidator<SetCustomerStatusCommand>
{
    public SetCustomerStatusCommandValidator()
    {
        RuleFor(command => command.CustomerId)
            .NotEmpty()
            .WithMessage(
                "CustomerId boş ola bilməz.");
    }
}