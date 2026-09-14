namespace bagsisbaku.Application.Security;

public static class PermissionEvaluator
{
    public static bool HasPermission(
        bool isSuperAdmin,
        IReadOnlySet<string> assignedPermissions,
        string requiredPermission)
    {
        ArgumentNullException.ThrowIfNull(assignedPermissions);

        if (string.IsNullOrWhiteSpace(requiredPermission))
        {
            return false;
        }

        if (!PermissionNames.All.Contains(requiredPermission))
        {
            return false;
        }

        if (isSuperAdmin)
        {
            return true;
        }

        return assignedPermissions.Contains(requiredPermission);
    }
}
