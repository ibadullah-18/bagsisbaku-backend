using bagsisbaku.Domain.Home;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace bagsisbaku.Infrastructure.Persistence.Configurations;

internal sealed class HomeSectionTranslationConfiguration
    : IEntityTypeConfiguration<HomeSectionTranslation>
{
    public void Configure(
        EntityTypeBuilder<HomeSectionTranslation> builder)
    {
        builder.ToTable(
            "home_section_translations",
            "home");

        builder.ConfigureAuditable();

        builder.Property(translation => translation.Language)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(translation => translation.Title)
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(translation => translation.Subtitle)
            .HasMaxLength(500);

        builder.HasIndex(translation => new
        {
            translation.HomeSectionId,
            translation.Language
        }).IsUnique();

        builder.HasOne<HomeSection>()
            .WithMany()
            .HasForeignKey(
                translation => translation.HomeSectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}