using bagsisbaku.Domain.Common;

namespace bagsisbaku.Domain.Orders;

public sealed class Order : AuditableEntity
{
    private readonly List<OrderItem> _items = [];

    private readonly List<OrderStatusHistory>
        _statusHistory = [];

    private Order()
    {
    }

    private Order(
        Guid id,
        Guid userId,
        string orderNumber,
        DeliveryType deliveryType,
        PaymentMethod paymentMethod,
        OrderDeliverySnapshot delivery,
        string? customerNote,
        OrderPromotionSnapshot? promotion,
        decimal deliveryFee,
        DateTimeOffset placedAtUtc,
        IReadOnlyCollection<OrderItemSnapshot> items)
        : base(id)
    {
        UserId =
            DomainGuard.NotEmpty(
                userId,
                nameof(UserId));

        OrderNumber =
            DomainGuard.Required(
                orderNumber,
                nameof(OrderNumber),
                40);

        DeliveryType =
            DomainGuard.DefinedEnum(
                deliveryType,
                nameof(DeliveryType));

        PaymentMethod =
            DomainGuard.DefinedEnum(
                paymentMethod,
                nameof(PaymentMethod));

        ArgumentNullException.ThrowIfNull(delivery);
        ArgumentNullException.ThrowIfNull(items);

        if (items.Count == 0)
        {
            throw new DomainException(
                "Sifarişdə ən azı bir məhsul olmalıdır.");
        }

        if (deliveryFee < 0)
        {
            throw new DomainException(
                "Çatdırılma qiyməti mənfi ola bilməz.");
        }

        if (DeliveryType == DeliveryType.StorePickup &&
            delivery.CustomerAddressId.HasValue)
        {
            throw new DomainException(
                "Mağazadan götürmə zamanı ünvan seçilməməlidir.");
        }

        if (DeliveryType == DeliveryType.AddressDelivery &&
            !delivery.CustomerAddressId.HasValue)
        {
            throw new DomainException(
                "Ünvana çatdırılma üçün ünvan seçilməlidir.");
        }

        if (DeliveryType == DeliveryType.StorePickup &&
            deliveryFee != 0)
        {
            throw new DomainException(
                "Mağazadan götürmə üçün çatdırılma qiyməti sıfır olmalıdır.");
        }

        CustomerAddressId =
            delivery.CustomerAddressId;

        RecipientFullName =
            delivery.RecipientFullName;

        PhoneNumber =
            delivery.PhoneNumber;

        City = delivery.City;
        District = delivery.District;
        AddressLine = delivery.AddressLine;
        PostalCode = delivery.PostalCode;
        DeliveryNote = delivery.DeliveryNote;
        Latitude = delivery.Latitude;
        Longitude = delivery.Longitude;

        CustomerNote =
            DomainGuard.Optional(
                customerNote,
                nameof(CustomerNote),
                1000);

        PromoCodeId =
            promotion?.PromoCodeId;

        PromoCode =
            promotion?.Code;

        PromoDiscountAmount =
            promotion?.DiscountAmount ?? 0;

        DeliveryFee = deliveryFee;
        PlacedAtUtc = placedAtUtc;
        Status = OrderStatus.Pending;

        foreach (var snapshot in items)
        {
            _items.Add(
                OrderItem.Create(
                    Id,
                    snapshot));
        }

        Subtotal =
            _items.Sum(
                item => item.LineSubtotal);

        DiscountAmount =
            _items.Sum(
                item => item.LineDiscountAmount);

        var discountedItemsTotal =
            _items.Sum(
                item => item.LineTotal);

        if (PromoDiscountAmount >
            discountedItemsTotal)
        {
            throw new DomainException(
                "Promo kod endirimi məhsulların yekun məbləğindən böyük ola bilməz.");
        }

        Total =
            discountedItemsTotal -
            PromoDiscountAmount +
            DeliveryFee;

        AddStatusHistory(
            previousStatus: null,
            newStatus: OrderStatus.Pending,
            changedByUserId: UserId,
            note: "Sifariş yaradıldı.",
            changedAtUtc: placedAtUtc);
    }

    public Guid UserId { get; private set; }

    public string OrderNumber { get; private set; } =
        string.Empty;

    public OrderStatus Status { get; private set; }

    public DeliveryType DeliveryType { get; private set; }

    public PaymentMethod PaymentMethod { get; private set; }

    public Guid? CustomerAddressId { get; private set; }

    public string RecipientFullName { get; private set; } =
        string.Empty;

    public string PhoneNumber { get; private set; } =
        string.Empty;

    public string? City { get; private set; }

    public string? District { get; private set; }

    public string? AddressLine { get; private set; }

    public string? PostalCode { get; private set; }

    public string? DeliveryNote { get; private set; }

    public decimal? Latitude { get; private set; }

    public decimal? Longitude { get; private set; }

    public string? CustomerNote { get; private set; }

    public decimal Subtotal { get; private set; }

    public decimal DiscountAmount { get; private set; }

    public Guid? PromoCodeId { get; private set; }

    public string? PromoCode { get; private set; }

    public decimal PromoDiscountAmount
    {
        get;
        private set;
    }

    public decimal TotalDiscountAmount =>
        DiscountAmount +
        PromoDiscountAmount;

    public decimal DeliveryFee { get; private set; }

    public decimal Total { get; private set; }

    public DateTimeOffset PlacedAtUtc { get; private set; }

