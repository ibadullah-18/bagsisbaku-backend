namespace bagsisbaku.Contracts.Favorites;

public sealed record FavoriteListResponse(
    string Language,
    int TotalCount,
    IReadOnlyList<FavoriteProductResponse> Items);

public sealed record FavoriteProductResponse(
    Guid FavoriteId,
    Guid ProductId,
    string Name,
    string ProductCode,
    string? Model,
    int ProductType,
    string BrandName,
    decimal Price,
    decimal? DiscountPrice,
    decimal CurrentPrice,
    bool IsDiscounted,
    string? ImageUrl,
    bool IsAvailable,
    DateTimeOffset AddedAtUtc);