using bagsisbaku.Domain.Common;

namespace bagsisbaku.Domain.Catalog;

public sealed class CatalogDefault : AuditableEntity
{
    private CatalogDefault()
    {
    }

    private CatalogDefault(
        Guid id,
        ProductType productType,
        Guid defaultCategoryId,
        Guid defaultSizeId)
        : base(id)
    {
        ProductType = DomainGuard.DefinedEnum(
            productType,
            nameof(ProductType));

        SetDefaults(
            defaultCategoryId,
            defaultSizeId);
    }

    public ProductType ProductType { get; private set; }

    public Guid DefaultCategoryId { get; private set; }

    public Guid DefaultSizeId { get; private set; }

    public static CatalogDefault Create(
        ProductType productType,
        Guid defaultCategoryId,
        Guid defaultSizeId)
    {
        return new CatalogDefault(
            Guid.NewGuid(),
            productType,
            defaultCategoryId,
            defaultSizeId);
    }

    public void SetDefaults(
        Guid defaultCategoryId,
        Guid defaultSizeId)
    {
        DefaultCategoryId = DomainGuard.NotEmpty(
            defaultCategoryId,
            nameof(DefaultCategoryId));

        DefaultSizeId = DomainGuard.NotEmpty(
            defaultSizeId,
            nameof(DefaultSizeId));
    }
}
