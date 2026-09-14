using bagsisbaku.Application.Administration.Admins;
using Microsoft.Extensions.DependencyInjection;

namespace bagsisbaku.Infrastructure.Administration.Admins;

public static class AdminManagementExtensions
{
    public static IServiceCollection AddAdminManagement(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<
            IAdminManagementService,
            AdminManagementService>();

        return services;
    }
}
