using bagsisbaku.Application.Home;
using bagsisbaku.Domain.Localization;
using bagsisbaku.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace bagsisbaku.Infrastructure.Home;

public sealed class PublicHomeSectionQuery(
    ApplicationDbContext dbContext)
    : IPublicHomeSectionQuery
{
    public async Task<IReadOnlyList<PublicHomeSectionModel>>
        GetActiveAsync(
            SupportedLanguage language,
            CancellationToken cancellationToken = default)
    {
        var sections =
            await dbContext.HomeSections
                .AsNoTracking()
                .Where(section =>
                    section.IsActive &&
                    section.ImageUrl != null)
                .OrderBy(section => section.SortOrder)
                .ThenBy(section => section.Id)
                .ToListAsync(cancellationToken);

        if (sections.Count == 0)
        {
            return [];
        }

        var sectionIds =
            sections
                .Select(section => section.Id)
                .ToArray();

        var translations =
            language == SupportedLanguage.Azerbaijani
                ? []
                : await dbContext.HomeSectionTranslations
                    .AsNoTracking()
                    .Where(translation =>
                        sectionIds.Contains(
                            translation.HomeSectionId) &&
                        translation.Language == language)
                    .ToListAsync(cancellationToken);

        var translationBySectionId =
            translations.ToDictionary(
                translation => translation.HomeSectionId);

        var languageCode =
            language switch
            {
                SupportedLanguage.Russian => "ru",
                SupportedLanguage.English => "en",
                _ => "az"
            };

        return sections
            .Select(section =>
            {
                translationBySectionId.TryGetValue(
                    section.Id,
                    out var translation);

                return new PublicHomeSectionModel(
                    section.Id,
                    (int)section.Type,
                    languageCode,
                    translation?.Title ?? section.Title,
                    translation?.Subtitle ??
                        section.Subtitle,
                    section.ImageUrl!,
                    section.TargetUrl,
                    section.SortOrder);
            })
            .ToArray();
    }
}