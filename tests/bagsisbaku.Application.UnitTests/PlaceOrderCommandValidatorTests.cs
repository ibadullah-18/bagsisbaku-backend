using bagsisbaku.Application.Orders;
using bagsisbaku.Domain.Orders;
using Xunit;

namespace bagsisbaku.Application.UnitTests;

public sealed class PlaceOrderCommandValidatorTests
{
    private readonly PlaceOrderCommandValidator
        _validator = new();

    [Fact]
    public void AddressDeliveryWithAddressIsValid()
    {
        var command =
            new PlaceOrderCommand(
                DeliveryType.AddressDelivery,
                Guid.NewGuid(),
                null,
                null,
                "Qapıya çatdırın.");

        var result =
            _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void AddressDeliveryWithoutAddressIsInvalid()
    {
        var command =
            new PlaceOrderCommand(
                DeliveryType.AddressDelivery,
                null,
                null,
                null,
                null);

        var result =
            _validator.Validate(command);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                nameof(
                    PlaceOrderCommand
                        .CustomerAddressId));
    }

    [Fact]
    public void StorePickupWithContactIsValid()
    {
        var command =
            new PlaceOrderCommand(
                DeliveryType.StorePickup,
                null,
                "Ibadulla Huseynzade",
                "+994501234567",
                null);

        var result =
            _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void StorePickupWithoutContactIsInvalid()
    {
        var command =
            new PlaceOrderCommand(
                DeliveryType.StorePickup,
                null,
                null,
                null,
                null);

        var result =
            _validator.Validate(command);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                nameof(
                    PlaceOrderCommand
                        .PickupRecipientFullName));

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                nameof(
                    PlaceOrderCommand
                        .PickupPhoneNumber));
    }

    [Fact]
    public void UndefinedDeliveryTypeIsInvalid()
    {
        var command =
            new PlaceOrderCommand(
                (DeliveryType)99,
                null,
                null,
                null,
                null);

        var result =
            _validator.Validate(command);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                nameof(
                    PlaceOrderCommand
                        .DeliveryType));
    }
}