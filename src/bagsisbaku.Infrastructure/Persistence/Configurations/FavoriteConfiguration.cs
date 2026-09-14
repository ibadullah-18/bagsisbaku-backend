using bagsisbaku.Domain.Catalog;
using bagsisbaku.Domain.Favorites;
using bagsisbaku.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace bagsisbaku.Infrastructure.Persistence.Configurations;

internal sealed class FavoriteConfiguration
    : IEntityTypeConfiguration<Favorite>
{
    public void Configure(
        EntityTypeBuilder<Favorite> builder)
    {
        builder.ToTable(
            "favorites",
            "customers");

        builder.HasKey(favorite =>
            favorite.Id);

        builder.Property(favorite =>
                favorite.Id)
            .ValueGeneratedNever();

        builder.Property(favorite =>
                favorite.UserId)
            .IsRequired();

        builder.Property(favorite =>
                favorite.ProductId)
            .IsRequired();

        builder.Property(favorite =>
                favorite.CreatedAtUtc)
            .IsRequired();

        builder.Property(favorite =>
                favorite.UpdatedAtUtc);

        builder.HasIndex(favorite =>
                favorite.UserId)
            .HasDatabaseName(
                "ix_favorites_user_id");

        builder.HasIndex(favorite =>
                favorite.ProductId)
            .HasDatabaseName(
                "ix_favorites_product_id");

        builder.HasIndex(favorite =>
                new
                {
                    favorite.UserId,
                    favorite.ProductId
                })
            .IsUnique()
            .HasDatabaseName(
                "ux_favorites_user_product");

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(favorite =>
                favorite.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(favorite =>
                favorite.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}