using bagsisbaku.Domain.Catalog;
using bagsisbaku.Domain.Common;
using bagsisbaku.Domain.Orders;
using Xunit;

namespace bagsisbaku.Domain.UnitTests;

public sealed class OrderTests
{
    [Fact]
    public void CreateCalculatesOrderTotals()
    {
        var order =
            CreateAddressDeliveryOrder();

        Assert.Equal(
            OrderStatus.Pending,
            order.Status);

        Assert.Equal(
            440m,
            order.Subtotal);

        Assert.Equal(
            80.20m,
            order.DiscountAmount);

        Assert.Equal(
            5m,
            order.DeliveryFee);

        Assert.Equal(
            364.80m,
            order.Total);

        Assert.Single(order.Items);
        Assert.Single(order.StatusHistory);

        var history =
            order.StatusHistory.Single();

        Assert.Null(history.PreviousStatus);

        Assert.Equal(
            OrderStatus.Pending,
            history.NewStatus);
    }

    [Fact]
    public void AddressDeliveryCanCompleteLifecycle()
    {
        var order =
            CreateAddressDeliveryOrder();

        var adminId = Guid.NewGuid();

        order.Confirm(
            adminId,
            DateTimeOffset.UtcNow);

        order.StartDelivery(
            adminId,
            DateTimeOffset.UtcNow);

        order.MarkDelivered(
            adminId,
            DateTimeOffset.UtcNow);

        Assert.Equal(
            OrderStatus.Delivered,
            order.Status);

        Assert.NotNull(order.ConfirmedAtUtc);
        Assert.NotNull(order.OnDeliveryAtUtc);
        Assert.NotNull(order.DeliveredAtUtc);

        Assert.Equal(
            4,
            order.StatusHistory.Count);
    }

    [Fact]
    public void StorePickupCannotStartDelivery()
    {
        var delivery =
            OrderDeliverySnapshot.ForStorePickup(
                "Ibadulla Huseynzade",
                "+994501234567");

        var order =
            Order.Create(
                Guid.NewGuid(),
                "bagsis-test-0002",
                DeliveryType.StorePickup,
                PaymentMethod.Cash,
                delivery,
                null,
                0m,
                DateTimeOffset.UtcNow,
                [CreateItemSnapshot()]);

        order.Confirm(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow);

        Assert.Throws<DomainException>(
            () => order.StartDelivery(
                Guid.NewGuid(),
                DateTimeOffset.UtcNow));
    }

    [Fact]
    public void CancelledOrderCannotBeConfirmed()
    {
        var order =
            CreateAddressDeliveryOrder();

        order.Cancel(
            Guid.NewGuid(),
            "Müştəri sifarişi ləğv etdi.",
            DateTimeOffset.UtcNow);

        Assert.Equal(
            OrderStatus.Cancelled,
            order.Status);

        Assert.NotNull(order.CancelledAtUtc);
        Assert.False(order.CanBeCancelled);

        Assert.Throws<DomainException>(
            () => order.Confirm(
                Guid.NewGuid(),
                DateTimeOffset.UtcNow));
    }

    private static Order CreateAddressDeliveryOrder()
    {
        var delivery =
            OrderDeliverySnapshot.ForAddressDelivery(
                Guid.NewGuid(),
                "Ibadulla Huseynzade",
                "+994501234567",
                "Bakı",
                "Yasamal",
                "Həsən bəy Zərdabi küçəsi 10",
                "AZ1000",
                "Zəng edin.",
                40.389m,
                49.809m);

        return Order.Create(
            Guid.NewGuid(),
            "bagsis-test-0001",
            DeliveryType.AddressDelivery,
            PaymentMethod.Cash,
            delivery,
            "Məhsulu diqqətlə qablaşdırın.",
            5m,
            DateTimeOffset.UtcNow,
            [CreateItemSnapshot()]);
    }

    private static OrderItemSnapshot
        CreateItemSnapshot()
    {
        return new OrderItemSnapshot(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Sarı adidas idman ayaqqabısı",
            "BAGSIS-TEST-SHOE-001",
            ProductType.Shoe,
            "adidas",
            "https://images.example.test/shoe.png",
            Guid.NewGuid(),
            "42",
            Guid.NewGuid(),
            "Sarı",
            "#FFD700",
            220m,
            179.90m,
            2);
    }
}