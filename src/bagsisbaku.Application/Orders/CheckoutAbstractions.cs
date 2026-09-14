using bagsisbaku.Domain.Orders;

namespace bagsisbaku.Application.Orders;

public interface IOrderNumberGenerator
{
    string Generate(
        DateTimeOffset utcNow);
}

public interface IDeliveryFeeCalculator
{
    decimal Calculate(
        DeliveryType deliveryType,
        decimal? latitude,
        decimal? longitude);
}