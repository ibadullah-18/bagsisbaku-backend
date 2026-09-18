using bagsisbaku.Domain.Orders;
using FluentValidation;

namespace bagsisbaku.Application.Orders;

public sealed class PlaceOrderCommandValidator
    : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderCommandValidator()
    {
        RuleFor(command => command.DeliveryType)
            .IsInEnum()
            .WithMessage(
                "Çatdırılma növü düzgün deyil.");

        RuleFor(command => command.CustomerNote)
            .MaximumLength(1000)
            .WithMessage(
                "Müştəri qeydi 1000 simvoldan çox ola bilməz.");

        When(
            command =>
                command.DeliveryType ==
                DeliveryType.AddressDelivery,
            () =>
            {
                RuleFor(
                        command =>
                            command.CustomerAddressId)
                    .Must(
                        addressId =>
                            addressId.HasValue &&
                            addressId.Value != Guid.Empty)
                    .WithMessage(
                        "Ünvana çatdırılma üçün ünvan seçilməlidir.");

                RuleFor(
                        command =>
                            command.PickupRecipientFullName)
                    .Empty()
                    .WithMessage(
                        "Ünvana çatdırılmada ayrıca pickup adı göndərilməməlidir.");

                RuleFor(
                        command =>
                            command.PickupPhoneNumber)
                    .Empty()
                    .WithMessage(
                        "Ünvana çatdırılmada ayrıca pickup telefonu göndərilməməlidir.");

        ConfigurePromoCodeValidation();
            });

        When(
            command =>
                command.DeliveryType ==
                DeliveryType.StorePickup,
            () =>
            {
                RuleFor(
                        command =>
                            command.CustomerAddressId)
                    .Null()
                    .WithMessage(
                        "Mağazadan götürmə zamanı ünvan seçilməməlidir.");

                RuleFor(
                        command =>
                            command.PickupRecipientFullName)
                    .NotEmpty()
                    .WithMessage(
                        "Alıcının adı yazılmalıdır.")
                    .MaximumLength(160)
                    .WithMessage(
                        "Alıcının adı 160 simvoldan çox ola bilməz.");

                RuleFor(
                        command =>
                            command.PickupPhoneNumber)
                    .NotEmpty()
                    .WithMessage(
                        "Telefon nömrəsi yazılmalıdır.")
                    .MaximumLength(32)
                    .WithMessage(
                        "Telefon nömrəsi 32 simvoldan çox ola bilməz.");
            });
    }

    private void ConfigurePromoCodeValidation()
    {
        RuleFor(command => command.PromoCode)
            .MaximumLength(
                Promotions.PromoCodeNormalizer
                    .MaximumCodeLength)
            .When(
                command =>
                    !string.IsNullOrWhiteSpace(
                        command.PromoCode))
            .WithMessage(
                "Promo kod maksimum 50 simvol ola bilər.");
    }}