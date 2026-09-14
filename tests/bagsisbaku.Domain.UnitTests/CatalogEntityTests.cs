using bagsisbaku.Domain.Catalog;
using bagsisbaku.Domain.Common;
using Xunit;

namespace bagsisbaku.Domain.UnitTests;

public sealed class CatalogEntityTests
{
    [Fact]
    public void BrandCreate_ShouldTrimNameAndActivateBrand()
    {
        var brand = Brand.Create("  Guess  ");

        Assert.Equal("Guess", brand.Name);
        Assert.True(brand.IsActive);
        Assert.NotEqual(Guid.Empty, brand.Id);
    }

    [Fact]
    public void BrandCreate_ShouldRejectEmptyName()
    {
        var action = () => Brand.Create("   ");

        Assert.Throws<DomainException>(action);
    }

    [Fact]
    public void Brand_ShouldBeSharedAndShouldNotContainDefaultProperties()
    {
        Assert.Null(
            typeof(Brand).GetProperty(nameof(ProductType)));

        Assert.Null(
            typeof(Brand).GetProperty("IsDefault"));

        Assert.Null(
            typeof(Brand).GetProperty("DefaultProductType"));
    }

    [Fact]
    public void Category_ShouldBelongToProductType()
    {
        var category = Category.Create(
            "İdman ayaqqabısı",
            ProductType.Shoe);

        Assert.Equal(
            ProductType.Shoe,
            category.ProductType);
    }

    [Fact]
    public void Category_ShouldRejectUnknownProductType()
    {
        var unknownType = (ProductType)999;

        var action = () => Category.Create(
            "Naməlum",
            unknownType);

        Assert.Throws<DomainException>(action);
    }

    [Fact]
    public void Size_ShouldRejectNegativeSortOrder()
    {
        var action = () => Size.Create(
            "42",
            ProductType.Shoe,
            -1);

        Assert.Throws<DomainException>(action);
    }

    [Fact]
    public void BagSize_ShouldAllowStandardValue()
    {
        var size = Size.Create(
            "Standart",
            ProductType.Bag,
            1);

        Assert.Equal("Standart", size.Value);
        Assert.Equal(
            ProductType.Bag,
            size.ProductType);
    }

    [Fact]
    public void Color_ShouldNormalizeHexCode()
    {
        var color = Color.Create(
            "Qara",
            "1a1a1a");

        Assert.Equal("#1A1A1A", color.HexCode);
    }

    [Fact]
    public void Color_ShouldRejectInvalidHexCode()
    {
        var action = () => Color.Create(
            "Yanlış rəng",
            "ZZZZZZ");

        Assert.Throws<DomainException>(action);
    }

    [Fact]
    public void CatalogDefault_ShouldContainOnlyCategoryAndSizeDefaults()
    {
        var categoryId = Guid.NewGuid();
        var sizeId = Guid.NewGuid();

        var catalogDefault = CatalogDefault.Create(
            ProductType.Bag,
            categoryId,
            sizeId);

        Assert.Equal(
            ProductType.Bag,
            catalogDefault.ProductType);

        Assert.Equal(
            categoryId,
            catalogDefault.DefaultCategoryId);

        Assert.Equal(
            sizeId,
            catalogDefault.DefaultSizeId);

        Assert.Null(
            typeof(CatalogDefault)
                .GetProperty("DefaultBrandId"));
    }

    [Fact]
    public void CatalogDefault_ShouldRejectEmptyIds()
    {
        var action = () => CatalogDefault.Create(
            ProductType.Shoe,
            Guid.Empty,
            Guid.NewGuid());

        Assert.Throws<DomainException>(action);
    }
}
