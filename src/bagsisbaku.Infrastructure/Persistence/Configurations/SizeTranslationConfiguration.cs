using bagsisbaku.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace bagsisbaku.Infrastructure.Persistence.Configurations;

internal sealed class SizeTranslationConfiguration
    : IEntityTypeConfiguration<SizeTranslation>
{
    public void Configure(
        EntityTypeBuilder<SizeTranslation> builder)
    {
        builder.ToTable(
            "size_translations",
            "catalog");

        builder.ConfigureAuditable();

        builder.Property(translation => translation.Language)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(translation => translation.Value)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(
                translation => new
                {
                    translation.SizeId,
                    translation.Language
                })
            .IsUnique();

        builder.HasOne<Size>()
            .WithMany()
            .HasForeignKey(
                translation => translation.SizeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
