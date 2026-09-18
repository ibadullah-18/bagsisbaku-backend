using bagsisbaku.Domain.Localization;

namespace bagsisbaku.Application.Store;

public sealed record StoreSettingsModel(
    Guid Id,
    string StoreName,
    string PrimaryPhone,
    string WhatsAppPhone,
    string? Email,
    string? InstagramUrl,
    string? TikTokUrl,
    string Address,
    string WorkingHours,
    string DeliveryInformation,
    string ReturnPolicy,
    string AboutText,
    decimal Latitude,
    decimal Longitude,
    string? MapUrl,
    string? LogoUrl,
    string RowVersion,
    IReadOnlyCollection<StoreSettingsTranslationModel>
        Translations);

public sealed record StoreSettingsTranslationModel(
    Guid Id,
    SupportedLanguage Language,
    string LanguageCode,
    string Address,
    string WorkingHours,
    string DeliveryInformation,
    string ReturnPolicy,
    string AboutText);

public sealed record LocalizedStoreSettingsModel(
    Guid Id,
    string Language,
    string StoreName,
    string PrimaryPhone,
    string WhatsAppPhone,
    string? Email,
    string? InstagramUrl,
    string? TikTokUrl,
    string Address,
    string WorkingHours,
    string DeliveryInformation,
    string ReturnPolicy,
    string AboutText,
    decimal Latitude,
    decimal Longitude,
    string? MapUrl,
    string? LogoUrl);