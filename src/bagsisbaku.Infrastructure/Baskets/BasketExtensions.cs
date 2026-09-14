using bagsisbaku.Application.Baskets;
using Microsoft.Extensions.DependencyInjection;

namespace bagsisbaku.Infrastructure.Baskets;

public static class BasketExtensions
{
    public static IServiceCollection AddBaskets(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<BasketModelBuilder>();

        services.AddScoped<
            IBasketService,
            BasketService>();

        return services;
    }
}