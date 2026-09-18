using bagsisbaku.Application.Promotions.Administration;
using Microsoft.Extensions.DependencyInjection;

namespace bagsisbaku.Infrastructure.Promotions.Administration;

public static class AdminPromotionExtensions
{
    public static IServiceCollection
        AddAdminPromotions(
            this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(
            services);

        services.AddScoped<
            IAdminPromotionService,
            AdminPromotionService>();

        return services;
    }
}