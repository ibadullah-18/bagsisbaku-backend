using bagsisbaku.Application.Abstractions.Storage;
using bagsisbaku.Application.Common.Results;
using bagsisbaku.Application.Home;
using bagsisbaku.Domain.Home;
using bagsisbaku.Domain.Localization;
using bagsisbaku.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace bagsisbaku.Infrastructure.Home;

public sealed class AdminHomeSectionEditor(
    ApplicationDbContext dbContext,
    IImageStorage imageStorage,
    ILogger<AdminHomeSectionEditor> logger)
    : IAdminHomeSectionEditor
{
    private static readonly Error SectionNotFound =
        Error.NotFound(
            "home.section-not-found",
            "Ana səhifə bloku tapılmadı.");

    public async Task<Result<HomeSectionModel>> UpdateAsync(
        Guid sectionId,
        UpdateHomeSectionCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var section =
            await dbContext.HomeSections
                .SingleOrDefaultAsync(
                    item => item.Id == sectionId,
                    cancellationToken);

        if (section is null)
        {
            return Result.Failure<HomeSectionModel>(
                SectionNotFound);
        }

        section.UpdateContent(
            command.Title,
            command.Subtitle,
            command.TargetUrl);

        section.SetSortOrder(command.SortOrder);

        var translations =
            await dbContext.HomeSectionTranslations
                .Where(item =>
                    item.HomeSectionId == sectionId)
                .ToListAsync(cancellationToken);

        if (command.Translations is not null)
        {
            var requestedLanguages =
                command.Translations
                    .Select(item =>
                        (SupportedLanguage)item.Language)
                    .ToHashSet();

            var removedTranslations =
                translations
                    .Where(item =>
                        !requestedLanguages.Contains(
                            item.Language))
                    .ToArray();

            dbContext.HomeSectionTranslations
                .RemoveRange(removedTranslations);

            foreach (var input in command.Translations)
            {
                var language =
                    (SupportedLanguage)input.Language;

                var existing =
                    translations.FirstOrDefault(
                        item =>
                            item.Language == language);

                if (existing is not null)
                {
                    existing.Update(
                        input.Title,
                        input.Subtitle);

                    continue;
                }

                var created =
                    HomeSectionTranslation.Create(
                        sectionId,
                        language,
                        input.Title,
                        input.Subtitle);

                dbContext.HomeSectionTranslations.Add(
                    created);

                translations.Add(created);
            }

            translations.RemoveAll(item =>
                !requestedLanguages.Contains(
                    item.Language));
        }

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return Result.Success(
            new HomeSectionModel(
                section.Id,
                (int)section.Type,
                section.Title,
                section.Subtitle,
                section.TargetUrl,
                section.ImageUrl,
                section.SortOrder,
                section.IsActive,
                translations
                    .OrderBy(item => item.Language)
                    .Select(item =>
                        new HomeSectionTranslationModel(
                            (int)item.Language,
                            item.Title,
                            item.Subtitle))
                    .ToArray()));
    }

    public async Task<Result> DeleteAsync(
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
            return Result.Failure(SectionNotFound);
        }

        var imagePublicId =
            section.ImagePublicId;

        dbContext.HomeSections.Remove(section);

        await dbContext.SaveChangesAsync(
            cancellationToken);

        if (!string.IsNullOrWhiteSpace(imagePublicId))
        {
            try
            {
                await imageStorage.DeleteAsync(
                    imagePublicId,
                    CancellationToken.None);
            }
            catch (ImageStorageException exception)
            {
                logger.LogWarning(
                    exception,
                    "Silinmiş bannerin şəkli Cloudinary-də qaldı. PublicId: {PublicId}",
                    imagePublicId);
            }
        }

        return Result.Success();
    }
}