using bagsisbaku.Application.Abstractions.Persistence;
using bagsisbaku.Application.Abstractions.Storage;
using bagsisbaku.Application.Abstractions.Time;
using bagsisbaku.Application.Catalog.Products.Public;
using bagsisbaku.Application.Catalog.Public;
using bagsisbaku.Infrastructure.Identity;
using bagsisbaku.Infrastructure.Persistence;
using bagsisbaku.Infrastructure.Persistence.Interceptors;
using bagsisbaku.Infrastructure.Persistence.Queries;
using bagsisbaku.Infrastructure.Persistence.Repositories;
using bagsisbaku.Infrastructure.Storage;
using bagsisbaku.Infrastructure.Time;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace bagsisbaku.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString,
        CloudinarySettings cloudinarySettings)
    {
        ArgumentNullException.ThrowIfNull(
            cloudinarySettings);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            connectionString);

        services.AddSingleton(cloudinarySettings);

        services.AddSingleton<
            IImageStorage,
            CloudinaryImageStorage>();

        services.AddSingleton<IClock, SystemClock>();

        services.AddSingleton<
            AuditableEntityInterceptor>();

        services.AddDbContext<ApplicationDbContext>(
            (serviceProvider, options) =>
            {
                options.UseSqlServer(
                    connectionString,
                    sqlServerOptions =>
                    {
                        sqlServerOptions.MigrationsAssembly(
                            typeof(ApplicationDbContext)
                                .Assembly
                                .FullName);

                        sqlServerOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay:
                                TimeSpan.FromSeconds(10),
                            errorNumbersToAdd: null);
                    });

                options.AddInterceptors(
                    serviceProvider.GetRequiredService<
                        AuditableEntityInterceptor>());
            });

        services.AddDataProtection();

        services
            .AddIdentityCore<AppUser>(
                options =>
                {
                    options.User.RequireUniqueEmail = true;

                    options.SignIn.RequireConfirmedEmail = true;

                    options.Password.RequiredLength = 8;
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireUppercase = true;

                    options.Password
                        .RequireNonAlphanumeric = true;

                    options.Lockout.AllowedForNewUsers = true;

                    options.Lockout.MaxFailedAccessAttempts = 5;

                    options.Lockout.DefaultLockoutTimeSpan =
                        TimeSpan.FromMinutes(15);
                })
            .AddRoles<AppRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.Configure<
            DataProtectionTokenProviderOptions>(
            options =>
            {
                options.TokenLifespan =
                    TimeSpan.FromMinutes(20);
            });

        services.AddScoped<IUnitOfWork>(
            serviceProvider =>
                serviceProvider.GetRequiredService<
                    ApplicationDbContext>());

        services.AddScoped<
            IProductRepository,
            ProductRepository>();

        services.AddScoped<
            IBrandRepository,
            BrandRepository>();

        services.AddScoped<
            ICategoryRepository,
            CategoryRepository>();

        services.AddScoped<
            ISizeRepository,
            SizeRepository>();

        services.AddScoped<
            IColorRepository,
            ColorRepository>();

        services.AddScoped<
            ICatalogDefaultRepository,
            CatalogDefaultRepository>();

        services.AddScoped<
            ITranslationRepository,
            TranslationRepository>();

        services.AddScoped<
            IProductCatalogQuery,
            ProductCatalogQuery>();

        services.AddScoped<
            IPublicCatalogFilterQuery,
            PublicCatalogFilterQuery>();

        return services;
    }
}

