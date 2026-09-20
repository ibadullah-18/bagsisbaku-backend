using bagsisbaku.Application.Abstractions.Storage;
using bagsisbaku.Application.Common.Results;
using bagsisbaku.Application.Home;
using bagsisbaku.Application.Media;
using bagsisbaku.Domain.Home;
using bagsisbaku.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace bagsisbaku.Infrastructure.Home;

public sealed class HomeSectionPublicationService(
    ApplicationDbContext dbContext,
    IImageStorage imageStorage,
    ILogger<HomeSectionPublicationService> logger)
    : IHomeSectionPublicationService
{
    private static readonly Error SectionNotFound =
        Error.NotFound(
            "home.section-not-found",
            "Ana səhifə bloku tapılmadı.");

    public async Task<Result<HomeSectionPublicationModel>>
        UploadImageAsync(
            Guid sectionId,
            ImageUploadCommand command,
            CancellationToken cancellationToken = default)
    {
        var section =
            await dbContext.HomeSections
                .SingleOrDefaultAsync(
                    item => item.Id == sectionId,
                    cancellationToken);

        if (section is null)
        {
            return Result.Failure<HomeSectionPublicationModel>(
                SectionNotFound);
        }

        var validationError =
            await ImageFileValidator.ValidateAsync(
                command,
                cancellationToken);

        if (validationError is not null)
        {
            return Result.Failure<HomeSectionPublicationModel>(
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
                section.ImagePublicId;

            section.SetImage(
                uploadedImage.Url,
                uploadedImage.PublicId);

            await dbContext.SaveChangesAsync(
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(
                    previousPublicId) &&
                previousPublicId != uploadedImage.PublicId)
            {
                await TryDeleteAsync(
                    previousPublicId);
            }

            return Result.Success(ToModel(section));
        }
        catch (ImageStorageException)
        {
            if (uploadedImage is not null)
            {
                await TryDeleteAsync(
                    uploadedImage.PublicId);
            }

            return Result.Failure<HomeSectionPublicationModel>(
                MediaErrors.StorageFailure);
        }
        catch
        {
            if (uploadedImage is not null)
            {
                await TryDeleteAsync(
                    uploadedImage.PublicId);
            }

            throw;
        }
    }

    public async Task<Result<HomeSectionPublicationModel>>
        RemoveImageAsync(
            Guid sectionId,
            CancellationToken cancellationToken = default)
    {
        var section =
            await dbContext.HomeSections
                .SingleOrDefaultAsync(
                    item => item.Id == sectionId,
                    cancellationToken);

        if (section is null)
        {
            return Result.Failure<HomeSectionPublicationModel>(
                SectionNotFound);
        }

        var previousPublicId =
            section.ImagePublicId;

        section.RemoveImage();

        await dbContext.SaveChangesAsync(
            cancellationToken);

        if (!string.IsNullOrWhiteSpace(
                previousPublicId))
        {
            await TryDeleteAsync(previousPublicId);
        }

        return Result.Success(ToModel(section));
    }

    public async Task<Result<HomeSectionPublicationModel>>
        SetActiveAsync(
            Guid sectionId,
            bool isActive,
            CancellationToken cancellationToken = default)
    {
        var section =
            await dbContext.HomeSections
                .SingleOrDefaultAsync(
                    item => item.Id == sectionId,
                    cancellationToken);

        if (section is null)
        {
            return Result.Failure<HomeSectionPublicationModel>(
                SectionNotFound);
        }

        if (isActive &&
            string.IsNullOrWhiteSpace(section.ImageUrl))
        {
            return Result.Failure<HomeSectionPublicationModel>(
                Error.Validation(
                    "home.image-required",
                    "Bloku aktivləşdirmək üçün şəkil yüklə."));
        }

        if (isActive)
        {
            section.Activate();
        }
        else
        {
            section.Deactivate();
        }

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return Result.Success(ToModel(section));
    }

    private async Task TryDeleteAsync(
        string publicId)
    {
        try
        {
            await imageStorage.DeleteAsync(
                publicId,
                CancellationToken.None);
        }
        catch (ImageStorageException exception)
        {
            logger.LogWarning(
                exception,
                "Köhnə banner şəkli Cloudinary-dən silinmədi. PublicId: {PublicId}",
                publicId);
        }
    }

    private static HomeSectionPublicationModel ToModel(
        HomeSection section)
    {
        return new HomeSectionPublicationModel(
            section.Id,
            section.ImageUrl,
            section.IsActive);
    }
}