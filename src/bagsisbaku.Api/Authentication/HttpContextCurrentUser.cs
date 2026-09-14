using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using bagsisbaku.Application.Abstractions.Authentication;
using bagsisbaku.Application.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace bagsisbaku.Api.Authentication;

internal sealed class HttpContextCurrentUser(
    IHttpContextAccessor httpContextAccessor)
    : ICurrentUser
{
    private IReadOnlySet<string>? _permissions;

    private ClaimsPrincipal? Principal =>
        httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var userIdValue =
                Principal?
                    .FindFirst(
                        ClaimTypes.NameIdentifier)?
                    .Value
                ?? Principal?
                    .FindFirst(
                        JwtRegisteredClaimNames.Sub)?
                    .Value;

            return Guid.TryParse(
                userIdValue,
                out var userId)
                    ? userId
                    : null;
        }
    }

    public bool IsAuthenticated =>
        Principal?.Identity?.IsAuthenticated == true;

    public bool IsSuperAdmin =>
        Principal?.IsInRole("superadmin") == true;

    public IReadOnlySet<string> Permissions
    {
        get
        {
            if (_permissions is not null)
            {
                return _permissions;
            }

            _permissions =
                Principal?
                    .FindAll(
                        CustomClaimTypes.Permission)
                    .Select(claim => claim.Value)
                    .Where(permission =>
                        !string.IsNullOrWhiteSpace(
                            permission))
                    .ToHashSet(
                        StringComparer.Ordinal)
                ?? new HashSet<string>(
                    StringComparer.Ordinal);

            return _permissions;
        }
    }

    public bool HasPermission(string permission)
    {
        return PermissionEvaluator.HasPermission(
            IsSuperAdmin,
            Permissions,
            permission);
    }
}

public static class CurrentUserExtensions
{
    public static IServiceCollection AddCurrentUser(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHttpContextAccessor();

        services.AddScoped<
            ICurrentUser,
            HttpContextCurrentUser>();

        return services;
    }
}