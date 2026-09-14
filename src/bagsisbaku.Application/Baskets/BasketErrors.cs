using bagsisbaku.Application.Common.Results;

namespace bagsisbaku.Application.Baskets;

public static class BasketErrors
{
    public static readonly Error AuthenticationRequired =
        Error.Unauthorized(
            "basket.authentication-required",
            "Səbətdən istifadə etmək üçün hesaba daxil olmaq lazımdır.");

    public static readonly Error CustomerNotFound =
        Error.NotFound(
            "basket.customer-not-found",
            "İstifadəçi hesabı tapılmadı.");

    public static readonly Error CustomerInactive =
        Error.Forbidden(
            "basket.customer-inactive",
            "İstifadəçi hesabı deaktiv edilib.");

    public static readonly Error BasketItemNotFound =
        Error.NotFound(
            "basket.item-not-found",
            "Səbətdə seçilmiş məhsul tapılmadı.");

    public static readonly Error ProductVariantNotFound =
        Error.NotFound(
            "basket.product-variant-not-found",
            "Seçilmiş məhsul variantı tapılmadı.");

    public static readonly Error ProductUnavailable =
        Error.Conflict(
            "basket.product-unavailable",
            "Məhsul hazırda satış üçün aktiv deyil.");

    public static readonly Error ProductVariantUnavailable =
        Error.Conflict(
            "basket.product-variant-unavailable",
            "Seçilmiş ölçü və rəng hazırda aktiv deyil.");

    public static readonly Error OutOfStock =
        Error.Conflict(
            "basket.out-of-stock",
            "Seçilmiş məhsul stokda yoxdur.");

    public static Error InsufficientStock(
        int availableStock)
    {
        return Error.Conflict(
            "basket.insufficient-stock",
            $"Stokda yalnız {availableStock} ədəd məhsul mövcuddur.");
    }

    public static Error InvalidQuantity(
        string description)
    {
        return Error.Validation(
            "basket.invalid-quantity",
            description);
    }
}