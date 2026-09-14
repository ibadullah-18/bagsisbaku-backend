using bagsisbaku.Domain.Catalog;
using bagsisbaku.Domain.Common;

namespace bagsisbaku.Domain.Orders;

public sealed class OrderItem : Entity
{
    private OrderItem()
    {
    }

    private OrderItem(
        Guid id,
        Guid orderId,
        OrderItemSnapshot snapshot)
        : base(id)
    {
        OrderId =
            DomainGuard.NotEmpty(
                orderId,
                nameof(OrderId));

        ProductId =
            DomainGuard.NotEmpty(
                snapshot.ProductId,
                nameof(ProductId));

        ProductVariantId =
            DomainGuard.NotEmpty(
                snapshot.ProductVariantId,
                nameof(ProductVariantId));

        ProductName =
            DomainGuard.Required(
                snapshot.ProductName,
                nameof(ProductName),
                200);

        ProductCode =
            DomainGuard.Required(
                snapshot.ProductCode,
                nameof(ProductCode),
                80);

        ProductType =
            DomainGuard.DefinedEnum(
                snapshot.ProductType,
                nameof(ProductType));

        BrandName =
            DomainGuard.Required(
                snapshot.BrandName,
                nameof(BrandName),
                120);

        ImageUrl =
            DomainGuard.Optional(
                snapshot.ImageUrl,
                nameof(ImageUrl),
                2048);

        SizeId =
            DomainGuard.NotEmpty(
                snapshot.SizeId,
                nameof(SizeId));

        Size =
            DomainGuard.Required(
                snapshot.Size,
                nameof(Size),
                80);

        ColorId =
            DomainGuard.NotEmpty(
                snapshot.ColorId,
                nameof(ColorId));

        Color =
            DomainGuard.Required(
                snapshot.Color,
                nameof(Color),
                80);

        HexCode =
            DomainGuard.Optional(
                snapshot.HexCode,
                nameof(HexCode),
                16);

        OriginalUnitPrice =
            DomainGuard.Positive(
                snapshot.OriginalUnitPrice,
                nameof(OriginalUnitPrice));

        UnitPrice =
            DomainGuard.Positive(
                snapshot.UnitPrice,
                nameof(UnitPrice));

        if (UnitPrice > OriginalUnitPrice)
        {
            throw new DomainException(
                "Satış qiyməti əsas qiymətdən böyük ola bilməz.");
        }

        Quantity =
            DomainGuard.Positive(
                snapshot.Quantity,
                nameof(Quantity));
    }

    public Guid OrderId { get; private set; }

    public Guid ProductId { get; private set; }

    public Guid ProductVariantId { get; private set; }

    public string ProductName { get; private set; } =
        string.Empty;

    public string ProductCode { get; private set; } =
        string.Empty;

    public ProductType ProductType { get; private set; }

    public string BrandName { get; private set; } =
        string.Empty;

    public string? ImageUrl { get; private set; }

    public Guid SizeId { get; private set; }

    public string Size { get; private set; } =
        string.Empty;

    public Guid ColorId { get; private set; }

    public string Color { get; private set; } =
        string.Empty;

    public string? HexCode { get; private set; }

    public decimal OriginalUnitPrice { get; private set; }

    public decimal UnitPrice { get; private set; }

    public int Quantity { get; private set; }

    public decimal UnitDiscountAmount =>
        OriginalUnitPrice - UnitPrice;

    public decimal LineSubtotal =>
        OriginalUnitPrice * Quantity;

    public decimal LineDiscountAmount =>
        UnitDiscountAmount * Quantity;

    public decimal LineTotal =>
        UnitPrice * Quantity;

    internal static OrderItem Create(
        Guid orderId,
        OrderItemSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        return new OrderItem(
            Guid.NewGuid(),
            orderId,
            snapshot);
    }
}