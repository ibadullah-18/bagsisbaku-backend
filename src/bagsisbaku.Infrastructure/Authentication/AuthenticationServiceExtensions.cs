using bagsisbaku.Application.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace bagsisbaku.Infrastructure.Authentication;

public static class AuthenticationServiceExtensions
{
    public static IServiceCollection
        AddBagsisbakuAuthentication(
            this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<
            IAuthenticationService,
            AuthenticationService>();

        return services;
    }
}
