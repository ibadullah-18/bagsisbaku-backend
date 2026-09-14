using bagsisbaku.Application.Abstractions.Storage;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Logging;

namespace bagsisbaku.Infrastructure.Storage;

internal sealed partial class CloudinaryImageStorage
    : IImageStorage
{
    private readonly Cloudinary? _cloudinary;

    private readonly ILogger<CloudinaryImageStorage>
        _logger;

    public CloudinaryImageStorage(
        CloudinarySettings settings,
        ILogger<CloudinaryImageStorage> logger)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;

        if (!settings.IsConfigured)
        {
            LogConfigurationMissing();
            return;
        }

        var account = new Account(
            settings.CloudName,
            settings.ApiKey,
            settings.ApiSecret);

        _cloudinary = new Cloudinary(account);
        _cloudinary.Api.Secure = true;
    }

    public async Task<StoredImage> UploadAsync(
        Stream content,
        string fileName,
        ImageFolder folder,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        try
        {
            var cloudinary = GetCloudinary();

            var uploadParameters = new ImageUploadParams
            {
                File = new FileDescription(
                    fileName,
                    content),

                Folder = GetFolder(folder),
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false
            };

            var result = await cloudinary.UploadAsync(
                uploadParameters,
                cancellationToken);

            if (result.Error is not null)
            {
                throw new ImageStorageException(
                    result.Error.Message);
            }

            var imageUrl =
                result.SecureUrl?.AbsoluteUri;

            if (string.IsNullOrWhiteSpace(imageUrl) ||
                string.IsNullOrWhiteSpace(result.PublicId))
            {
                throw new ImageStorageException(
                    "Cloudinary düzgün upload nəticəsi qaytarmadı.");
            }

            return new StoredImage(
                imageUrl,
                result.PublicId,
                result.Width,
                result.Height,
                result.Format ?? string.Empty,
                result.Bytes);
        }
        catch (Exception exception)
        {
            LogUploadFailure(
                fileName,
                folder,
                exception);

            if (exception is ImageStorageException)
            {
                throw;
            }

            throw new ImageStorageException(
                "Cloudinary upload əməliyyatı uğursuz oldu.",
                exception);
        }
    }

    public async Task DeleteAsync(
        string publicId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(publicId))
        {
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var cloudinary = GetCloudinary();

            var deletionParameters =
                new DeletionParams(publicId)
                {
                    ResourceType = ResourceType.Image,
                    Invalidate = true
                };

            var result = await cloudinary.DestroyAsync(
                deletionParameters);

            if (result.Error is not null)
            {
                throw new ImageStorageException(
                    result.Error.Message);
            }

            if (result.Result is not "ok" and not "not found")
            {
                throw new ImageStorageException(
                    $"Cloudinary silmə nəticəsi: {result.Result}");
            }
        }
        catch (Exception exception)
        {
            LogDeleteFailure(
                publicId,
                exception);

            if (exception is ImageStorageException)
            {
                throw;
            }

            throw new ImageStorageException(
                "Cloudinary şəkil silmə əməliyyatı uğursuz oldu.",
                exception);
        }
    }

    private Cloudinary GetCloudinary()
    {
        return _cloudinary ??
            throw new ImageStorageException(
                "Cloudinary konfiqurasiyası tapılmadı.");
    }

    private static string GetFolder(
        ImageFolder folder)
    {
        return folder switch
        {
            ImageFolder.Products =>
                "bagsisbaku/products",

            ImageFolder.Brands =>
                "bagsisbaku/brands",

            ImageFolder.Categories =>
                "bagsisbaku/categories",

            ImageFolder.Profiles =>
                "bagsisbaku/profiles",

            ImageFolder.Content =>
                "bagsisbaku/content",

            _ => throw new ImageStorageException(
                "Cloudinary qovluğu düzgün deyil.")
        };
    }

    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Error,
        Message =
            "Cloudinary upload uğursuz oldu. " +
            "FileName: {FileName}; Folder: {Folder}.")]
    private partial void LogUploadFailure(
        string fileName,
        ImageFolder folder,
        Exception exception);

    [LoggerMessage(
        EventId = 2002,
        Level = LogLevel.Error,
        Message =
            "Cloudinary silmə əməliyyatı uğursuz oldu. " +
            "PublicId: {PublicId}.")]
    private partial void LogDeleteFailure(
        string publicId,
        Exception exception);

    [LoggerMessage(
        EventId = 2003,
        Level = LogLevel.Error,
        Message =
            "Cloudinary konfiqurasiyası mövcud deyil.")]
    private partial void LogConfigurationMissing();
}
