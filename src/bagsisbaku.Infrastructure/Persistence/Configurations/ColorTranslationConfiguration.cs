using bagsisbaku.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace bagsisbaku.Infrastructure.Persistence.Configurations;

internal sealed class ColorTranslationConfiguration
    : IEntityTypeConfiguration<ColorTranslation>
{
    public void Configure(
        EntityTypeBuilder<ColorTranslation> builder)
    {
        builder.ToTable(
            "color_translations",
            "catalog");

        builder.ConfigureAuditable();

        builder.Property(translation => translation.Language)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(translation => translation.Name)
            .HasMaxLength(80)
            .IsRequired();

        builder.HasIndex(
                translation => new
                {
                    translation.ColorId,
                    translation.Language
                })
            .IsUnique();

        builder.HasOne<Color>()
            .WithMany()
            .HasForeignKey(
                translation => translation.ColorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
