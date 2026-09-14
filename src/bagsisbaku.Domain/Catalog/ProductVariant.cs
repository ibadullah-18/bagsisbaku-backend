using bagsisbaku.Domain.Common;

namespace bagsisbaku.Domain.Catalog;

public sealed class ProductVariant : Entity
{
    private ProductVariant()
    {
    }

    private ProductVariant(
        Guid id,
        Guid productId,
        Guid sizeId,
        Guid colorId,
        int stockCount)
        : base(id)
    {
        ProductId = DomainGuard.NotEmpty(
            productId,
            nameof(ProductId));

        SizeId = DomainGuard.NotEmpty(
            sizeId,
            nameof(SizeId));

        ColorId = DomainGuard.NotEmpty(
            colorId,
            nameof(ColorId));

        StockCount = DomainGuard.NotNegative(
            stockCount,
            nameof(StockCount));

        IsActive = true;
    }

    public Guid ProductId { get; private set; }

    public Guid SizeId { get; private set; }

    public Guid ColorId { get; private set; }

    public int StockCount { get; private set; }

    public bool IsActive { get; private set; }

    public byte[] RowVersion { get; private set; } = [];

    internal static ProductVariant Create(
        Guid productId,
        Guid sizeId,
        Guid colorId,
        int stockCount)
    {
        return new ProductVariant(
            Guid.NewGuid(),
            productId,
            sizeId,
            colorId,
            stockCount);
    }

    public void SetStock(int stockCount)
    {
        StockCount = DomainGuard.NotNegative(
            stockCount,
            nameof(StockCount));
    }

    public void IncreaseStock(int quantity)
    {
        var validQuantity = DomainGuard.Positive(
            quantity,
            nameof(quantity));

        StockCount = checked(StockCount + validQuantity);
    }

    public void DecreaseStock(int quantity)
    {
        var validQuantity = DomainGuard.Positive(
            quantity,
            nameof(quantity));

        if (validQuantity > StockCount)
        {
            throw new DomainException(
                "Stokda kifayət qədər məhsul yoxdur.");
        }

        StockCount -= validQuantity;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
