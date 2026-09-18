using bagsisbaku.Domain.Localization;

namespace bagsisbaku.Application.Store;

public sealed record UpdateStoreSettingsCommand(
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
    string RowVersion);

public sealed record UpsertStoreSettingsTranslationCommand(
    SupportedLanguage Language,
    string Address,
    string WorkingHours,
    string DeliveryInformation,
    string ReturnPolicy,
    string AboutText,
    string RowVersion);