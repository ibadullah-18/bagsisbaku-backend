using bagsisbaku.Application.Security;
using Microsoft.Extensions.DependencyInjection;

namespace bagsisbaku.Infrastructure.Authorization;

public static class PermissionAuthorizationExtensions
{
    public static IServiceCollection
        AddPermissionAuthorization(
            this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddAuthorization(
            options =>
            {
                foreach (
                    var permission in PermissionNames.All)
                {
                    options.AddPolicy(
                        permission,
                        policy =>
                        {
                            policy.RequireAuthenticatedUser();

                            policy.RequireClaim(
                                CustomClaimTypes.Permission,
                                permission);
                        });
                }
            });

        return services;
    }
}
