using bagsisbaku.Application.Orders;
using Microsoft.Extensions.DependencyInjection;

namespace bagsisbaku.Infrastructure.Orders;

public static class CustomerOrderExtensions
{
    public static IServiceCollection AddCustomerOrders(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<
            ICustomerOrderService,
            CustomerOrderService>();

        return services;
    }
}