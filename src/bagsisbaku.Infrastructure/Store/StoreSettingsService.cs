using bagsisbaku.Application.Abstractions.Storage;
using bagsisbaku.Application.Media;
using bagsisbaku.Application.Common.Results;
using bagsisbaku.Application.Store;
using bagsisbaku.Domain.Localization;
using bagsisbaku.Domain.Store;
using bagsisbaku.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace bagsisbaku.Infrastructure.Store;

internal sealed class StoreSettingsService(
    ApplicationDbContext dbContext,
    IImageStorage imageStorage)
    : IStoreSettingsService
{
    public async Task<Result<StoreSettingsModel>>
        GetAdminAsync(
            CancellationToken cancellationToken = default)
    {
        var settings =
            await dbContext.StoreSettings
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    cancellationToken);

        if (settings is null)
        {
            return Result.Failure<StoreSettingsModel>(
                StoreSettingsErrors.NotFound);
        }

        var translations =
            await dbContext.StoreSettingsTranslations
                .AsNoTracking()
                .Where(
                    translation =>
                        translation.StoreSettingsId ==
                        settings.Id)
                .OrderBy(
                    translation =>
                        translation.Language)
                .ToArrayAsync(
                    cancellationToken);

        return Result.Success(
            ToAdminModel(
                settings,
                translations));
    }

    public async Task<Result<LocalizedStoreSettingsModel>>
        GetPublicAsync(
            SupportedLanguage language,
            CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(language))
        {
            return Result.Failure<
                LocalizedStoreSettingsModel>(
                StoreSettingsErrors.UnsupportedLanguage);
        }

        var settings =
            await dbContext.StoreSettings
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    cancellationToken);

        if (settings is null)
        {
            return Result.Failure<
                LocalizedStoreSettingsModel>(
                StoreSettingsErrors.NotFound);
        }

        StoreSettingsTranslation? translation = null;

        if (language != SupportedLanguage.Azerbaijani)
        {
            translation =
                await dbContext.StoreSettingsTranslations
                    .AsNoTracking()
                    .SingleOrDefaultAsync(
                        currentTranslation =>
                            currentTranslation.StoreSettingsId ==
                            settings.Id &&
                            currentTranslation.Language ==
                            language,
                        cancellationToken);
        }

        return Result.Success(
            ToPublicModel(
                settings,
                translation,
                language));
    }

    public async Task<Result<StoreSettingsModel>>
        UpdateAsync(
            UpdateStoreSettingsCommand command,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            command);

        if (!TryDecodeRowVersion(
            command.RowVersion,
            out var expectedRowVersion))
        {
            return Result.Failure<StoreSettingsModel>(
                StoreSettingsErrors.InvalidRowVersion);
        }

        var settings =
            await dbContext.StoreSettings
                .SingleOrDefaultAsync(
                    cancellationToken);

        if (settings is null)
        {
            return Result.Failure<StoreSettingsModel>(
                StoreSettingsErrors.NotFound);
        }

        dbContext
            .Entry(settings)
            .Property(
                currentSettings =>
                    currentSettings.RowVersion)
            .OriginalValue =
                expectedRowVersion;

        settings.Update(
            command.PrimaryPhone,
            command.WhatsAppPhone,
            command.Email,
            command.InstagramUrl,
            command.TikTokUrl,
            command.Address,
            command.WorkingHours,
            command.DeliveryInformation,
            command.ReturnPolicy,
            command.AboutText,
            command.Latitude,
            command.Longitude,
            command.MapUrl);

        try
        {
            await dbContext.SaveChangesAsync(
                cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            dbContext.ChangeTracker.Clear();

            return Result.Failure<StoreSettingsModel>(
                StoreSettingsErrors.ConcurrencyConflict);
        }

        return await GetAdminAsync(
            cancellationToken);
    }

    public async Task<Result<StoreSettingsModel>>
        UpsertTranslationAsync(
            UpsertStoreSettingsTranslationCommand command,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            command);

        if (!Enum.IsDefined(command.Language))
        {
            return Result.Failure<StoreSettingsModel>(
                StoreSettingsErrors.UnsupportedLanguage);
        }

        if (
            command.Language ==
            SupportedLanguage.Azerbaijani
        )
        {
            return Result.Failure<StoreSettingsModel>(
                StoreSettingsErrors
                    .AzerbaijaniTranslationNotAllowed);
        }

        if (!TryDecodeRowVersion(
            command.RowVersion,
            out var expectedRowVersion))
        {
            return Result.Failure<StoreSettingsModel>(
                StoreSettingsErrors.InvalidRowVersion);
        }

        var settings =
            await dbContext.StoreSettings
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    cancellationToken);

        if (settings is null)
        {
            return Result.Failure<StoreSettingsModel>(
                StoreSettingsErrors.NotFound);
        }

        if (
            !settings.RowVersion.AsSpan()
                .SequenceEqual(expectedRowVersion)
        )
        {
            return Result.Failure<StoreSettingsModel>(
                StoreSettingsErrors.ConcurrencyConflict);
        }

        var translation =
            await dbContext.StoreSettingsTranslations
                .SingleOrDefaultAsync(
                    currentTranslation =>
                        currentTranslation.StoreSettingsId ==
                        settings.Id &&
                        currentTranslation.Language ==
                        command.Language,
                    cancellationToken);

        if (translation is null)
        {
            translation =
                StoreSettingsTranslation.Create(
                    settings.Id,
                    command.Language,
                    command.Address,
                    command.WorkingHours,
                    command.DeliveryInformation,
                    command.ReturnPolicy,
                    command.AboutText);

            dbContext.StoreSettingsTranslations.Add(
                translation);
        }
        else
        {
            translation.Update(
                command.Address,
                command.WorkingHours,
                command.DeliveryInformation,
                command.ReturnPolicy,
                command.AboutText);
        }

        try
        {
            await dbContext.SaveChangesAsync(
                cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            dbContext.ChangeTracker.Clear();

            return Result.Failure<StoreSettingsModel>(
                StoreSettingsErrors.ConcurrencyConflict);
        }

        return await GetAdminAsync(
            cancellationToken);
    }

    public async Task<Result<StoredImage>>
        UploadLogoAsync(
            ImageUploadCommand command,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            command);

        var settings =
            await dbContext.StoreSettings
                .SingleOrDefaultAsync(
                    cancellationToken);

        if (settings is null)
        {
            return Result.Failure<StoredImage>(
                StoreSettingsErrors.NotFound);
        }

        var validationError =
            await ImageFileValidator.ValidateAsync(
                command,
                cancellationToken);

        if (validationError is not null)
        {
            return Result.Failure<StoredImage>(
                validationError);
        }

        StoredImage? uploadedImage = null;

        try
        {
            uploadedImage =
                await imageStorage.UploadAsync(
                    command.Content,
                    command.FileName,
                    ImageFolder.Content,
                    cancellationToken);

            var previousPublicId =
                settings.LogoPublicId;

            settings.SetLogo(
                uploadedImage.Url,
                uploadedImage.PublicId);

            await dbContext.SaveChangesAsync(
                cancellationToken);

            if (
                !string.IsNullOrWhiteSpace(
                    previousPublicId) &&
                !string.Equals(
                    previousPublicId,
                    uploadedImage.PublicId,
                    StringComparison.Ordinal)
            )
            {
                await TryDeleteImageAsync(
                    previousPublicId);
            }

            return Result.Success(
                uploadedImage);
        }
        catch (DbUpdateConcurrencyException)
        {
            dbContext.ChangeTracker.Clear();

            if (uploadedImage is not null)
            {
                await TryDeleteImageAsync(
                    uploadedImage.PublicId);
            }

            return Result.Failure<StoredImage>(
                StoreSettingsErrors
                    .ConcurrencyConflict);
        }
        catch (ImageStorageException)
        {
            return Result.Failure<StoredImage>(
                MediaErrors.StorageFailure);
        }
        catch
        {
            if (uploadedImage is not null)
            {
                await TryDeleteImageAsync(
                    uploadedImage.PublicId);
            }

            throw;
        }
    }

    public async Task<Result> RemoveLogoAsync(
        CancellationToken cancellationToken = default)
    {
        var settings =
            await dbContext.StoreSettings
                .SingleOrDefaultAsync(
                    cancellationToken);

        if (settings is null)
        {
            return Result.Failure(
                StoreSettingsErrors.NotFound);
        }

        var previousPublicId =
            settings.LogoPublicId;

        settings.RemoveLogo();

        try
        {
            await dbContext.SaveChangesAsync(
                cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            dbContext.ChangeTracker.Clear();

            return Result.Failure(
                StoreSettingsErrors
                    .ConcurrencyConflict);
        }

        if (!string.IsNullOrWhiteSpace(
            previousPublicId))
        {
            await TryDeleteImageAsync(
                previousPublicId);
        }

        return Result.Success();
    }

    private async Task TryDeleteImageAsync(
        string publicId)
    {
        try
        {
            await imageStorage.DeleteAsync(
                publicId,
                CancellationToken.None);
        }
        catch (ImageStorageException)
        {
            // Database əməliyyatı uğurludur.
            // Cloudinary orphan şəkli sonradan təmizlənə bilər.
        }
    }
    private static StoreSettingsModel ToAdminModel(
        StoreSettings settings,
        IReadOnlyCollection<StoreSettingsTranslation>
            translations)
    {
        var translationModels =
            translations
                .Select(
                    translation =>
                        new StoreSettingsTranslationModel(
                            translation.Id,
                            translation.Language,
                            GetLanguageCode(
                                translation.Language),
                            translation.Address,
                            translation.WorkingHours,
                            translation
                                .DeliveryInformation,
                            translation.ReturnPolicy,
                            translation.AboutText))
                .ToArray();

        return new StoreSettingsModel(
            settings.Id,
            settings.StoreName,
            settings.PrimaryPhone,
            settings.WhatsAppPhone,
            settings.Email,
            settings.InstagramUrl,
            settings.TikTokUrl,
            settings.Address,
            settings.WorkingHours,
            settings.DeliveryInformation,
            settings.ReturnPolicy,
            settings.AboutText,
            settings.Latitude,
            settings.Longitude,
            settings.MapUrl,
            settings.LogoUrl,
            Convert.ToBase64String(
                settings.RowVersion),
            translationModels);
    }

    private static LocalizedStoreSettingsModel
        ToPublicModel(
            StoreSettings settings,
            StoreSettingsTranslation? translation,
            SupportedLanguage requestedLanguage)
    {
        var effectiveLanguage =
            translation is null
                ? SupportedLanguage.Azerbaijani
                : requestedLanguage;

        return new LocalizedStoreSettingsModel(
            settings.Id,
            GetLanguageCode(
                effectiveLanguage),
            settings.StoreName,
            settings.PrimaryPhone,
            settings.WhatsAppPhone,
            settings.Email,
            settings.InstagramUrl,
            settings.TikTokUrl,
            translation?.Address ??
                settings.Address,
            translation?.WorkingHours ??
                settings.WorkingHours,
            translation?.DeliveryInformation ??
                settings.DeliveryInformation,
            translation?.ReturnPolicy ??
                settings.ReturnPolicy,
            translation?.AboutText ??
                settings.AboutText,
            settings.Latitude,
            settings.Longitude,
            settings.MapUrl,
            settings.LogoUrl);
    }

    private static bool TryDecodeRowVersion(
        string rowVersion,
        out byte[] result)
    {
        result = [];

        if (string.IsNullOrWhiteSpace(rowVersion))
        {
            return false;
        }

        try
        {
            result =
                Convert.FromBase64String(
                    rowVersion.Trim());

            return result.Length > 0;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static string GetLanguageCode(
        SupportedLanguage language)
    {
        return language switch
        {
            SupportedLanguage.Azerbaijani => "az",
            SupportedLanguage.Russian => "ru",
            SupportedLanguage.English => "en",
            _ => "az"
        };
    }
}