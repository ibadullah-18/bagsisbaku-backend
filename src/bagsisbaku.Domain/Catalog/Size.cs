using bagsisbaku.Domain.Common;

namespace bagsisbaku.Domain.Catalog;

public sealed class Size : AuditableEntity
{
    private Size()
    {
    }

    private Size(
        Guid id,
        string value,
        ProductType productType,
        int sortOrder)
        : base(id)
    {
        Value = DomainGuard.Required(
            value,
            nameof(Value),
            50);

        ProductType = DomainGuard.DefinedEnum(
            productType,
            nameof(ProductType));

        SortOrder = DomainGuard.NotNegative(
            sortOrder,
            nameof(SortOrder));

        IsActive = true;
    }

    public string Value { get; private set; } = string.Empty;

    public ProductType ProductType { get; private set; }

    public int SortOrder { get; private set; }

    public bool IsActive { get; private set; }

    public static Size Create(
        string value,
        ProductType productType,
        int sortOrder)
    {
        return new Size(
            Guid.NewGuid(),
            value,
            productType,
            sortOrder);
    }

    public void Update(
        string value,
        int sortOrder)
    {
        Value = DomainGuard.Required(
            value,
            nameof(Value),
            50);

        SortOrder = DomainGuard.NotNegative(
            sortOrder,
            nameof(SortOrder));
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
