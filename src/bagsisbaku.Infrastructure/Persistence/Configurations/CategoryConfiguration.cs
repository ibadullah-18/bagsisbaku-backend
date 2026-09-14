using bagsisbaku.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace bagsisbaku.Infrastructure.Persistence.Configurations;

internal sealed class CategoryConfiguration
    : IEntityTypeConfiguration<Category>
{
    public void Configure(
        EntityTypeBuilder<Category> builder)
    {
        builder.ToTable(
            "categories",
            "catalog");

        builder.ConfigureAuditable();

        builder.Property(category => category.Name)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(category => category.ProductType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(category => category.IconUrl)
            .HasMaxLength(2048);

        builder.Property(category => category.IconPublicId)
            .HasMaxLength(255);

        builder.Property(category => category.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.HasIndex(
                category => new
                {
                    category.ProductType,
                    category.Name
                })
            .IsUnique();

        builder.HasIndex(
            category => new
            {
                category.ProductType,
                category.IsActive
            });
    }
}
