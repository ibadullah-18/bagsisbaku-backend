using bagsisbaku.Application.Orders;
using bagsisbaku.Domain.Orders;

namespace bagsisbaku.Infrastructure.Orders;

internal sealed class DefaultDeliveryFeeCalculator
    : IDeliveryFeeCalculator
{
    private const decimal AddressDeliveryFee = 5m;

    public decimal Calculate(
        DeliveryType deliveryType,
        decimal? latitude,
        decimal? longitude)
    {
        return deliveryType switch
        {
            DeliveryType.StorePickup =>
                0m,

            DeliveryType.AddressDelivery =>
                AddressDeliveryFee,

            _ =>
                throw new ArgumentOutOfRangeException(
                    nameof(deliveryType),
                    deliveryType,
                    "Çatdırılma növü düzgün deyil.")
        };
    }
}