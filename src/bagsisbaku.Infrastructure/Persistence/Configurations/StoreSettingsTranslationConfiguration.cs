using bagsisbaku.Domain.Store;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace bagsisbaku.Infrastructure.Persistence.Configurations;

internal sealed class StoreSettingsTranslationConfiguration
    : IEntityTypeConfiguration<StoreSettingsTranslation>
{
    public void Configure(
        EntityTypeBuilder<StoreSettingsTranslation> builder)
    {
        builder.ToTable(
            "store_setting_translations",
            "store");

        builder.ConfigureAuditable();

        builder.Property(translation =>
                translation.Language)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(translation =>
                translation.Address)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(translation =>
                translation.WorkingHours)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(translation =>
                translation.DeliveryInformation)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(translation =>
                translation.ReturnPolicy)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(translation =>
                translation.AboutText)
            .HasMaxLength(4000)
            .IsRequired();

        builder.HasIndex(
                translation => new
                {
                    translation.StoreSettingsId,
                    translation.Language
                })
            .IsUnique()
            .HasDatabaseName(
                "ux_store_setting_translations_settings_language");

        builder.HasOne<StoreSettings>()
            .WithMany()
            .HasForeignKey(
                translation =>
                    translation.StoreSettingsId)
            .OnDelete(
                DeleteBehavior.Cascade);
    }
}