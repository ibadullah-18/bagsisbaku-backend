using bagsisbaku.Application.Announcements;
using Microsoft.Extensions.DependencyInjection;

namespace bagsisbaku.Infrastructure.Announcements;

public static class EmailAnnouncementExtensions
{
    public static IServiceCollection
        AddEmailAnnouncements(
            this IServiceCollection services,
            EmailAnnouncementSettings settings)
    {
        ArgumentNullException.ThrowIfNull(
            services);

        ArgumentNullException.ThrowIfNull(
            settings);

        settings.Validate();

        services.AddSingleton(
            settings);

        services.AddScoped<
            IEmailAnnouncementService,
            EmailAnnouncementService>();

        services.AddScoped<
            IEmailAnnouncementProcessor,
            EmailAnnouncementProcessor>();

        services.AddHostedService<
            EmailAnnouncementBackgroundService>();

        return services;
    }
}