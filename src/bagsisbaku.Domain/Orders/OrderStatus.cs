namespace bagsisbaku.Domain.Orders;

public enum OrderStatus
{
    Pending = 1,
    Confirmed = 2,
    OnDelivery = 3,
    Delivered = 4,
    Cancelled = 5
}