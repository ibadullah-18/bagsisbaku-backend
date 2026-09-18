using bagsisbaku.Application.Promotions.Customer;
using Microsoft.Extensions.DependencyInjection;

namespace bagsisbaku.Infrastructure.Promotions;

public static class PromoCodeExtensions
{
    public static IServiceCollection
        AddPromoCodeValidation(
            this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(
            services);

        services.AddScoped<
            IPromoCodeValidationService,
            PromoCodeValidationService>();

        return services;
    }
}