using bagsisbaku.Domain.Common;
using bagsisbaku.Domain.Localization;

namespace bagsisbaku.Domain.Catalog;

public sealed class ColorTranslation : AuditableEntity
{
    private ColorTranslation()
    {
    }

    private ColorTranslation(
        Guid id,
        Guid colorId,
        SupportedLanguage language,
        string name)
        : base(id)
    {
        ColorId = DomainGuard.NotEmpty(
            colorId,
            nameof(ColorId));

        Language = TranslationGuard.SecondaryLanguage(
            language);

        UpdateName(name);
    }

    public Guid ColorId { get; private set; }

    public SupportedLanguage Language { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public static ColorTranslation Create(
        Guid colorId,
        SupportedLanguage language,
        string name)
    {
        return new ColorTranslation(
            Guid.NewGuid(),
            colorId,
            language,
            name);
    }

    public void UpdateName(string name)
    {
        Name = DomainGuard.Required(
            name,
            nameof(Name),
            80);
    }
}
