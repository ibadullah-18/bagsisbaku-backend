using bagsisbaku.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace bagsisbaku.Infrastructure.Persistence.Configurations;

internal sealed class ColorConfiguration
    : IEntityTypeConfiguration<Color>
{
    public void Configure(
        EntityTypeBuilder<Color> builder)
    {
        builder.ToTable(
            "colors",
            "catalog");

        builder.ConfigureAuditable();

        builder.Property(color => color.Name)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(color => color.HexCode)
            .HasMaxLength(7);

        builder.Property(color => color.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.HasIndex(color => color.Name)
            .IsUnique();

        builder.HasIndex(color => color.IsActive);
    }
}
