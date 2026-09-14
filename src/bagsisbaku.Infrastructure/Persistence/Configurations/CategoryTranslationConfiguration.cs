using bagsisbaku.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace bagsisbaku.Infrastructure.Persistence.Configurations;

internal sealed class CategoryTranslationConfiguration
    : IEntityTypeConfiguration<CategoryTranslation>
{
    public void Configure(
        EntityTypeBuilder<CategoryTranslation> builder)
    {
        builder.ToTable(
            "category_translations",
            "catalog");

        builder.ConfigureAuditable();

        builder.Property(translation => translation.Language)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(translation => translation.Name)
            .HasMaxLength(120)
            .IsRequired();

        builder.HasIndex(
                translation => new
                {
                    translation.CategoryId,
                    translation.Language
                })
            .IsUnique();

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(
                translation => translation.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
