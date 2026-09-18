using bagsisbaku.Application.Store;
using bagsisbaku.Contracts.Store;

namespace bagsisbaku.Api.Models.Store;

internal static class StoreSettingsResponseMapper
{
    public static StoreSettingsResponse ToResponse(
        StoreSettingsModel model)
    {
        return new StoreSettingsResponse(
            model.Id,
            model.StoreName,
            model.PrimaryPhone,
            model.WhatsAppPhone,
            model.Email,
            model.InstagramUrl,
            model.TikTokUrl,
            model.Address,
            model.WorkingHours,
            model.DeliveryInformation,
            model.ReturnPolicy,
            model.AboutText,
            model.Latitude,
            model.Longitude,
            model.MapUrl,
            model.LogoUrl,
            model.RowVersion,
            model.Translations
                .Select(ToTranslationResponse)
                .ToArray());
    }

    public static PublicStoreSettingsResponse
        ToPublicResponse(
            LocalizedStoreSettingsModel model)
    {
        return new PublicStoreSettingsResponse(
            model.Id,
            model.Language,
            model.StoreName,
            model.PrimaryPhone,
            model.WhatsAppPhone,
            model.Email,
            model.InstagramUrl,
            model.TikTokUrl,
            model.Address,
            model.WorkingHours,
            model.DeliveryInformation,
            model.ReturnPolicy,
            model.AboutText,
            model.Latitude,
            model.Longitude,
            model.MapUrl,
            model.LogoUrl);
    }

    private static StoreSettingsTranslationResponse
        ToTranslationResponse(
            StoreSettingsTranslationModel model)
    {
        return new StoreSettingsTranslationResponse(
            model.Id,
            model.LanguageCode,
            model.Address,
            model.WorkingHours,
            model.DeliveryInformation,
            model.ReturnPolicy,
            model.AboutText);
    }
}