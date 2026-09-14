using bagsisbaku.Application.Customers.Administration;
using Microsoft.Extensions.DependencyInjection;

namespace bagsisbaku.Infrastructure.Customers.Administration;

public static class AdminCustomerExtensions
{
    public static IServiceCollection AddAdminCustomers(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<
            IAdminCustomerService,
            AdminCustomerService>();

        return services;
    }
}