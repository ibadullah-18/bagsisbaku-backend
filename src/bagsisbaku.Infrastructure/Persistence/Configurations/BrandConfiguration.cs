using bagsisbaku.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace bagsisbaku.Infrastructure.Persistence.Configurations;

internal sealed class BrandConfiguration
    : IEntityTypeConfiguration<Brand>
{
    public void Configure(
        EntityTypeBuilder<Brand> builder)
    {
        builder.ToTable(
            "brands",
            "catalog");

        builder.ConfigureAuditable();

        builder.Property(brand => brand.Name)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(brand => brand.ImageUrl)
            .HasMaxLength(2048);

        builder.Property(brand => brand.ImagePublicId)
            .HasMaxLength(255);

        builder.Property(brand => brand.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.HasIndex(brand => brand.Name)
            .IsUnique();

        builder.HasIndex(brand => brand.IsActive);
    }
}
