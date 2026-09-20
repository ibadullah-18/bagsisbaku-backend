using bagsisbaku.Domain.Common;
using bagsisbaku.Domain.Localization;

namespace bagsisbaku.Domain.Home;

public sealed class HomeSectionTranslation : AuditableEntity
{
    private HomeSectionTranslation()
    {
    }

    private HomeSectionTranslation(
        Guid id,
        Guid homeSectionId,
        SupportedLanguage language,
        string title,
        string? subtitle)
        : base(id)
    {
        HomeSectionId = DomainGuard.NotEmpty(
            homeSectionId,
            nameof(HomeSectionId));

        Language = DomainGuard.DefinedEnum(
            language,
            nameof(Language));

        Update(title, subtitle);
    }

    public Guid HomeSectionId { get; private set; }

    public SupportedLanguage Language { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string? Subtitle { get; private set; }

    public static HomeSectionTranslation Create(
        Guid homeSectionId,
        SupportedLanguage language,
        string title,
        string? subtitle)
    {
        return new HomeSectionTranslation(
            Guid.NewGuid(),
            homeSectionId,
            language,
            title,
            subtitle);
    }

    public void Update(string title, string? subtitle)
    {
        Title = DomainGuard.Required(
            title,
            nameof(Title),
            160);

        Subtitle = DomainGuard.Optional(
            subtitle,
            nameof(Subtitle),
            500);
    }
}
