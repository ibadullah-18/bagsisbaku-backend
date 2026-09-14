using bagsisbaku.Application.Common.Results;

namespace bagsisbaku.Application.Administration.Admins;

public static class AdminManagementErrors
{
    public static readonly Error AdminNotFound =
        Error.NotFound(
            "admins.not-found",
            "Admin tapılmadı.");

    public static readonly Error EmailAlreadyRegistered =
        Error.Conflict(
            "admins.email-already-registered",
            "Bu email ilə istifadəçi artıq mövcuddur.");

    public static readonly Error TargetIsSuperAdmin =
        Error.Forbidden(
            "admins.target-is-superadmin",
            "Superadmin hesabı admin əməliyyatı ilə dəyişdirilə bilməz.");

    public static readonly Error AdminRoleNotFound =
        Error.Failure(
            "admins.role-not-found",
            "Sistemdə admin rolu tapılmadı.");

    public static Error InvalidPermission(
        string permission)
    {
        return Error.Validation(
            "admins.invalid-permission",
            $"'{permission}' düzgün permission deyil.");
    }

    public static Error IdentityValidation(
        string description)
    {
        return Error.Validation(
            "admins.identity-validation",
            description);
    }
}
