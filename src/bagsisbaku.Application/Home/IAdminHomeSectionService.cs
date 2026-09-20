namespace bagsisbaku.Application.Home;

public sealed record HomeSectionTranslationInput(
    int Language,
    string Title,
    string? Subtitle);

public sealed record CreateHomeSectionCommand(
    int Type,
    string Title,
    string? Subtitle,
    string? TargetUrl,
    int SortOrder,
    IReadOnlyList<HomeSectionTranslationInput>? Translations);

public sealed record HomeSectionTranslationModel(
    int Language,
    string Title,
    string? Subtitle);

public sealed record HomeSectionModel(
    Guid Id,
    int Type,
    string Title,
    string? Subtitle,
    string? TargetUrl,
    string? ImageUrl,
    int SortOrder,
    bool IsActive,
    IReadOnlyList<HomeSectionTranslationModel> Translations);

public interface IAdminHomeSectionService
{
    Task<HomeSectionModel> CreateAsync(
        CreateHomeSectionCommand command,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HomeSectionModel>> GetAllAsync(
        CancellationToken cancellationToken = default);
}