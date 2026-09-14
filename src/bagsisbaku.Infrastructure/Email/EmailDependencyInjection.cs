using bagsisbaku.Application.Abstractions.Email;
using Microsoft.Extensions.DependencyInjection;

namespace bagsisbaku.Infrastructure.Email;

public static class EmailDependencyInjection
{
    public static IServiceCollection AddEmailDelivery(
        this IServiceCollection services,
        SmtpEmailSettings settings)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(settings);

        services.AddSingleton(settings);

        services.AddScoped<
            IEmailSender,
            SmtpEmailSender>();

        return services;
    }
}
