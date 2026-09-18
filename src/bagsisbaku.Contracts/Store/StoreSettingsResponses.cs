namespace bagsisbaku.Contracts.Store;

public sealed record StoreSettingsResponse(
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
    IReadOnlyCollection<StoreSettingsTranslationResponse>
        Translations);

public sealed record StoreSettingsTranslationResponse(
    Guid Id,
    string Language,
    string Address,
    string WorkingHours,
    string DeliveryInformation,
    string ReturnPolicy,
    string AboutText);

public sealed record PublicStoreSettingsResponse(
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