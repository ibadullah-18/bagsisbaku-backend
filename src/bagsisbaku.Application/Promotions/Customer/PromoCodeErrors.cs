using bagsisbaku.Application.Common.Results;

namespace bagsisbaku.Application.Promotions.Customer;

public static class PromoCodeErrors
{
    public static readonly Error AuthenticationRequired =
        Error.Unauthorized(
            "promo-codes.authentication-required",
            "Promo koddan istifadə etmək üçün hesaba daxil olmaq lazımdır.");

    public static readonly Error InvalidCode =
        Error.NotFound(
            "promo-codes.invalid-code",
            "Promo kod düzgün deyil və ya tapılmadı.");

    public static readonly Error Inactive =
        Error.Conflict(
            "promo-codes.inactive",
            "Promo kod hazırda aktiv deyil.");

    public static readonly Error NotStarted =
        Error.Conflict(
            "promo-codes.not-started",
            "Promo kodun istifadə vaxtı hələ başlamayıb.");

    public static readonly Error Expired =
        Error.Conflict(
            "promo-codes.expired",
            "Promo kodun istifadə müddəti bitib.");

    public static readonly Error UsageLimitReached =
        Error.Conflict(
            "promo-codes.usage-limit-reached",
            "Promo kodun ümumi istifadə limiti dolub.");

    public static readonly Error CustomerUsageLimitReached =
        Error.Conflict(
            "promo-codes.customer-limit-reached",
            "Bu promo koddan icazə verilən sayda istifadə etmisiniz.");

    public static readonly Error BasketEmpty =
        Error.Conflict(
            "promo-codes.basket-empty",
            "Promo kod tətbiq etmək üçün səbətdə məhsul olmalıdır.");

    public static readonly Error BasketUnavailable =
        Error.Conflict(
            "promo-codes.basket-unavailable",
            "Səbətdə hazırda sifariş edilə bilməyən məhsul var.");

    public static readonly Error InvalidRequest =
        Error.Validation(
            "promo-codes.invalid-request",
            "Promo kod məlumatı düzgün deyil.");

    public static readonly Error UnexpectedFailure =
        Error.Failure(
            "promo-codes.unexpected-failure",
            "Promo kod yoxlanılarkən gözlənilməyən xəta baş verdi.");

    public static Error MinimumOrderNotMet(
        decimal minimumOrderAmount)
    {
        return Error.Conflict(
            "promo-codes.minimum-order-not-met",
            $"Bu promo kod üçün minimum sifariş məbləği {minimumOrderAmount:0.00} AZN-dir.");
    }

    public static Error Validation(
        string description)
    {
        return Error.Validation(
            "promo-codes.validation",
            description);
    }
}