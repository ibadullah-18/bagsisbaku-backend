using bagsisbaku.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace bagsisbaku.Infrastructure.Persistence.Configurations;

internal sealed class ProductTranslationConfiguration
    : IEntityTypeConfiguration<ProductTranslation>
{
    public void Configure(
        EntityTypeBuilder<ProductTranslation> builder)
    {
        builder.ToTable(
            "product_translations",
            "catalog");

        builder.ConfigureAuditable();

        builder.Property(translation => translation.Language)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(translation => translation.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(translation => translation.Description)
            .HasMaxLength(4000);

        builder.HasIndex(
                translation => new
                {
                    translation.ProductId,
                    translation.Language
                })
            .IsUnique();

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(
                translation => translation.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
