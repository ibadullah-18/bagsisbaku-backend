using bagsisbaku.Domain.Store;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace bagsisbaku.Infrastructure.Persistence.Configurations;

internal sealed class StoreSettingsSeedConfiguration
    : IEntityTypeConfiguration<StoreSettings>
{
    public static readonly Guid StoreSettingsId =
        new("b4616c76-8748-447a-b154-e4d3fd5f8e72");

    public void Configure(
        EntityTypeBuilder<StoreSettings> builder)
    {
        builder.HasData(
            new
            {
                Id = StoreSettingsId,
                StoreName = StoreSettings.BrandName,
                PrimaryPhone = "+994519723718",
                WhatsAppPhone = "+994519723718",
                Email = "bagsisbaku2026@gmail.com",
                InstagramUrl =
                    "https://www.instagram.com/bagsisbaku",
                TikTokUrl = (string?)null,
                Address = "Bakı, Azərbaycan",
                WorkingHours = "Hər gün 10:00–21:00",
                DeliveryInformation =
                    "Çatdırılma məlumatları sifariş zamanı dəqiqləşdirilir.",
                ReturnPolicy =
                    "Qaytarma və dəyişdirmə şərtləri mağaza ilə razılaşdırılır.",
                AboutText =
                    "bagsisbaku çanta və ayaqqabı mağazasıdır.",
                Latitude = 40.376504m,
                Longitude = 49.841709m,
                MapUrl = (string?)null,
                LogoUrl = (string?)null,
                LogoPublicId = (string?)null,
                CreatedAtUtc =
                    new DateTimeOffset(
                        2026,
                        9,
                        18,
                        0,
                        0,
                        0,
                        TimeSpan.Zero),
                UpdatedAtUtc =
                    (DateTimeOffset?)null
            });
    }
}