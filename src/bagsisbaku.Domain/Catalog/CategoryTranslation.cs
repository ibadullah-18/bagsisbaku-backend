using bagsisbaku.Domain.Common;
using bagsisbaku.Domain.Localization;

namespace bagsisbaku.Domain.Catalog;

public sealed class CategoryTranslation : AuditableEntity
{
    private CategoryTranslation()
    {
    }

    private CategoryTranslation(
        Guid id,
        Guid categoryId,
        SupportedLanguage language,
        string name)
        : base(id)
    {
        CategoryId = DomainGuard.NotEmpty(
            categoryId,
            nameof(CategoryId));

        Language = TranslationGuard.SecondaryLanguage(
            language);

        UpdateName(name);
    }

    public Guid CategoryId { get; private set; }

    public SupportedLanguage Language { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public static CategoryTranslation Create(
        Guid categoryId,
        SupportedLanguage language,
        string name)
    {
        return new CategoryTranslation(
            Guid.NewGuid(),
            categoryId,
            language,
            name);
    }

    public void UpdateName(string name)
    {
        Name = DomainGuard.Required(
            name,
            nameof(Name),
            120);
    }
}
