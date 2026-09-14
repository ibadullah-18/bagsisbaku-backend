using Microsoft.AspNetCore.Authorization;

namespace bagsisbaku.Api.Authorization;

[AttributeUsage(
    AttributeTargets.Class |
    AttributeTargets.Method,
    AllowMultiple = true,
    Inherited = true)]
public sealed class HasPermissionAttribute
    : AuthorizeAttribute
{
    public HasPermissionAttribute(
        string permission)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            permission);

        Policy = permission;
    }
}
