using bagsisbaku.Domain.Localization;

namespace bagsisbaku.Application.Home;

public sealed record PublicHomeSectionModel(
    Guid Id,
    int Type,
    string Language,
    string Title,
    string? Subtitle,
    string ImageUrl,
    string? TargetUrl,
    int SortOrder);

public interface IPublicHomeSectionQuery
{
    Task<IReadOnlyList<PublicHomeSectionModel>> GetActiveAsync(
        SupportedLanguage language,
        CancellationToken cancellationToken = default);
}