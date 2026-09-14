using bagsisbaku.Domain.Common;
using bagsisbaku.Domain.Localization;

namespace bagsisbaku.Domain.Catalog;

public sealed class SizeTranslation : AuditableEntity
{
    private SizeTranslation()
    {
    }

    private SizeTranslation(
        Guid id,
        Guid sizeId,
        SupportedLanguage language,
        string value)
        : base(id)
    {
        SizeId = DomainGuard.NotEmpty(
            sizeId,
            nameof(SizeId));

        Language = TranslationGuard.SecondaryLanguage(
            language);

        UpdateValue(value);
    }

    public Guid SizeId { get; private set; }

    public SupportedLanguage Language { get; private set; }

    public string Value { get; private set; } = string.Empty;

    public static SizeTranslation Create(
        Guid sizeId,
        SupportedLanguage language,
        string value)
    {
        return new SizeTranslation(
            Guid.NewGuid(),
            sizeId,
            language,
            value);
    }

    public void UpdateValue(string value)
    {
        Value = DomainGuard.Required(
            value,
            nameof(Value),
            50);
    }
}
