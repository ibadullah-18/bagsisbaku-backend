using bagsisbaku.Domain.Orders;
using FluentValidation;

namespace bagsisbaku.Application.Orders.Administration;

public sealed class AdminOrderFilterValidator
    : AbstractValidator<AdminOrderFilter>
{
    public AdminOrderFilterValidator()
    {
        RuleFor(filter => filter.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage(
                "Səhifə nömrəsi ən azı 1 olmalıdır.");

        RuleFor(filter => filter.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage(
                "Səhifə ölçüsü 1 və 100 arasında olmalıdır.");

        RuleFor(filter => filter.Search)
            .MaximumLength(160)
            .When(
                filter =>
                    !string.IsNullOrWhiteSpace(
                        filter.Search))
            .WithMessage(
                "Axtarış mətni maksimum 160 simvol ola bilər.");

        RuleFor(filter => filter.Status)
            .Must(BeValidOrderStatus)
            .WithMessage(
                "Sifariş statusu düzgün deyil.");

        RuleFor(filter => filter.DeliveryType)
            .Must(BeValidDeliveryType)
            .WithMessage(
                "Çatdırılma növü düzgün deyil.");

        RuleFor(filter => filter)
            .Must(HaveValidDateRange)
            .WithMessage(
                "Başlanğıc tarixi bitmə tarixindən böyük ola bilməz.");
    }

    private static bool BeValidOrderStatus(
        int? status)
    {
        return !status.HasValue ||
               Enum.IsDefined(
                   typeof(OrderStatus),
                   status.Value);
    }

    private static bool BeValidDeliveryType(
        int? deliveryType)
    {
        return !deliveryType.HasValue ||
               Enum.IsDefined(
                   typeof(DeliveryType),
                   deliveryType.Value);
    }

    private static bool HaveValidDateRange(
        AdminOrderFilter filter)
    {
        return !filter.FromUtc.HasValue ||
               !filter.ToUtc.HasValue ||
               filter.FromUtc.Value <=
               filter.ToUtc.Value;
    }
}