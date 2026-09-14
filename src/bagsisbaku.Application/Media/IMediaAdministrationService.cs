using bagsisbaku.Application.Abstractions.Storage;
using bagsisbaku.Application.Common.Results;

namespace bagsisbaku.Application.Media;

public interface IMediaAdministrationService
{
    Task<Result<StoredImage>> UploadBrandLogoAsync(
        Guid brandId,
        ImageUploadCommand command,
        CancellationToken cancellationToken = default);

    Task<Result> RemoveBrandLogoAsync(
        Guid brandId,
        CancellationToken cancellationToken = default);

    Task<Result<StoredImage>> UploadCategoryIconAsync(
        Guid categoryId,
        ImageUploadCommand command,
        CancellationToken cancellationToken = default);

    Task<Result> RemoveCategoryIconAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default);

    Task<Result<ProductImageUploadModel>> AddProductImageAsync(
        Guid productId,
        ImageUploadCommand command,
        CancellationToken cancellationToken = default);

    Task<Result> RemoveProductImageAsync(
        Guid productId,
        Guid imageId,
        CancellationToken cancellationToken = default);

    Task<Result> SetPrimaryProductImageAsync(
        Guid productId,
        Guid imageId,
        CancellationToken cancellationToken = default);
}

public sealed record ProductImageUploadModel(
    Guid ImageId,
    StoredImage StoredImage,
    bool IsPrimary,
    int SortOrder);
