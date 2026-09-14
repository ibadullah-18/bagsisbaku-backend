using Microsoft.Extensions.DependencyInjection;

namespace bagsisbaku.Infrastructure.Identity;

public static class IdentityBootstrapExtensions
{
    public static IServiceCollection AddIdentityBootstrap(
        this IServiceCollection services,
        IdentityBootstrapSettings settings)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(settings);

        settings.Validate();

        services.AddSingleton(settings);

        services.AddScoped<
            IIdentityBootstrapper,
            IdentityBootstrapper>();

        return services;
    }
}
