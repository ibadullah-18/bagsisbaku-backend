using bagsisbaku.Application.Common.Results;

namespace bagsisbaku.Application.Customers;

public static class CustomerProfileErrors
{
    public static readonly Error AuthenticationRequired =
        Error.Unauthorized(
            "customers.authentication-required",
            "Bu əməliyyat üçün hesaba daxil olmaq lazımdır.");

    public static readonly Error CustomerNotFound =
        Error.NotFound(
            "customers.not-found",
            "Müştəri hesabı tapılmadı.");

    public static readonly Error CustomerInactive =
        Error.Forbidden(
            "customers.inactive",
            "Müştəri hesabı deaktiv edilib.");

    public static readonly Error AddressNotFound =
        Error.NotFound(
            "customers.address-not-found",
            "Ünvan tapılmadı.");

    public static readonly Error UnexpectedFailure =
        Error.Failure(
            "customers.unexpected-failure",
            "Profil əməliyyatı zamanı gözlənilməz xəta baş verdi.");

    public static Error Validation(
        string description)
    {
        return Error.Validation(
            "customers.validation",
            description);
    }
}