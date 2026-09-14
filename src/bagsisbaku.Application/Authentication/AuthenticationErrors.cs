using bagsisbaku.Application.Common.Results;

namespace bagsisbaku.Application.Authentication;

public static class AuthenticationErrors
{
    public static readonly Error EmailAlreadyRegistered =
        Error.Conflict(
            "authentication.email-already-registered",
            "Bu email ilə istifadəçi artıq mövcuddur.");

    public static readonly Error InvalidCredentials =
        Error.Unauthorized(
            "authentication.invalid-credentials",
            "Email və ya şifrə yanlışdır.");

    public static readonly Error EmailNotConfirmed =
        Error.Forbidden(
            "authentication.email-not-confirmed",
            "Daxil olmaq üçün email təsdiqlənməlidir.");

    public static readonly Error AccountInactive =
        Error.Forbidden(
            "authentication.account-inactive",
            "İstifadəçi hesabı deaktiv edilib.");

    public static readonly Error UserNotFound =
        Error.NotFound(
            "authentication.user-not-found",
            "İstifadəçi tapılmadı.");

    public static readonly Error InvalidEmailConfirmationToken =
        Error.Validation(
            "authentication.invalid-email-confirmation-token",
            "Email təsdiq token-i yanlışdır və ya vaxtı bitib.");

    public static readonly Error EmailAlreadyConfirmed =
        Error.Conflict(
            "authentication.email-already-confirmed",
            "Email artıq təsdiqlənib.");

    public static readonly Error InvalidRefreshToken =
        Error.Unauthorized(
            "authentication.invalid-refresh-token",
            "Refresh token yanlışdır və ya vaxtı bitib.");

    public static readonly Error InvalidPasswordResetToken =
        Error.Validation(
            "authentication.invalid-password-reset-token",
            "Şifrə yeniləmə token-i yanlışdır və ya vaxtı bitib.");

    public static readonly Error UnexpectedFailure =
        Error.Failure(
            "authentication.unexpected-failure",
            "Authentication əməliyyatı zamanı xəta baş verdi.");

    public static Error IdentityValidation(
        string description)
    {
        return Error.Validation(
            "authentication.identity-validation",
            description);
    }
}
