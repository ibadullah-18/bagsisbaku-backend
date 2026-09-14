using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace bagsisbaku.Api.OpenApi;

public static class SwaggerExtensions
{
    public static IServiceCollection AddApiDocumentation(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(
                "v1",
                new OpenApiInfo
                {
                    Title = "bagsisbaku api",
                    Version = "v1",
                    Description =
                        "bagsisbaku mağazasının backend API-si."
                });

            options.AddSecurityDefinition(
                "bearer",
                new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description =
                        "JWT access token daxil edin. " +
                        "Bearer sözünü yazmaq lazım deyil."
                });

            options.AddSecurityRequirement(
                document =>
                    new OpenApiSecurityRequirement
                    {
                        [
                            new OpenApiSecuritySchemeReference(
                                "bearer",
                                document)
                        ] = []
                    });
        });

        return services;
    }
}