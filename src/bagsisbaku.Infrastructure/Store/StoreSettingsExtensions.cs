using bagsisbaku.Application.Store;
using Microsoft.Extensions.DependencyInjection;

namespace bagsisbaku.Infrastructure.Store;

public static class StoreSettingsExtensions
{
    public static IServiceCollection AddStoreSettings(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(
            services);

        services.AddScoped<
            IStoreSettingsService,
            StoreSettingsService>();

        return services;
    }
}