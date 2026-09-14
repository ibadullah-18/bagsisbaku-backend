using bagsisbaku.Application.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace bagsisbaku.Infrastructure.Authentication;

public static class AuthenticationEmailExtensions
{
    public static IServiceCollection
        AddAuthenticationEmails(
            this IServiceCollection services,
            AuthenticationUrlSettings settings)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(settings);

        settings.Validate();

        services.AddSingleton(settings);

        services.AddSingleton<
            IAuthenticationEmailFactory,
            AuthenticationEmailFactory>();

        return services;
    }
}
