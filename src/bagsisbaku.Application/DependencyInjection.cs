using bagsisbaku.Application.Catalog.Administration;
using bagsisbaku.Application.Catalog.Products;
using bagsisbaku.Application.Media;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace bagsisbaku.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(
            typeof(DependencyInjection).Assembly);

        services.AddScoped<
            ICatalogAdministrationService,
            CatalogAdministrationService>();

        services.AddScoped<
            IProductAdministrationService,
            ProductAdministrationService>();

        services.AddScoped<
            IMediaAdministrationService,
            MediaAdministrationService>();

        return services;
    }
}
