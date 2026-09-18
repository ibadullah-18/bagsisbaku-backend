using System.Net.Http.Headers;
using bagsisbaku.Application.Abstractions.Authentication;
using bagsisbaku.Application.Security;
using bagsisbaku.Domain.Catalog;
using bagsisbaku.Domain.Localization;
using bagsisbaku.Infrastructure.Identity;
using bagsisbaku.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using Xunit;

namespace bagsisbaku.IntegrationTests;

public sealed partial class CatalogApiFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _sqlServer =
        new MsSqlBuilder(
            "mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    private BagsisbakuApiFactory? _factory;

    public HttpClient Client { get; private set; } =
        null!;

    public HttpClient AuthenticatedClient
    {
        get;
        private set;
    } = null!;

    public Guid CustomerId
    {
        get;
        private set;
    }

    public string CustomerViewerAccessToken
    {
        get;
        private set;
    } = string.Empty;

    public string CustomerManagerAccessToken
    {
        get;
        private set;
    } = string.Empty;

    public string CustomerRefreshToken
    {
        get;
        private set;
    } = string.Empty;
    public string OrderViewerAccessToken
    {
        get;
        private set;
    } = string.Empty;

    public string OrderManagerAccessToken
    {
        get;
        private set;
    } = string.Empty;

    public Guid ShoeProductId { get; private set; }

    public Guid ShoeVariantId { get; private set; }

    public Guid BagProductId { get; private set; }

    public Guid OutOfStockProductId
    {
        get;
        private set;
    }

    public Guid OutOfStockVariantId
    {
        get;
        private set;
    }

    public async ValueTask InitializeAsync()
    {
        await _sqlServer.StartAsync();

        _factory =
            new BagsisbakuApiFactory(
                _sqlServer.GetConnectionString());

        var clientOptions =
            new WebApplicationFactoryClientOptions
            {
                BaseAddress =
                    new Uri("https://localhost"),

                AllowAutoRedirect = false
            };

        Client =
            _factory.CreateClient(clientOptions);

        AuthenticatedClient =
            _factory.CreateClient(clientOptions);

        await using var scope =
            _factory.Services.CreateAsyncScope();

        var dbContext =
            scope.ServiceProvider.GetRequiredService<
                ApplicationDbContext>();

        await dbContext.Database.MigrateAsync();

        var customer =
            await SeedAsync(dbContext);

        var refreshTokenGenerator =
            scope.ServiceProvider.GetRequiredService<
                IRefreshTokenGenerator>();

        var customerAdministrator =
            await SeedCustomerAdministrationAsync(
                dbContext,
                customer,
                refreshTokenGenerator);

        var accessTokenGenerator =
            scope.ServiceProvider.GetRequiredService<
                IAccessTokenGenerator>();

        var accessToken =
            accessTokenGenerator.Generate(
                new AccessTokenUser(
                    customer.Id,
                    customer.Email!,
                    customer.FullName,
                    [],
                    []));

        AuthenticatedClient
            .DefaultRequestHeaders
            .Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    accessToken.Token);

        var orderViewerToken =
            accessTokenGenerator.Generate(
                new AccessTokenUser(
                    customerAdministrator.Id,
                    customerAdministrator.Email!,
                    customerAdministrator.FullName,
                    [SystemRoles.Admin],
                    [
                        PermissionNames.Orders.View
                    ]));

        OrderViewerAccessToken =
            orderViewerToken.Token;

        var orderManagerToken =
            accessTokenGenerator.Generate(
                new AccessTokenUser(
                    customerAdministrator.Id,
                    customerAdministrator.Email!,
                    customerAdministrator.FullName,
                    [SystemRoles.Admin],
                    [
                        PermissionNames.Orders.View,
                        PermissionNames.Orders.UpdateStatus
                    ]));

        OrderManagerAccessToken =
            orderManagerToken.Token;

        var customerViewerToken =
            accessTokenGenerator.Generate(
                new AccessTokenUser(
                    customerAdministrator.Id,
                    customerAdministrator.Email!,
                    customerAdministrator.FullName,
                    [SystemRoles.Admin],
                    [
                        PermissionNames.Customers.View
                    ]));

        CustomerViewerAccessToken =
            customerViewerToken.Token;

        var customerManagerToken =
            accessTokenGenerator.Generate(
                new AccessTokenUser(
                    customerAdministrator.Id,
                    customerAdministrator.Email!,
                    customerAdministrator.FullName,
                    [SystemRoles.Admin],
                    [
                        PermissionNames.Customers.View,
                        PermissionNames.Customers.Manage
                    ]));

        CustomerManagerAccessToken =
            customerManagerToken.Token;
    }

    public async Task<bool>
        IsCustomerRefreshTokenRevokedAsync(
            CancellationToken cancellationToken = default)
    {
        if (_factory is null)
        {
            throw new InvalidOperationException(
                "Integration test factory başladılmayıb.");
        }

        await using var scope =
            _factory.Services.CreateAsyncScope();

        var dbContext =
            scope.ServiceProvider.GetRequiredService<
                ApplicationDbContext>();

        var generatedRefreshTokenHash =
            scope.ServiceProvider
                .GetRequiredService<
                    IRefreshTokenGenerator>()
                .Hash(CustomerRefreshToken);

        return await dbContext.RefreshTokens
            .AsNoTracking()
            .Where(token =>
                token.TokenHash ==
                generatedRefreshTokenHash)
            .Select(token =>
                token.RevokedAtUtc.HasValue)
            .SingleAsync(
                cancellationToken);
    }
    public async ValueTask DisposeAsync()
    {
        AuthenticatedClient?.Dispose();
        Client?.Dispose();

        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }

        await _sqlServer.DisposeAsync();
    }

    private async Task<AppUser>
        SeedCustomerAdministrationAsync(
            ApplicationDbContext dbContext,
            AppUser customer,
            IRefreshTokenGenerator refreshTokenGenerator)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(customer);
        ArgumentNullException.ThrowIfNull(
            refreshTokenGenerator);

        var normalizedCustomerRole =
            SystemRoles.Customer.ToUpperInvariant();

        var customerRole =
            await dbContext.Roles
                .SingleOrDefaultAsync(role =>
                    role.NormalizedName ==
                    normalizedCustomerRole);

        if (customerRole is null)
        {
            customerRole =
                AppRole.Create(
                    SystemRoles.Customer,
                    "Integration test müştəri rolu");

            dbContext.Roles.Add(customerRole);
        }

        var normalizedAdminRole =
            SystemRoles.Admin.ToUpperInvariant();

        var adminRole =
            await dbContext.Roles
                .SingleOrDefaultAsync(role =>
                    role.NormalizedName ==
                    normalizedAdminRole);

        if (adminRole is null)
        {
            adminRole =
                AppRole.Create(
                    SystemRoles.Admin,
                    "Integration test admin rolu");

            dbContext.Roles.Add(adminRole);
        }

        customer.EmailConfirmed = true;

        var customerAdministrator =
            AppUser.Create(
                "Customer Administration Test Admin",
                "customer.admin@bagsisbaku.test",
                DateTimeOffset.UtcNow);

        customerAdministrator.EmailConfirmed = true;

        dbContext.Users.Add(
            customerAdministrator);

        dbContext.UserRoles.AddRange(
            new IdentityUserRole<Guid>
            {
                UserId = customer.Id,
                RoleId = customerRole.Id
            },
            new IdentityUserRole<Guid>
            {
                UserId = customerAdministrator.Id,
                RoleId = adminRole.Id
            });

        var generatedRefreshToken =
            refreshTokenGenerator.Generate();

        var utcNow =
            DateTimeOffset.UtcNow;

        var customerRefreshToken =
            RefreshToken.Create(
                customer.Id,
                generatedRefreshToken.TokenHash,
                utcNow,
                utcNow.AddDays(30),
                "127.0.0.1");

        dbContext.RefreshTokens.Add(
            customerRefreshToken);

        await dbContext.SaveChangesAsync();

        CustomerId =
            customer.Id;

        CustomerRefreshToken =
            generatedRefreshToken.Token;

        return customerAdministrator;
    }
    private async Task<AppUser> SeedAsync(
        ApplicationDbContext dbContext)
    {
        var brand =
            Brand.Create("adidas");

        brand.SetImage(
            "https://images.example.test/adidas.png",
            "integration-tests/brands/adidas");

        var shoeCategory =
            Category.Create(
                "İdman ayaqqabıları",
                ProductType.Shoe);

        var bagCategory =
            Category.Create(
                "Çantalar",
                ProductType.Bag);

        var shoeSize =
            Size.Create(
                "42",
                ProductType.Shoe,
                sortOrder: 42);

        var bagSize =
            Size.Create(
                "Standart",
                ProductType.Bag,
                sortOrder: 1);

        var yellow =
            Color.Create(
                "Sarı",
                "#FFD700");

        var black =
            Color.Create(
                "Qara",
                "#000000");

        var shoeProduct =
            Product.Create(
                name:
                    "Sarı adidas idman ayaqqabısı",
                description:
                    "Integration test üçün ayaqqabı.",
                productCode:
                    "BAGSIS-TEST-SHOE-001",
                model:
                    "Runner 01",
                price:
                    220m,
                discountPrice:
                    179.90m,
                productType:
                    ProductType.Shoe,
                categoryId:
                    shoeCategory.Id,
                brandId:
                    brand.Id,
                isFeatured:
                    true);

        var shoeVariant =
            shoeProduct.AddVariant(
                shoeSize.Id,
                yellow.Id,
                stockCount: 8);

        _ = shoeProduct.AddImage(
            "https://images.example.test/" +
            "yellow-adidas-shoe.png",
            "integration-tests/products/" +
            "yellow-adidas-shoe");

        ShoeProductId =
            shoeProduct.Id;

        ShoeVariantId =
            shoeVariant.Id;

        var bagProduct =
            Product.Create(
                name:
                    "Qara adidas çanta",
                description:
                    "Tərcümə fallback testi üçün çanta.",
                productCode:
                    "BAGSIS-TEST-BAG-001",
                model:
                    "Bag 01",
                price:
                    180m,
                discountPrice:
                    null,
                productType:
                    ProductType.Bag,
                categoryId:
                    bagCategory.Id,
                brandId:
                    brand.Id);

        _ = bagProduct.AddVariant(
            bagSize.Id,
            black.Id,
            stockCount: 4);

        _ = bagProduct.AddImage(
            "https://images.example.test/" +
            "black-adidas-bag.png",
            "integration-tests/products/" +
            "black-adidas-bag");

        BagProductId =
            bagProduct.Id;

        var outOfStockProduct =
            Product.Create(
                name:
                    "Stokda olmayan ayaqqabı",
                description:
                    null,
                productCode:
                    "BAGSIS-TEST-SHOE-OUT-001",
                model:
                    null,
                price:
                    150m,
                discountPrice:
                    null,
                productType:
                    ProductType.Shoe,
                categoryId:
                    shoeCategory.Id,
                brandId:
                    brand.Id);

        var outOfStockVariant =
            outOfStockProduct.AddVariant(
                shoeSize.Id,
                yellow.Id,
                stockCount: 0);

        OutOfStockProductId =
            outOfStockProduct.Id;

        OutOfStockVariantId =
            outOfStockVariant.Id;

        var customer =
            AppUser.Create(
                "Basket Test Customer",
                "basket.customer@bagsisbaku.test",
                DateTimeOffset.UtcNow);

        dbContext.Brands.Add(brand);

        dbContext.Categories.AddRange(
            shoeCategory,
            bagCategory);

        dbContext.Sizes.AddRange(
            shoeSize,
            bagSize);

        dbContext.Colors.AddRange(
            yellow,
            black);

        dbContext.Products.AddRange(
            shoeProduct,
            bagProduct,
            outOfStockProduct);

        dbContext.Users.Add(customer);

        dbContext.ProductTranslations.AddRange(
            ProductTranslation.Create(
                shoeProduct.Id,
                SupportedLanguage.Russian,
                "Жёлтые кроссовки adidas",
                "Обувь для интеграционного теста."),

            ProductTranslation.Create(
                shoeProduct.Id,
                SupportedLanguage.English,
                "Yellow adidas shoes",
                "Shoes for an integration test."));

        dbContext.CategoryTranslations.AddRange(
            CategoryTranslation.Create(
                shoeCategory.Id,
                SupportedLanguage.Russian,
                "Спортивная обувь"),

            CategoryTranslation.Create(
                shoeCategory.Id,
                SupportedLanguage.English,
                "Sports shoes"));

        dbContext.ColorTranslations.AddRange(
            ColorTranslation.Create(
                yellow.Id,
                SupportedLanguage.Russian,
                "Жёлтый"),

            ColorTranslation.Create(
                yellow.Id,
                SupportedLanguage.English,
                "Yellow"));

        dbContext.SizeTranslations.AddRange(
            SizeTranslation.Create(
                shoeSize.Id,
                SupportedLanguage.Russian,
                "42"),

            SizeTranslation.Create(
                shoeSize.Id,
                SupportedLanguage.English,
                "42"));

        await dbContext.SaveChangesAsync();

        return customer;
    }
}