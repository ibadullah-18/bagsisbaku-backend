using bagsisbaku.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace bagsisbaku.Infrastructure.Persistence.Configurations;

internal sealed class SizeConfiguration
    : IEntityTypeConfiguration<Size>
{
    public void Configure(
        EntityTypeBuilder<Size> builder)
    {
        builder.ToTable(
            "sizes",
            "catalog");

        builder.ConfigureAuditable();

        builder.Property(size => size.Value)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(size => size.ProductType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(size => size.SortOrder)
            .IsRequired();

        builder.Property(size => size.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.HasIndex(
                size => new
                {
                    size.ProductType,
                    size.Value
                })
            .IsUnique();

        builder.HasIndex(
            size => new
            {
                size.ProductType,
                size.IsActive,
                size.SortOrder
            });
    }
}
