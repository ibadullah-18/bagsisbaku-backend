using bagsisbaku.Application.Common.Results;

namespace bagsisbaku.Application.Orders.Administration;

public static class AdminOrderErrors
{
    public static readonly Error AuthenticationRequired =
        Error.Unauthorized(
            "admin-orders.authentication-required",
            "Bu əməliyyat üçün sistemə daxil olmaq lazımdır.");

    public static readonly Error AdministratorNotFound =
        Error.NotFound(
            "admin-orders.administrator-not-found",
            "Admin hesabı tapılmadı.");

    public static readonly Error AdministratorInactive =
        Error.Forbidden(
            "admin-orders.administrator-inactive",
            "Admin hesabı deaktiv edilib.");

    public static readonly Error OrderNotFound =
        Error.NotFound(
            "admin-orders.not-found",
            "Sifariş tapılmadı.");

    public static readonly Error InvalidOrderId =
        Error.Validation(
            "admin-orders.invalid-id",
            "Sifariş ID-si düzgün deyil.");

    public static readonly Error InvalidFilter =
        Error.Validation(
            "admin-orders.invalid-filter",
            "Sifariş filter məlumatları düzgün deyil.");

    public static readonly Error InvalidStatus =
        Error.Validation(
            "admin-orders.invalid-status",
            "Sifariş statusu düzgün deyil.");

    public static readonly Error InvalidTransition =
        Error.Conflict(
            "admin-orders.invalid-transition",
            "Sifariş bu statusa keçirilə bilməz.");

    public static readonly Error ProductVariantNotFound =
        Error.Conflict(
            "admin-orders.product-variant-not-found",
            "Məhsul variantı tapılmadığı üçün stok geri qaytarıla bilmədi.");

    public static readonly Error ConcurrencyConflict =
        Error.Conflict(
            "admin-orders.concurrency-conflict",
            "Sifariş və ya stok məlumatı dəyişib. Yenidən cəhd edin.");

    public static Error Validation(
        string description)
    {
        return Error.Validation(
            "admin-orders.validation",
            description);
    }
}