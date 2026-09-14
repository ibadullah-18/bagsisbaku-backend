using bagsisbaku.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace bagsisbaku.Infrastructure.Persistence.Configurations;

internal sealed class CatalogDefaultConfiguration
    : IEntityTypeConfiguration<CatalogDefault>
{
    public void Configure(
        EntityTypeBuilder<CatalogDefault> builder)
    {
        builder.ToTable(
            "catalog_defaults",
            "catalog");

        builder.ConfigureAuditable();

        builder.Property(
                catalogDefault => catalogDefault.ProductType)
            .HasConversion<int>()
            .IsRequired();

        builder.HasIndex(
                catalogDefault => catalogDefault.ProductType)
            .IsUnique();

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(
                catalogDefault =>
                    catalogDefault.DefaultCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Size>()
            .WithMany()
            .HasForeignKey(
                catalogDefault =>
                    catalogDefault.DefaultSizeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
