using bagsisbaku.Application.Customers;
using Microsoft.Extensions.DependencyInjection;

namespace bagsisbaku.Infrastructure.Customers;

public static class CustomerProfileExtensions
{
    public static IServiceCollection AddCustomerProfiles(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<
            ICustomerProfileService,
            CustomerProfileService>();

        return services;
    }
}