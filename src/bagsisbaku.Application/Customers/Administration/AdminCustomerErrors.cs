using bagsisbaku.Application.Common.Results;

namespace bagsisbaku.Application.Customers.Administration;

public static class AdminCustomerErrors
{
    public static readonly Error AdminAuthenticationRequired =
        Error.Unauthorized(
            "admin-customers.authentication-required",
            "Bu əməliyyat üçün admin hesabına daxil olmaq lazımdır.");

    public static readonly Error PermissionDenied =
        Error.Forbidden(
            "admin-customers.permission-denied",
            "Bu əməliyyatı yerinə yetirmək üçün icazəniz yoxdur.");

    public static readonly Error AdminNotFound =
        Error.Unauthorized(
            "admin-customers.admin-not-found",
            "Admin hesabı tapılmadı.");

    public static readonly Error AdminInactive =
        Error.Forbidden(
            "admin-customers.admin-inactive",
            "Admin hesabı deaktiv edilib.");

    public static readonly Error CustomerNotFound =
        Error.NotFound(
            "admin-customers.customer-not-found",
            "Müştəri hesabı tapılmadı.");

    public static readonly Error TargetIsNotCustomer =
        Error.NotFound(
            "admin-customers.target-is-not-customer",
            "Göstərilən istifadəçi müştəri hesabı deyil.");

    public static readonly Error ConcurrencyConflict =
        Error.Conflict(
            "admin-customers.concurrency-conflict",
            "Müştəri məlumatları başqa əməliyyat tərəfindən dəyişdirilib. Yenidən cəhd edin.");

    public static readonly Error UnexpectedFailure =
        Error.Failure(
            "admin-customers.unexpected-failure",
            "Müştəri idarəetmə əməliyyatı zamanı gözlənilməz xəta baş verdi.");

    public static Error Validation(
        string description)
    {
        return Error.Validation(
            "admin-customers.validation",
            description);
    }
}