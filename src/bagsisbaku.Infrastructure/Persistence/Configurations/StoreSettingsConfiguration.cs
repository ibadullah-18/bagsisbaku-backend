using bagsisbaku.Domain.Store;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace bagsisbaku.Infrastructure.Persistence.Configurations;

internal sealed class StoreSettingsConfiguration
    : IEntityTypeConfiguration<StoreSettings>
{
    public void Configure(
        EntityTypeBuilder<StoreSettings> builder)
    {
        builder.ToTable(
            "store_settings",
            "store");

        builder.HasKey(settings =>
            settings.Id);

        builder.Property(settings =>
                settings.Id)
            .ValueGeneratedNever();

        builder.Property(settings =>
                settings.StoreName)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(settings =>
                settings.PrimaryPhone)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(settings =>
                settings.WhatsAppPhone)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(settings =>
                settings.Email)
            .HasMaxLength(256);

        builder.Property(settings =>
                settings.InstagramUrl)
            .HasMaxLength(2048);

        builder.Property(settings =>
                settings.TikTokUrl)
            .HasMaxLength(2048);

        builder.Property(settings =>
                settings.Address)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(settings =>
                settings.WorkingHours)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(settings =>
                settings.DeliveryInformation)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(settings =>
                settings.ReturnPolicy)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(settings =>
                settings.AboutText)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(settings =>
                settings.Latitude)
            .HasPrecision(9, 6)
            .IsRequired();

        builder.Property(settings =>
                settings.Longitude)
            .HasPrecision(9, 6)
            .IsRequired();

        builder.Property(settings =>
                settings.MapUrl)
            .HasMaxLength(2048);

        builder.Property(settings =>
                settings.LogoUrl)
            .HasMaxLength(2048);

        builder.Property(settings =>
                settings.LogoPublicId)
            .HasMaxLength(255);

        builder.Property(settings =>
                settings.CreatedAtUtc)
            .HasPrecision(0)
            .IsRequired();

        builder.Property(settings =>
                settings.UpdatedAtUtc)
            .HasPrecision(0);

        builder.Property(settings =>
                settings.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasIndex(settings =>
                settings.StoreName)
            .IsUnique()
            .HasDatabaseName(
                "ux_store_settings_store_name");
    }
}