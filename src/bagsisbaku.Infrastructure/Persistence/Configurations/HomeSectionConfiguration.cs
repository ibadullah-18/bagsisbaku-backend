using bagsisbaku.Domain.Home;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace bagsisbaku.Infrastructure.Persistence.Configurations;

internal sealed class HomeSectionConfiguration
    : IEntityTypeConfiguration<HomeSection>
{
    public void Configure(
        EntityTypeBuilder<HomeSection> builder)
    {
        builder.ToTable("home_sections", "home");

        builder.ConfigureAuditable();

        builder.Property(section => section.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(section => section.Title)
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(section => section.Subtitle)
            .HasMaxLength(500);

        builder.Property(section => section.TargetUrl)
            .HasMaxLength(2048);

        builder.Property(section => section.ImageUrl)
            .HasMaxLength(2048);

        builder.Property(section => section.ImagePublicId)
            .HasMaxLength(255);

        builder.Property(section => section.SortOrder)
            .IsRequired();

        builder.Property(section => section.IsActive)
            .IsRequired();

        builder.HasIndex(section => new
        {
            section.Type,
            section.IsActive,
            section.SortOrder
        });
    }
}