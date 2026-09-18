using bagsisbaku.Application.Common.Results;

namespace bagsisbaku.Application.Promotions.Administration;

public static class AdminPromotionErrors
{
    public static readonly Error PromoCodeNotFound =
        Error.NotFound(
            "admin-promotions.not-found",
            "Promo kod tapılmadı.");

    public static readonly Error CodeAlreadyExists =
        Error.Conflict(
            "admin-promotions.code-exists",
            "Bu promo kod artıq mövcuddur.");

    public static readonly Error InvalidFilter =
        Error.Validation(
            "admin-promotions.invalid-filter",
            "Promo kod filter məlumatları düzgün deyil.");

    public static readonly Error InvalidRequest =
        Error.Validation(
            "admin-promotions.invalid-request",
            "Promo kod məlumatları düzgün deyil.");

    public static readonly Error ConcurrencyConflict =
        Error.Conflict(
            "admin-promotions.concurrency-conflict",
            "Promo kod başqa əməliyyat tərəfindən dəyişdirilib. Yenidən cəhd edin.");

    public static readonly Error UnexpectedFailure =
        Error.Failure(
            "admin-promotions.unexpected-failure",
            "Promo kod əməliyyatı zamanı gözlənilməyən xəta baş verdi.");

    public static Error Validation(
        string description)
    {
        return Error.Validation(
            "admin-promotions.validation",
            description);
    }
}