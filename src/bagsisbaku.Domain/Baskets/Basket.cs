using bagsisbaku.Domain.Common;

namespace bagsisbaku.Domain.Baskets;

public sealed class Basket : AuditableEntity
{
    private readonly List<BasketItem> _items = [];

    private Basket()
    {
    }

    private Basket(
        Guid id,
        Guid userId)
        : base(id)
    {
        UserId = DomainGuard.NotEmpty(
            userId,
            nameof(UserId));
    }

    public Guid UserId { get; private set; }

    public IReadOnlyCollection<BasketItem> Items =>
        _items;

    public int TotalQuantity =>
        _items.Sum(item => item.Quantity);

    public static Basket Create(Guid userId)
    {
        return new Basket(
            Guid.NewGuid(),
            userId);
    }

    public BasketItem AddItem(
        Guid productVariantId,
        int quantity)
    {
        var validVariantId =
            DomainGuard.NotEmpty(
                productVariantId,
                nameof(productVariantId));

        var existingItem =
            _items.SingleOrDefault(
                item =>
                    item.ProductVariantId ==
                    validVariantId);

        if (existingItem is not null)
        {
            existingItem.IncreaseQuantity(
                quantity);

            return existingItem;
        }

        var newItem =
            BasketItem.Create(
                Id,
                validVariantId,
                quantity);

        _items.Add(newItem);

        return newItem;
    }

    public void UpdateItemQuantity(
        Guid basketItemId,
        int quantity)
    {
        var validBasketItemId =
            DomainGuard.NotEmpty(
                basketItemId,
                nameof(basketItemId));

        var item =
            _items.SingleOrDefault(
                basketItem =>
                    basketItem.Id ==
                    validBasketItemId)
            ?? throw new DomainException(
                "Səbət məhsulu tapılmadı.");

        item.SetQuantity(quantity);
    }

    public void RemoveItem(Guid basketItemId)
    {
        var validBasketItemId =
            DomainGuard.NotEmpty(
                basketItemId,
                nameof(basketItemId));

        var item =
            _items.SingleOrDefault(
                basketItem =>
                    basketItem.Id ==
                    validBasketItemId)
            ?? throw new DomainException(
                "Səbət məhsulu tapılmadı.");

        _items.Remove(item);
    }

    public void Clear()
    {
        _items.Clear();
    }
}