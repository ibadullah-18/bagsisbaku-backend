using bagsisbaku.Domain.Common;

namespace bagsisbaku.Domain.Orders;

public sealed class OrderStatusHistory : Entity
{
    private OrderStatusHistory()
    {
    }

    private OrderStatusHistory(
        Guid id,
        Guid orderId,
        OrderStatus? previousStatus,
        OrderStatus newStatus,
        Guid? changedByUserId,
        string? note,
        DateTimeOffset changedAtUtc)
        : base(id)
    {
        OrderId =
            DomainGuard.NotEmpty(
                orderId,
                nameof(OrderId));

        if (previousStatus.HasValue)
        {
            PreviousStatus =
                DomainGuard.DefinedEnum(
                    previousStatus.Value,
                    nameof(PreviousStatus));
        }

        NewStatus =
            DomainGuard.DefinedEnum(
                newStatus,
                nameof(NewStatus));

        if (changedByUserId == Guid.Empty)
        {
            throw new DomainException(
                "Statusu dəyişən istifadəçi ID-si boş ola bilməz.");
        }

        ChangedByUserId = changedByUserId;

        Note =
            DomainGuard.Optional(
                note,
                nameof(Note),
                500);

        ChangedAtUtc = changedAtUtc;
    }

    public Guid OrderId { get; private set; }

    public OrderStatus? PreviousStatus
    {
        get;
        private set;
    }

    public OrderStatus NewStatus { get; private set; }

    public Guid? ChangedByUserId { get; private set; }

    public string? Note { get; private set; }

    public DateTimeOffset ChangedAtUtc
    {
        get;
        private set;
    }

    internal static OrderStatusHistory Create(
        Guid orderId,
        OrderStatus? previousStatus,
        OrderStatus newStatus,
        Guid? changedByUserId,
        string? note,
        DateTimeOffset changedAtUtc)
    {
        return new OrderStatusHistory(
            Guid.NewGuid(),
            orderId,
            previousStatus,
            newStatus,
            changedByUserId,
            note,
            changedAtUtc);
    }
}