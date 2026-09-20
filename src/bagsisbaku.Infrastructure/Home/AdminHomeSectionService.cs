using bagsisbaku.Application.Home;
using bagsisbaku.Domain.Home;
using bagsisbaku.Domain.Localization;
using bagsisbaku.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace bagsisbaku.Infrastructure.Home;

public sealed class AdminHomeSectionService(
    ApplicationDbContext dbContext)
    : IAdminHomeSectionService
{
    public async Task<HomeSectionModel> CreateAsync(
        CreateHomeSectionCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var section =
            HomeSection.Create(
                (HomeSectionType)command.Type,
                command.Title,
                command.Subtitle,
                command.TargetUrl,
                command.SortOrder);

        var translations =
            (command.Translations ??
                Array.Empty<HomeSectionTranslationInput>())
            .Select(input =>
                HomeSectionTranslation.Create(
                    section.Id,
                    (SupportedLanguage)input.Language,
                    input.Title,
                    input.Subtitle))
            .ToArray();

        dbContext.HomeSections.Add(section);

        dbContext.HomeSectionTranslations
            .AddRange(translations);

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return ToModel(section, translations);
    }

    public async Task<IReadOnlyList<HomeSectionModel>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var sections =
            await dbContext.HomeSections
                .AsNoTracking()
                .OrderBy(section => section.SortOrder)
                .ThenBy(section => section.Id)
                .ToListAsync(cancellationToken);

        if (sections.Count == 0)
        {
            return [];
        }

        var sectionIds =
            sections.Select(section => section.Id).ToArray();

        var translations =
            await dbContext.HomeSectionTranslations
                .AsNoTracking()
                .Where(translation =>
                    sectionIds.Contains(
                        translation.HomeSectionId))
                .ToListAsync(cancellationToken);

        var translationsBySection =
            translations.ToLookup(
                translation =>
                    translation.HomeSectionId);

        return sections
            .Select(section =>
                ToModel(
                    section,
                    translationsBySection[
                        section.Id]))
            .ToArray();
    }

    private static HomeSectionModel ToModel(
        HomeSection section,
        IEnumerable<HomeSectionTranslation> translations)
    {
        return new HomeSectionModel(
            section.Id,
            (int)section.Type,
            section.Title,
            section.Subtitle,
            section.TargetUrl,
            section.ImageUrl,
            section.SortOrder,
            section.IsActive,
            translations
                .OrderBy(translation =>
                    translation.Language)
                .Select(translation =>
                    new HomeSectionTranslationModel(
                        (int)translation.Language,
                        translation.Title,
                        translation.Subtitle))
                .ToArray());
    }
}