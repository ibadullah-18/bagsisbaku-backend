using bagsisbaku.Application.Abstractions.Persistence;
using bagsisbaku.Domain.Baskets;
using bagsisbaku.Domain.Catalog;
using bagsisbaku.Domain.Orders;
using bagsisbaku.Domain.Customers;
using bagsisbaku.Domain.Favorites;
using bagsisbaku.Infrastructure.Identity;
using bagsisbaku.Infrastructure.Persistence.Configurations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace bagsisbaku.Infrastructure.Persistence;

public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<AppUser, AppRole, Guid>(options),
      IUnitOfWork
{
    public DbSet<Product> Products => Set<Product>();

    public DbSet<ProductImage> ProductImages =>
        Set<ProductImage>();

    public DbSet<ProductVariant> ProductVariants =>
        Set<ProductVariant>();

    public DbSet<ProductTranslation> ProductTranslations =>
        Set<ProductTranslation>();

    public DbSet<Brand> Brands => Set<Brand>();

    public DbSet<Category> Categories =>
        Set<Category>();

    public DbSet<CategoryTranslation> CategoryTranslations =>
        Set<CategoryTranslation>();

    public DbSet<Size> Sizes => Set<Size>();

    public DbSet<SizeTranslation> SizeTranslations =>
        Set<SizeTranslation>();

    public DbSet<Color> Colors => Set<Color>();

    public DbSet<ColorTranslation> ColorTranslations =>
        Set<ColorTranslation>();

    public DbSet<CatalogDefault> CatalogDefaults =>
        Set<CatalogDefault>();

    public DbSet<Basket> Baskets =>
        Set<Basket>();

    public DbSet<BasketItem> BasketItems =>
        Set<BasketItem>();
    public DbSet<CustomerAddress> CustomerAddresses =>
        Set<CustomerAddress>();
    public DbSet<Favorite> Favorites =>
        Set<Favorite>();

    public DbSet<RefreshToken> RefreshTokens =>
        Set<RefreshToken>();

    public DbSet<Order> Orders =>
        Set<Order>();

    public DbSet<OrderItem> OrderItems =>
        Set<OrderItem>();

    public DbSet<OrderStatusHistory> OrderStatusHistories =>
        Set<OrderStatusHistory>();
    protected override void OnModelCreating(
        ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);

        builder.ConfigureIdentityModel();
    }
}
