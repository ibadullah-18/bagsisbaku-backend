using System.ComponentModel.DataAnnotations;

namespace bagsisbaku.Contracts.Orders;

public sealed record PlaceOrderRequest
{
    [Range(1, 2)]
    public int DeliveryType { get; init; }

    public Guid? CustomerAddressId { get; init; }

    [MaxLength(160)]
    public string? PickupRecipientFullName
    {
        get;
        init;
    }

    [MaxLength(32)]
    public string? PickupPhoneNumber
    {
        get;
        init;
    }

    [MaxLength(1000)]
    public string? CustomerNote { get; init; }

    public string? PromoCode { get; init; }
}