using bagsisbaku.Domain.Common;
using bagsisbaku.Domain.Localization;

namespace bagsisbaku.Domain.Catalog;

public sealed class ProductTranslation : AuditableEntity
{
    private ProductTranslation()
    {
    }

    private ProductTranslation(
        Guid id,
        Guid productId,
        SupportedLanguage language,
        string name,
        string? description)
        : base(id)
    {
        ProductId = DomainGuard.NotEmpty(
            productId,
            nameof(ProductId));

        Language = TranslationGuard.SecondaryLanguage(
            language);

        Update(name, description);
    }

    public Guid ProductId { get; private set; }

    public SupportedLanguage Language { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public static ProductTranslation Create(
        Guid productId,
        SupportedLanguage language,
        string name,
        string? description)
    {
        return new ProductTranslation(
            Guid.NewGuid(),
            productId,
            language,
            name,
            description);
    }

    public void Update(
        string name,
        string? description)
    {
        Name = DomainGuard.Required(
            name,
            nameof(Name),
            200);

        Description = DomainGuard.Optional(
            description,
            nameof(Description),
            4000);
    }
}