    public DateTimeOffset? ConfirmedAtUtc
    {
        get;
        private set;
    }

    public DateTimeOffset? OnDeliveryAtUtc
    {
        get;
        private set;
    }

    public DateTimeOffset? DeliveredAtUtc
    {
        get;
        private set;
    }

    public DateTimeOffset? CancelledAtUtc
    {
        get;
        private set;
    }

    public byte[] RowVersion { get; private set; } = [];

    public IReadOnlyCollection<OrderItem> Items =>
        _items;

    public IReadOnlyCollection<OrderStatusHistory>
        StatusHistory =>
            _statusHistory;

    public bool CanBeCancelled =>
        Status is
            OrderStatus.Pending or
            OrderStatus.Confirmed;

    public static Order Create(
        Guid userId,
        string orderNumber,
        DeliveryType deliveryType,
        PaymentMethod paymentMethod,
        OrderDeliverySnapshot delivery,
        string? customerNote,
        decimal deliveryFee,
        DateTimeOffset placedAtUtc,
        IReadOnlyCollection<OrderItemSnapshot> items)
    {
        return Create(
            userId,
            orderNumber,
            deliveryType,
            paymentMethod,
            delivery,
            customerNote,
            promotion: null,
            deliveryFee,
            placedAtUtc,
            items);
    }
    public static Order Create(
        Guid userId,
        string orderNumber,
        DeliveryType deliveryType,
        PaymentMethod paymentMethod,
        OrderDeliverySnapshot delivery,
        string? customerNote,
        OrderPromotionSnapshot? promotion,
        decimal deliveryFee,
        DateTimeOffset placedAtUtc,
        IReadOnlyCollection<OrderItemSnapshot> items)
    {
        return new Order(
            Guid.NewGuid(),
            userId,
            orderNumber,
            deliveryType,
            paymentMethod,
            delivery,
            customerNote,
            promotion,
            deliveryFee,
            placedAtUtc,
            items);
    }

    public void Confirm(
        Guid changedByUserId,
        DateTimeOffset changedAtUtc,
        string? note = null)
    {
        if (Status != OrderStatus.Pending)
        {
            throw InvalidStatusTransition(
                OrderStatus.Confirmed);
        }

        ChangeStatus(
            OrderStatus.Confirmed,
            changedByUserId,
            note,
            changedAtUtc);

        ConfirmedAtUtc = changedAtUtc;
    }

    public void StartDelivery(
        Guid changedByUserId,
        DateTimeOffset changedAtUtc,
        string? note = null)
    {
        if (DeliveryType !=
            DeliveryType.AddressDelivery)
        {
            throw new DomainException(
                "Mağazadan götürülən sifariş çatdırılmaya çıxarıla bilməz.");
        }

        if (Status != OrderStatus.Confirmed)
        {
            throw InvalidStatusTransition(
                OrderStatus.OnDelivery);
        }

        ChangeStatus(
            OrderStatus.OnDelivery,
            changedByUserId,
            note,
            changedAtUtc);

        OnDeliveryAtUtc = changedAtUtc;
    }

    public void MarkDelivered(
        Guid changedByUserId,
        DateTimeOffset changedAtUtc,
        string? note = null)
    {
        var expectedStatus =
            DeliveryType ==
            DeliveryType.AddressDelivery
                ? OrderStatus.OnDelivery
                : OrderStatus.Confirmed;

        if (Status != expectedStatus)
        {
            throw InvalidStatusTransition(
                OrderStatus.Delivered);
        }

        ChangeStatus(
            OrderStatus.Delivered,
            changedByUserId,
            note,
            changedAtUtc);

        DeliveredAtUtc = changedAtUtc;
    }

    public void Cancel(
        Guid changedByUserId,
        string reason,
        DateTimeOffset changedAtUtc)
    {
        if (!CanBeCancelled)
        {
            throw InvalidStatusTransition(
                OrderStatus.Cancelled);
        }

        var validReason =
            DomainGuard.Required(
                reason,
                nameof(reason),
                500);

        ChangeStatus(
            OrderStatus.Cancelled,
            changedByUserId,
            validReason,
            changedAtUtc);

        CancelledAtUtc = changedAtUtc;
    }

    private void ChangeStatus(
        OrderStatus newStatus,
        Guid changedByUserId,
        string? note,
        DateTimeOffset changedAtUtc)
    {
        var validUserId =
            DomainGuard.NotEmpty(
                changedByUserId,
                nameof(changedByUserId));

        var previousStatus = Status;

        Status =
            DomainGuard.DefinedEnum(
                newStatus,
                nameof(newStatus));

        AddStatusHistory(
            previousStatus,
            Status,
            validUserId,
            note,
            changedAtUtc);
    }

    private void AddStatusHistory(
        OrderStatus? previousStatus,
        OrderStatus newStatus,
        Guid? changedByUserId,
        string? note,
        DateTimeOffset changedAtUtc)
    {
        _statusHistory.Add(
            OrderStatusHistory.Create(
                Id,
                previousStatus,
                newStatus,
                changedByUserId,
                note,
                changedAtUtc));
    }

    private DomainException InvalidStatusTransition(
        OrderStatus newStatus)
    {
        return new DomainException(
            $"Sifariş statusu {Status} vəziyyətindən " +
            $"{newStatus} vəziyyətinə keçirilə bilməz.");
    }
}