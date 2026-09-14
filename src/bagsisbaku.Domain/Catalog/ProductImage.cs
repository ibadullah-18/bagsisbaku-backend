using bagsisbaku.Domain.Common;

namespace bagsisbaku.Domain.Catalog;

public sealed class ProductImage : Entity
{
    private ProductImage()
    {
    }

    private ProductImage(
        Guid id,
        Guid productId,
        string imageUrl,
        string imagePublicId,
        int sortOrder,
        bool isPrimary)
        : base(id)
    {
        ProductId = DomainGuard.NotEmpty(
            productId,
            nameof(ProductId));

        ImageUrl = DomainGuard.Required(
            imageUrl,
            nameof(ImageUrl),
            2048);

        ImagePublicId = DomainGuard.Required(
            imagePublicId,
            nameof(ImagePublicId),
            255);

        SortOrder = DomainGuard.NotNegative(
            sortOrder,
            nameof(SortOrder));

        IsPrimary = isPrimary;
    }

    public Guid ProductId { get; private set; }

    public string ImageUrl { get; private set; } = string.Empty;

    public string ImagePublicId { get; private set; } = string.Empty;

    public int SortOrder { get; private set; }

    public bool IsPrimary { get; private set; }

    internal static ProductImage Create(
        Guid productId,
        string imageUrl,
        string imagePublicId,
        int sortOrder,
        bool isPrimary)
    {
        return new ProductImage(
            Guid.NewGuid(),
            productId,
            imageUrl,
            imagePublicId,
            sortOrder,
            isPrimary);
    }

    internal void SetSortOrder(int sortOrder)
    {
        SortOrder = DomainGuard.NotNegative(
            sortOrder,
            nameof(SortOrder));
    }

    internal void SetPrimary(bool isPrimary)
    {
        IsPrimary = isPrimary;
    }
}
