using bagsisbaku.Application.Orders.Administration;
using Microsoft.Extensions.DependencyInjection;

namespace bagsisbaku.Infrastructure.Orders.Administration;

public static class AdminOrderExtensions
{
    public static IServiceCollection AddAdminOrders(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<
            IAdminOrderService,
            AdminOrderService>();

        return services;
    }
}