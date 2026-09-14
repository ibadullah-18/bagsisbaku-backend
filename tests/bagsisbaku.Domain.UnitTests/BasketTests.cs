using bagsisbaku.Domain.Baskets;
using bagsisbaku.Domain.Common;
using Xunit;

namespace bagsisbaku.Domain.UnitTests;

public sealed class BasketTests
{
    [Fact]
    public void AddItemShouldAddVariantToBasket()
    {
        var basket =
            Basket.Create(
                Guid.NewGuid());

        var variantId =
            Guid.NewGuid();

        var item =
            basket.AddItem(
                variantId,
                2);

        Assert.Single(
            basket.Items);

        Assert.Equal(
            variantId,
            item.ProductVariantId);

        Assert.Equal(
            2,
            item.Quantity);

        Assert.Equal(
            2,
            basket.TotalQuantity);
    }

    [Fact]
    public void AddingSameVariantShouldIncreaseQuantity()
    {
        var basket =
            Basket.Create(
                Guid.NewGuid());

        var variantId =
            Guid.NewGuid();

        basket.AddItem(
            variantId,
            2);

        basket.AddItem(
            variantId,
            3);

        var item =
            Assert.Single(
                basket.Items);

        Assert.Equal(
            5,
            item.Quantity);
    }

    [Fact]
    public void UpdateAndRemoveShouldChangeBasket()
    {
        var basket =
            Basket.Create(
                Guid.NewGuid());

        var item =
            basket.AddItem(
                Guid.NewGuid(),
                1);

        basket.UpdateItemQuantity(
            item.Id,
            4);

        Assert.Equal(
            4,
            item.Quantity);

        basket.RemoveItem(
            item.Id);

        Assert.Empty(
            basket.Items);
    }

    [Fact]
    public void QuantityShouldNotExceedMaximum()
    {
        var basket =
            Basket.Create(
                Guid.NewGuid());

        Assert.Throws<DomainException>(
            () => basket.AddItem(
                Guid.NewGuid(),
                BasketItem.MaximumQuantity + 1));
    }
}