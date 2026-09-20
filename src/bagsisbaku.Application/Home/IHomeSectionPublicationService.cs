using bagsisbaku.Application.Common.Results;
using bagsisbaku.Application.Media;

namespace bagsisbaku.Application.Home;

public sealed record HomeSectionPublicationModel(
    Guid Id,
    string? ImageUrl,
    bool IsActive);

public interface IHomeSectionPublicationService
{
    Task<Result<HomeSectionPublicationModel>> UploadImageAsync(
        Guid sectionId,
        ImageUploadCommand command,
        CancellationToken cancellationToken = default);

    Task<Result<HomeSectionPublicationModel>> RemoveImageAsync(
        Guid sectionId,
        CancellationToken cancellationToken = default);

    Task<Result<HomeSectionPublicationModel>> SetActiveAsync(
        Guid sectionId,
        bool isActive,
        CancellationToken cancellationToken = default);
}