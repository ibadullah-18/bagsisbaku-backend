using bagsisbaku.Application.Common.Results;

namespace bagsisbaku.Application.Orders;

public static class CustomerOrderErrors
{
    public static readonly Error AuthenticationRequired =
        Error.Unauthorized(
            "orders.authentication-required",
            "Sifarişlərə baxmaq üçün hesaba daxil olmaq lazımdır.");

    public static readonly Error CustomerNotFound =
        Error.NotFound(
            "orders.customer-not-found",
            "İstifadəçi hesabı tapılmadı.");

    public static readonly Error CustomerInactive =
        Error.Forbidden(
            "orders.customer-inactive",
            "İstifadəçi hesabı deaktiv edilib.");

    public static readonly Error OrderNotFound =
        Error.NotFound(
            "orders.not-found",
            "Sifariş tapılmadı.");

    public static readonly Error CannotCancel =
        Error.Conflict(
            "orders.cannot-cancel",
            "Bu sifariş mövcud statusda ləğv edilə bilməz.");

    public static readonly Error ProductVariantNotFound =
        Error.Conflict(
            "orders.product-variant-not-found",
            "Sifarişdəki məhsul variantı tapılmadığı üçün stok geri qaytarıla bilmədi.");

    public static readonly Error ConcurrencyConflict =
        Error.Conflict(
            "orders.concurrency-conflict",
            "Sifariş və ya stok məlumatı dəyişib. Yenidən cəhd edin.");

    public static readonly Error InvalidOrderId =
        Error.Validation(
            "orders.invalid-id",
            "Sifariş ID-si düzgün deyil.");

    public static readonly Error InvalidPagination =
        Error.Validation(
            "orders.invalid-pagination",
            "Səhifə və səhifə ölçüsü düzgün deyil.");
}