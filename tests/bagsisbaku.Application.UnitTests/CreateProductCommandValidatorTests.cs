using bagsisbaku.Application.Catalog.Products;
using bagsisbaku.Domain.Catalog;
using Xunit;

namespace bagsisbaku.Application.UnitTests;

public sealed class CreateProductCommandValidatorTests
{
    private readonly CreateProductCommandValidator _validator =
        new();

    [Fact]
    public async Task ValidCommandShouldPassValidation()
    {
        var result =
            await _validator.ValidateAsync(
                CreateValidCommand());

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task MissingRussianNameShouldFailValidation()
    {
        var command = CreateValidCommand() with
        {
            NameRu = string.Empty
        };

        var result =
            await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task MissingEnglishNameShouldFailValidation()
    {
        var command = CreateValidCommand() with
        {
            NameEn = string.Empty
        };

        var result =
            await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task UnknownProductTypeShouldFailValidation()
    {
        var command = CreateValidCommand() with
        {
            ProductType = 999
        };

        var result =
            await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task InvalidDiscountShouldFailValidation()
    {
        var command = CreateValidCommand() with
        {
            Price = 200,
            DiscountPrice = 250
        };

        var result =
            await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
    }

    private static CreateProductCommand CreateValidCommand()
    {
        return new CreateProductCommand(
            NameAz: "Guess kişi ayaqqabısı",
            DescriptionAz: "Premium ayaqqabı",
            NameRu: "Мужская обувь Guess",
            DescriptionRu: "Премиальная обувь",
            NameEn: "Guess men's shoes",
            DescriptionEn: "Premium shoes",
            ProductCode: "GUESS-001",
            Model: "Classic",
            Price: 200,
            DiscountPrice: 180,
            ProductType: (int)ProductType.Shoe,
            CategoryId: Guid.NewGuid(),
            BrandId: Guid.NewGuid(),
            IsFeatured: true,
            Variants:
            [
                new CreateProductVariantCommand(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    5)
            ]);
    }
}
