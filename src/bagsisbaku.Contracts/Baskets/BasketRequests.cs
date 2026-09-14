namespace bagsisbaku.Contracts.Baskets;

public sealed record AddBasketItemRequest
{
    public Guid ProductVariantId { get; init; }

    public int Quantity { get; init; }
}

public sealed record UpdateBasketItemQuantityRequest
{
    public int Quantity { get; init; }
}