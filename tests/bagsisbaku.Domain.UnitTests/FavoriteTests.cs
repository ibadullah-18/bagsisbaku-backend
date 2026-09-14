using bagsisbaku.Domain.Common;
using bagsisbaku.Domain.Favorites;
using Xunit;

namespace bagsisbaku.Domain.UnitTests;

public sealed class FavoriteTests
{
    [Fact]
    public void CreateShouldCreateFavorite()
    {
        var userId =
            Guid.NewGuid();

        var productId =
            Guid.NewGuid();

        var favorite =
            Favorite.Create(
                userId,
                productId);

        Assert.NotEqual(
            Guid.Empty,
            favorite.Id);

        Assert.Equal(
            userId,
            favorite.UserId);

        Assert.Equal(
            productId,
            favorite.ProductId);
    }

    [Fact]
    public void CreateShouldRejectEmptyUserId()
    {
        var productId =
            Guid.NewGuid();

        Assert.Throws<DomainException>(
            () =>
                Favorite.Create(
                    Guid.Empty,
                    productId));
    }

    [Fact]
    public void CreateShouldRejectEmptyProductId()
    {
        var userId =
            Guid.NewGuid();

        Assert.Throws<DomainException>(
            () =>
                Favorite.Create(
                    userId,
                    Guid.Empty));
    }
}