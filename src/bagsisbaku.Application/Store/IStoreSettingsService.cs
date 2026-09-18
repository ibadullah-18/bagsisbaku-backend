using bagsisbaku.Application.Common.Results;
using bagsisbaku.Domain.Localization;

namespace bagsisbaku.Application.Store;

public interface IStoreSettingsService
{
    Task<Result<StoreSettingsModel>> GetAdminAsync(
        CancellationToken cancellationToken = default);

    Task<Result<LocalizedStoreSettingsModel>>
        GetPublicAsync(
            SupportedLanguage language,
            CancellationToken cancellationToken = default);

    Task<Result<StoreSettingsModel>> UpdateAsync(
        UpdateStoreSettingsCommand command,
        CancellationToken cancellationToken = default);

    Task<Result<StoreSettingsModel>>
        UpsertTranslationAsync(
            UpsertStoreSettingsTranslationCommand command,
            CancellationToken cancellationToken = default);

    Task<Result<
        bagsisbaku.Application.Abstractions.Storage.StoredImage>>
        UploadLogoAsync(
            bagsisbaku.Application.Media.ImageUploadCommand command,
            CancellationToken cancellationToken = default);

    Task<Result> RemoveLogoAsync(
        CancellationToken cancellationToken = default);
}