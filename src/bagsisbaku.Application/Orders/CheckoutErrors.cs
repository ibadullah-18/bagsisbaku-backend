using bagsisbaku.Application.Common.Results;

namespace bagsisbaku.Application.Orders;

public static class CheckoutErrors
{
    public static readonly Error AuthenticationRequired =
        Error.Unauthorized(
            "checkout.authentication-required",
            "Sifariş vermək üçün hesaba daxil olmaq lazımdır.");

    public static readonly Error CustomerNotFound =
        Error.NotFound(
            "checkout.customer-not-found",
            "İstifadəçi hesabı tapılmadı.");

    public static readonly Error CustomerInactive =
        Error.Forbidden(
            "checkout.customer-inactive",
            "İstifadəçi hesabı deaktiv edilib.");

    public static readonly Error BasketEmpty =
        Error.Validation(
            "checkout.basket-empty",
            "Sifariş yaratmaq üçün səbətdə məhsul olmalıdır.");

    public static readonly Error AddressRequired =
        Error.Validation(
            "checkout.address-required",
            "Ünvana çatdırılma üçün ünvan seçilməlidir.");

    public static readonly Error AddressNotAllowed =
        Error.Validation(
            "checkout.address-not-allowed",
            "Mağazadan götürmə zamanı ünvan seçilməməlidir.");

    public static readonly Error AddressNotFound =
        Error.NotFound(
            "checkout.address-not-found",
            "Seçilmiş ünvan tapılmadı.");

    public static readonly Error PickupContactRequired =
        Error.Validation(
            "checkout.pickup-contact-required",
            "Mağazadan götürmə üçün alıcının adı və telefon nömrəsi yazılmalıdır.");

    public static readonly Error ProductUnavailable =
        Error.Conflict(
            "checkout.product-unavailable",
            "Səbətdəki məhsullardan biri hazırda satış üçün aktiv deyil.");

    public static readonly Error ProductVariantUnavailable =
        Error.Conflict(
            "checkout.product-variant-unavailable",
            "Səbətdəki ölçü və ya rəng variantlarından biri aktiv deyil.");

    public static readonly Error OutOfStock =
        Error.Conflict(
            "checkout.out-of-stock",
            "Səbətdəki məhsullardan biri stokda yoxdur.");

    public static Error InsufficientStock(
        string productCode,
        int availableStock)
    {
        return Error.Conflict(
            "checkout.insufficient-stock",
            $"{productCode} məhsulundan stokda yalnız " +
            $"{availableStock} ədəd mövcuddur.");
    }

    public static readonly Error ConcurrencyConflict =
        Error.Conflict(
            "checkout.concurrency-conflict",
            "Məhsul stokunda dəyişiklik oldu. Səbəti yeniləyib yenidən yoxlayın.");

    public static readonly Error OrderNumberConflict =
        Error.Conflict(
            "checkout.order-number-conflict",
            "Sifariş nömrəsi yaradılarkən konflikt baş verdi. Yenidən cəhd edin.");
}