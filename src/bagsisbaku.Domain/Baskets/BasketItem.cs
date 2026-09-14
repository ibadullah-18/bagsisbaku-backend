using bagsisbaku.Domain.Common;

namespace bagsisbaku.Domain.Baskets;

public sealed class BasketItem : Entity
{
    public const int MaximumQuantity = 99;

    private BasketItem()
    {
    }

    private BasketItem(
        Guid id,
        Guid basketId,
        Guid productVariantId,
        int quantity)
        : base(id)
    {
        BasketId = DomainGuard.NotEmpty(
            basketId,
            nameof(BasketId));

        ProductVariantId = DomainGuard.NotEmpty(
            productVariantId,
            nameof(ProductVariantId));

        SetQuantity(quantity);
    }

    public Guid BasketId { get; private set; }

    public Guid ProductVariantId { get; private set; }

    public int Quantity { get; private set; }

    internal static BasketItem Create(
        Guid basketId,
        Guid productVariantId,
        int quantity)
    {
        return new BasketItem(
            Guid.NewGuid(),
            basketId,
            productVariantId,
            quantity);
    }

    public void SetQuantity(int quantity)
    {
        var validQuantity =
            DomainGuard.Positive(
                quantity,
                nameof(Quantity));

        if (validQuantity > MaximumQuantity)
        {
            throw new DomainException(
                $"Bir məhsuldan maksimum " +
                $"{MaximumQuantity} ədəd səbətə əlavə edilə bilər.");
        }

        Quantity = validQuantity;
    }

    public void IncreaseQuantity(int quantity)
    {
        var validQuantity =
            DomainGuard.Positive(
                quantity,
                nameof(quantity));

        var newQuantity =
            checked(Quantity + validQuantity);

        SetQuantity(newQuantity);
    }
}