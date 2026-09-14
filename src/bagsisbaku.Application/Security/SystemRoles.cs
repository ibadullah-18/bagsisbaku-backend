using System.Collections.Frozen;

namespace bagsisbaku.Application.Security;

public static class SystemRoles
{
    public const string SuperAdmin = "superadmin";

    public const string Admin = "admin";

    public const string Customer = "customer";

    private static readonly FrozenSet<string> AllRoles =
        new[]
        {
            SuperAdmin,
            Admin,
            Customer
        }
        .ToFrozenSet(StringComparer.Ordinal);

    public static IReadOnlySet<string> All => AllRoles;

    public static bool IsDefined(string role)
    {
        return !string.IsNullOrWhiteSpace(role) &&
               AllRoles.Contains(role);
    }
}
