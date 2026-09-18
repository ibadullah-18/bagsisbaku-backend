namespace bagsisbaku.Application.Favorites;

public sealed record FavoriteListModel(
    string Language,
    int TotalCount,
    IReadOnlyList<FavoriteProductModel> Items);

public sealed record FavoriteProductModel(
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