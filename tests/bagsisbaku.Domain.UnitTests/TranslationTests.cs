using bagsisbaku.Domain.Catalog;
using bagsisbaku.Domain.Common;
using bagsisbaku.Domain.Localization;
using Xunit;

namespace bagsisbaku.Domain.UnitTests;

public sealed class TranslationTests
{
    [Fact]
    public void ProductTranslationShouldCreateRussianContent()
    {
        var translation = ProductTranslation.Create(
            Guid.NewGuid(),
            SupportedLanguage.Russian,
            "Мужские кроссовки",
            "Премиальная обувь");

        Assert.Equal(
            SupportedLanguage.Russian,
            translation.Language);

        Assert.Equal(
            "Мужские кроссовки",
            translation.Name);
    }

    [Fact]
    public void ProductTranslationShouldCreateEnglishContent()
    {
        var translation = ProductTranslation.Create(
            Guid.NewGuid(),
            SupportedLanguage.English,
            "Men's sneakers",
            "Premium shoes");

        Assert.Equal(
            SupportedLanguage.English,
            translation.Language);

        Assert.Equal(
            "Men's sneakers",
            translation.Name);
    }

    [Fact]
    public void TranslationShouldRejectAzerbaijaniLanguage()
    {
        var action = () => ProductTranslation.Create(
            Guid.NewGuid(),
            SupportedLanguage.Azerbaijani,
            "Kişi ayaqqabısı",
            "Premium ayaqqabı");

        Assert.Throws<DomainException>(action);
    }

    [Fact]
    public void CategoryTranslationShouldTrimName()
    {
        var translation = CategoryTranslation.Create(
            Guid.NewGuid(),
            SupportedLanguage.English,
            "  Sneakers  ");

        Assert.Equal(
            "Sneakers",
            translation.Name);
    }

    [Fact]
    public void ColorTranslationShouldRequireName()
    {
        var action = () => ColorTranslation.Create(
            Guid.NewGuid(),
            SupportedLanguage.Russian,
            "   ");

        Assert.Throws<DomainException>(action);
    }

    [Fact]
    public void BrandShouldHaveImageButNotTranslation()
    {
        Assert.Null(
            typeof(Brand).GetProperty("Translations"));

        Assert.NotNull(
            typeof(Brand).GetProperty("ImageUrl"));

        Assert.NotNull(
            typeof(Brand).GetProperty("ImagePublicId"));
    }
}
