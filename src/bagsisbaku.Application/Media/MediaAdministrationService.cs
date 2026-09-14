using bagsisbaku.Application.Abstractions.Persistence;
using bagsisbaku.Application.Abstractions.Storage;
using bagsisbaku.Application.Common.Results;
using bagsisbaku.Domain.Common;

namespace bagsisbaku.Application.Media;

internal sealed class MediaAdministrationService(
    IBrandRepository brandRepository,
    ICategoryRepository categoryRepository,
    IProductRepository productRepository,
    IImageStorage imageStorage,
    IUnitOfWork unitOfWork)
    : IMediaAdministrationService
{
    public async Task<Result<StoredImage>> UploadBrandLogoAsync(
        Guid brandId,
        ImageUploadCommand command,
        CancellationToken cancellationToken = default)
    {
        var brand = await brandRepository.GetByIdAsync(
            brandId,
            cancellationToken);

        if (brand is null)
        {
            return Result.Failure<StoredImage>(
                MediaErrors.BrandNotFound);
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
            uploadedImage = await imageStorage.UploadAsync(
                command.Content,
                command.FileName,
                ImageFolder.Brands,
                cancellationToken);

            var previousPublicId = brand.ImagePublicId;

            brand.SetImage(
                uploadedImage.Url,
                uploadedImage.PublicId);

            await unitOfWork.SaveChangesAsync(
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(previousPublicId) &&
                previousPublicId != uploadedImage.PublicId)
            {
                await TryDeleteAsync(previousPublicId);
            }

            return Result.Success(uploadedImage);
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
                await TryDeleteAsync(
                    uploadedImage.PublicId);
            }

            throw;
        }
    }

    public async Task<Result> RemoveBrandLogoAsync(
        Guid brandId,
        CancellationToken cancellationToken = default)
    {
        var brand = await brandRepository.GetByIdAsync(
            brandId,
            cancellationToken);

        if (brand is null)
        {
            return Result.Failure(
                MediaErrors.BrandNotFound);
        }

        var publicId = brand.ImagePublicId;

        brand.RemoveImage();

        await unitOfWork.SaveChangesAsync(
            cancellationToken);

        if (!string.IsNullOrWhiteSpace(publicId))
        {
            await TryDeleteAsync(publicId);
        }

        return Result.Success();
    }

    public async Task<Result<StoredImage>>
        UploadCategoryIconAsync(
            Guid categoryId,
            ImageUploadCommand command,
            CancellationToken cancellationToken = default)
    {
        var category =
            await categoryRepository.GetByIdAsync(
                categoryId,
                cancellationToken);

        if (category is null)
        {
            return Result.Failure<StoredImage>(
                MediaErrors.CategoryNotFound);
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
            uploadedImage = await imageStorage.UploadAsync(
                command.Content,
                command.FileName,
                ImageFolder.Categories,
                cancellationToken);

            var previousPublicId = category.IconPublicId;

            category.SetIcon(
                uploadedImage.Url,
                uploadedImage.PublicId);

            await unitOfWork.SaveChangesAsync(
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(previousPublicId) &&
                previousPublicId != uploadedImage.PublicId)
            {
                await TryDeleteAsync(previousPublicId);
            }

            return Result.Success(uploadedImage);
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
                await TryDeleteAsync(
                    uploadedImage.PublicId);
            }

            throw;
        }
    }

    public async Task<Result> RemoveCategoryIconAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        var category =
            await categoryRepository.GetByIdAsync(
                categoryId,
                cancellationToken);

        if (category is null)
        {
            return Result.Failure(
                MediaErrors.CategoryNotFound);
        }

        var publicId = category.IconPublicId;

        category.RemoveIcon();

        await unitOfWork.SaveChangesAsync(
            cancellationToken);

        if (!string.IsNullOrWhiteSpace(publicId))
        {
            await TryDeleteAsync(publicId);
        }

        return Result.Success();
    }

    public async Task<Result<ProductImageUploadModel>>
        AddProductImageAsync(
            Guid productId,
            ImageUploadCommand command,
            CancellationToken cancellationToken = default)
    {
        var product =
            await productRepository
                .GetForUpdateWithDetailsByIdAsync(
                    productId,
                    cancellationToken);

        if (product is null)
        {
            return Result.Failure<ProductImageUploadModel>(
                MediaErrors.ProductNotFound);
        }

        var validationError =
            await ImageFileValidator.ValidateAsync(
                command,
                cancellationToken);

        if (validationError is not null)
        {
            return Result.Failure<ProductImageUploadModel>(
                validationError);
        }

        StoredImage? uploadedImage = null;

        try
        {
            uploadedImage = await imageStorage.UploadAsync(
                command.Content,
                command.FileName,
                ImageFolder.Products,
                cancellationToken);

            var productImage = product.AddImage(
                uploadedImage.Url,
                uploadedImage.PublicId);

            await unitOfWork.SaveChangesAsync(
                cancellationToken);

            return Result.Success(
                new ProductImageUploadModel(
                    productImage.Id,
                    uploadedImage,
                    productImage.IsPrimary,
                    productImage.SortOrder));
        }
        catch (DomainException exception)
        {
            if (uploadedImage is not null)
            {
                await TryDeleteAsync(
                    uploadedImage.PublicId);
            }

            return Result.Failure<ProductImageUploadModel>(
                MediaErrors.Invalid(
                    exception.Message));
        }
        catch (ImageStorageException)
        {
            return Result.Failure<ProductImageUploadModel>(
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

    public async Task<Result> RemoveProductImageAsync(
        Guid productId,
        Guid imageId,
        CancellationToken cancellationToken = default)
    {
        var product =
            await productRepository
                .GetForUpdateWithDetailsByIdAsync(
                    productId,
                    cancellationToken);

        if (product is null)
        {
            return Result.Failure(
                MediaErrors.ProductNotFound);
        }

        try
        {
            var removedImage =
                product.RemoveImage(imageId);

            await unitOfWork.SaveChangesAsync(
                cancellationToken);

            await TryDeleteAsync(
                removedImage.ImagePublicId);

            return Result.Success();
        }
        catch (DomainException exception)
        {
            return Result.Failure(
                MediaErrors.Invalid(
                    exception.Message));
        }
    }

    public async Task<Result> SetPrimaryProductImageAsync(
        Guid productId,
        Guid imageId,
        CancellationToken cancellationToken = default)
    {
        var product =
            await productRepository
                .GetForUpdateWithDetailsByIdAsync(
                    productId,
                    cancellationToken);

        if (product is null)
        {
            return Result.Failure(
                MediaErrors.ProductNotFound);
        }

        try
        {
            product.SetPrimaryImage(imageId);

            await unitOfWork.SaveChangesAsync(
                cancellationToken);

            return Result.Success();
        }
        catch (DomainException exception)
        {
            return Result.Failure(
                MediaErrors.Invalid(
                    exception.Message));
        }
    }

    private async Task TryDeleteAsync(string publicId)
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
            // Köhnə orphan şəkillər sonradan Worker ilə təmizlənəcək.
        }
    }
}
