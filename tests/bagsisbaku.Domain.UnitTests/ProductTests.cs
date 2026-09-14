using Xunit;
using bagsisbaku.Domain.Catalog;
using bagsisbaku.Domain.Common;

namespace bagsisbaku.Domain.UnitTests;

public sealed class ProductTests
{
    [Fact]
    public void Create_ShouldCreateActiveShoeProduct()
    {
        var product = CreateProduct();

        Assert.NotEqual(Guid.Empty, product.Id);
        Assert.Equal("Nike Air Force", product.Name);
        Assert.Equal(ProductType.Shoe, product.ProductType);
        Assert.True(product.IsActive);
        Assert.False(product.IsDiscounted);
        Assert.Equal(0, product.ViewCount);
    }

    [Fact]
    public void Create_ShouldRejectZeroPrice()
    {
        var action = () => Product.Create(
            "Nike Air Force",
            null,
            "NK-001",
            "Air Force",
            0,
            null,
            ProductType.Shoe,
            Guid.NewGuid(),
            Guid.NewGuid());

        Assert.Throws<DomainException>(action);
    }

    [Fact]
    public void UpdatePricing_ShouldActivateDiscount()
    {
        var product = CreateProduct();

        product.UpdatePricing(
            price: 200,
            discountPrice: 170);

        Assert.Equal(200, product.Price);
        Assert.Equal(170, product.DiscountPrice);
        Assert.True(product.IsDiscounted);
    }

    [Fact]
    public void UpdatePricing_ShouldRejectDiscountEqualToPrice()
    {
        var product = CreateProduct();

        var action = () => product.UpdatePricing(
            price: 200,
            discountPrice: 200);

        Assert.Throws<DomainException>(action);
    }

    [Fact]
    public void AddVariant_ShouldAddSizeColorAndStock()
    {
        var product = CreateProduct();
        var sizeId = Guid.NewGuid();
        var colorId = Guid.NewGuid();

        var variant = product.AddVariant(
            sizeId,
            colorId,
            5);

        Assert.Equal(sizeId, variant.SizeId);
        Assert.Equal(colorId, variant.ColorId);
        Assert.Equal(5, variant.StockCount);
        Assert.Single(product.Variants);
    }

    [Fact]
    public void AddVariant_ShouldRejectDuplicateSizeAndColor()
    {
        var product = CreateProduct();
        var sizeId = Guid.NewGuid();
        var colorId = Guid.NewGuid();

        product.AddVariant(
            sizeId,
            colorId,
            5);

        var action = () => product.AddVariant(
            sizeId,
            colorId,
            3);

        Assert.Throws<DomainException>(action);
    }

    [Fact]
    public void Variant_ShouldIncreaseAndDecreaseStock()
    {
        var product = CreateProduct();

        var variant = product.AddVariant(
            Guid.NewGuid(),
            Guid.NewGuid(),
            5);

        variant.IncreaseStock(3);
        variant.DecreaseStock(2);

        Assert.Equal(6, variant.StockCount);
    }

    [Fact]
    public void Variant_ShouldRejectInsufficientStock()
    {
        var product = CreateProduct();

        var variant = product.AddVariant(
            Guid.NewGuid(),
            Guid.NewGuid(),
            2);

        var action = () => variant.DecreaseStock(3);

        Assert.Throws<DomainException>(action);
        Assert.Equal(2, variant.StockCount);
    }

    [Fact]
    public void AddImage_ShouldMakeFirstImagePrimary()
    {
        var product = CreateProduct();

        var firstImage = product.AddImage(
            "https://images.example.com/first.webp",
            "bagsisbaku/products/first");

        var secondImage = product.AddImage(
            "https://images.example.com/second.webp",
            "bagsisbaku/products/second");

        Assert.True(firstImage.IsPrimary);
        Assert.False(secondImage.IsPrimary);
        Assert.Equal(0, firstImage.SortOrder);
        Assert.Equal(1, secondImage.SortOrder);
    }

    [Fact]
    public void SetPrimaryImage_ShouldChangePrimaryImage()
    {
        var product = CreateProduct();

        var firstImage = product.AddImage(
            "https://images.example.com/first.webp",
            "bagsisbaku/products/first");

        var secondImage = product.AddImage(
            "https://images.example.com/second.webp",
            "bagsisbaku/products/second");

        product.SetPrimaryImage(secondImage.Id);

        Assert.False(firstImage.IsPrimary);
        Assert.True(secondImage.IsPrimary);

        Assert.Equal(
            1,
            product.Images.Count(image => image.IsPrimary));
    }

    [Fact]
    public void RemoveImage_ShouldPromoteNextImageWhenPrimaryIsRemoved()
    {
        var product = CreateProduct();

        var firstImage = product.AddImage(
            "https://images.example.com/first.webp",
            "bagsisbaku/products/first");

        var secondImage = product.AddImage(
            "https://images.example.com/second.webp",
            "bagsisbaku/products/second");

        var removedImage = product.RemoveImage(firstImage.Id);

        Assert.Equal(firstImage.Id, removedImage.Id);
        Assert.Single(product.Images);
        Assert.True(secondImage.IsPrimary);
        Assert.Equal(0, secondImage.SortOrder);
    }

    [Fact]
    public void ReorderImages_ShouldUpdateSortOrders()
    {
        var product = CreateProduct();

        var firstImage = product.AddImage(
            "https://images.example.com/first.webp",
            "bagsisbaku/products/first");

        var secondImage = product.AddImage(
            "https://images.example.com/second.webp",
            "bagsisbaku/products/second");

        product.ReorderImages(
            [secondImage.Id, firstImage.Id]);

        Assert.Equal(1, firstImage.SortOrder);
        Assert.Equal(0, secondImage.SortOrder);
    }

    [Fact]
    public void RegisterView_ShouldIncreaseViewCount()
    {
        var product = CreateProduct();

        product.RegisterView();
        product.RegisterView();

        Assert.Equal(2, product.ViewCount);
    }

    private static Product CreateProduct()
    {
        return Product.Create(
            name: "Nike Air Force",
            description: "Premium ayaqqabı",
            productCode: "NK-001",
            model: "Air Force",
            price: 200,
            discountPrice: null,
            productType: ProductType.Shoe,
            categoryId: Guid.NewGuid(),
            brandId: Guid.NewGuid());
    }
}

