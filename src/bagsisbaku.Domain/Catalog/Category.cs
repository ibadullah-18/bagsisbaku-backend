using bagsisbaku.Domain.Common;

namespace bagsisbaku.Domain.Catalog;

public sealed class Category : AuditableEntity
{
    private Category()
    {
    }

    private Category(
        Guid id,
        string name,
        ProductType productType)
        : base(id)
    {
        Name = DomainGuard.Required(
            name,
            nameof(Name),
            120);

        ProductType = DomainGuard.DefinedEnum(
            productType,
            nameof(ProductType));

        IsActive = true;
    }

    public string Name { get; private set; } = string.Empty;

    public ProductType ProductType { get; private set; }

    public string? IconUrl { get; private set; }

    public string? IconPublicId { get; private set; }

    public bool IsActive { get; private set; }

    public static Category Create(
        string name,
        ProductType productType)
    {
        return new Category(
            Guid.NewGuid(),
            name,
            productType);
    }

    public void UpdateName(string name)
    {
        Name = DomainGuard.Required(
            name,
            nameof(Name),
            120);
    }

    public void SetIcon(
        string iconUrl,
        string iconPublicId)
    {
        IconUrl = DomainGuard.Required(
            iconUrl,
            nameof(IconUrl),
            2048);

        IconPublicId = DomainGuard.Required(
            iconPublicId,
            nameof(IconPublicId),
            255);
    }

    public void RemoveIcon()
    {
        IconUrl = null;
        IconPublicId = null;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
