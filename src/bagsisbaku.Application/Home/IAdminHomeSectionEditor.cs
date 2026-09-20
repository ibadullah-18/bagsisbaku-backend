using bagsisbaku.Application.Common.Results;

namespace bagsisbaku.Application.Home;

public sealed record UpdateHomeSectionCommand(
    string Title,
    string? Subtitle,
    string? TargetUrl,
    int SortOrder,
    IReadOnlyList<HomeSectionTranslationInput>? Translations);

public interface IAdminHomeSectionEditor
{
    Task<Result<HomeSectionModel>> UpdateAsync(
        Guid sectionId,
        UpdateHomeSectionCommand command,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(
        Guid sectionId,
        CancellationToken cancellationToken = default);
}