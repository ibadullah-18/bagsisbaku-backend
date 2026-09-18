using bagsisbaku.Application.Favorites;
using Microsoft.Extensions.DependencyInjection;

namespace bagsisbaku.Infrastructure.Favorites;

public static class FavoriteExtensions
{
    public static IServiceCollection AddFavorites(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(
            services);

        services.AddScoped<
            IFavoriteService,
            FavoriteService>();

        return services;
    }
}