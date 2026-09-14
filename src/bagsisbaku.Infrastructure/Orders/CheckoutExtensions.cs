using bagsisbaku.Application.Orders;
using Microsoft.Extensions.DependencyInjection;

namespace bagsisbaku.Infrastructure.Orders;

public static class CheckoutExtensions
{
    public static IServiceCollection AddCheckout(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<
            IOrderNumberGenerator,
            OrderNumberGenerator>();

        services.AddSingleton<
            IDeliveryFeeCalculator,
            DefaultDeliveryFeeCalculator>();

        services.AddScoped<
            ICheckoutService,
            CheckoutService>();

        return services;
    }
}