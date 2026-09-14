using bagsisbaku.Domain.Common;

namespace bagsisbaku.Domain.Favorites;

public sealed class Favorite : AuditableEntity
{
    private Favorite()
    {
    }

    private Favorite(
        Guid id,
        Guid userId,
        Guid productId)
        : base(id)
    {
        UserId =
            DomainGuard.NotEmpty(
                userId,
                nameof(UserId));

        ProductId =
            DomainGuard.NotEmpty(
                productId,
                nameof(ProductId));
    }

    public Guid UserId { get; private set; }

    public Guid ProductId { get; private set; }

    public static Favorite Create(
        Guid userId,
        Guid productId)
    {
        return new Favorite(
            Guid.NewGuid(),
            userId,
            productId);
    }
}