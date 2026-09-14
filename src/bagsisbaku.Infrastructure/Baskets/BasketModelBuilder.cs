using bagsisbaku.Application.Baskets;
using bagsisbaku.Domain.Baskets;
using bagsisbaku.Domain.Catalog;
using bagsisbaku.Domain.Localization;
using bagsisbaku.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace bagsisbaku.Infrastructure.Baskets;

internal sealed class BasketModelBuilder(
    ApplicationDbContext dbContext)
{
    public async Task<BasketModel> BuildAsync(
        Basket? basket,
        SupportedLanguage language,
        CancellationToken cancellationToken)
    {
        if (basket is null ||
            basket.Items.Count == 0)
        {
            return Empty(
                basket?.Id ?? Guid.Empty,
                language);
        }

        var basketItems =
            basket.Items
                .OrderBy(item => item.Id)
                .ToArray();

        var variantIds =
            basketItems
                .Select(item =>
                    item.ProductVariantId)
                .Distinct()
                .ToArray();

        var variants =
            await dbContext.ProductVariants
                .AsNoTracking()
                .Where(variant =>
                    variantIds.Contains(
                        variant.Id))
                .ToDictionaryAsync(
                    variant => variant.Id,
                    cancellationToken);

        var productIds =
            variants.Values
                .Select(variant =>
                    variant.ProductId)
                .Distinct()
                .ToArray();

        var sizeIds =
            variants.Values
                .Select(variant =>
                    variant.SizeId)
                .Distinct()
                .ToArray();

        var colorIds =
            variants.Values
                .Select(variant =>
                    variant.ColorId)
                .Distinct()
                .ToArray();

        var products =
            await dbContext.Products
                .AsNoTracking()
                .Where(product =>
                    productIds.Contains(
                        product.Id))
                .ToDictionaryAsync(
                    product => product.Id,
                    cancellationToken);

        var brandIds =
            products.Values
                .Select(product =>
                    product.BrandId)
                .Distinct()
                .ToArray();

        var brands =
            await dbContext.Brands
                .AsNoTracking()
                .Where(brand =>
                    brandIds.Contains(
                        brand.Id))
                .ToDictionaryAsync(
                    brand => brand.Id,
                    cancellationToken);

        var sizes =
            await dbContext.Sizes
                .AsNoTracking()
                .Where(size =>
                    sizeIds.Contains(
                        size.Id))
                .ToDictionaryAsync(
                    size => size.Id,
                    cancellationToken);

        var colors =
            await dbContext.Colors
                .AsNoTracking()
                .Where(color =>
                    colorIds.Contains(
                        color.Id))
                .ToDictionaryAsync(
                    color => color.Id,
                    cancellationToken);

        var productImages =
            await dbContext.ProductImages
                .AsNoTracking()
                .Where(image =>
                    productIds.Contains(
                        image.ProductId))
                .ToArrayAsync(
                    cancellationToken);

        var primaryImages =
            productImages
                .GroupBy(image =>
                    image.ProductId)
                .ToDictionary(
                    group => group.Key,
                    group =>
                        group
                            .OrderByDescending(image =>
                                image.IsPrimary)
                            .ThenBy(image =>
                                image.SortOrder)
                            .Select(image =>
                                image.ImageUrl)
                            .FirstOrDefault());

        var productTranslations =
            new Dictionary<Guid, ProductTranslation>();

        var sizeTranslations =
            new Dictionary<Guid, SizeTranslation>();

        var colorTranslations =
            new Dictionary<Guid, ColorTranslation>();

        if (language !=
            SupportedLanguage.Azerbaijani)
        {
            productTranslations =
                await dbContext.ProductTranslations
                    .AsNoTracking()
                    .Where(translation =>
                        productIds.Contains(
                            translation.ProductId) &&
                        translation.Language ==
                            language)
                    .ToDictionaryAsync(
                        translation =>
                            translation.ProductId,
                        cancellationToken);

            sizeTranslations =
                await dbContext.SizeTranslations
                    .AsNoTracking()
                    .Where(translation =>
                        sizeIds.Contains(
                            translation.SizeId) &&
                        translation.Language ==
                            language)
                    .ToDictionaryAsync(
                        translation =>
                            translation.SizeId,
                        cancellationToken);

            colorTranslations =
                await dbContext.ColorTranslations
                    .AsNoTracking()
                    .Where(translation =>
                        colorIds.Contains(
                            translation.ColorId) &&
                        translation.Language ==
                            language)
                    .ToDictionaryAsync(
                        translation =>
                            translation.ColorId,
                        cancellationToken);
        }

        var itemModels =
            new List<BasketItemModel>(
                basketItems.Length);

        foreach (var basketItem in basketItems)
        {
            if (!variants.TryGetValue(
                basketItem.ProductVariantId,
                out var variant))
            {
                itemModels.Add(
                    MissingVariant(
                        basketItem));

                continue;
            }

            products.TryGetValue(
                variant.ProductId,
                out var product);

            sizes.TryGetValue(
                variant.SizeId,
                out var size);

            colors.TryGetValue(
                variant.ColorId,
                out var color);

            var productName =
                GetProductName(
                    product,
                    productTranslations);

            var sizeValue =
                GetSizeValue(
                    size,
                    sizeTranslations);

            var colorName =
                GetColorName(
                    color,
                    colorTranslations);

            var brandName =
                product is not null &&
                brands.TryGetValue(
                    product.BrandId,
                    out var brand)
                    ? brand.Name
                    : string.Empty;

            var imageUrl =
                product is not null
                    ? primaryImages.GetValueOrDefault(
                        product.Id)
                    : null;

            var originalUnitPrice =
                product?.Price ?? 0m;

            var unitPrice =
                product?.DiscountPrice ??
                originalUnitPrice;

            var unitDiscountAmount =
                originalUnitPrice -
                unitPrice;

            var lineSubtotal =
                originalUnitPrice *
                basketItem.Quantity;

            var lineDiscountAmount =
                unitDiscountAmount *
                basketItem.Quantity;

            var lineTotal =
                unitPrice *
                basketItem.Quantity;

            var availabilityCode =
                GetAvailabilityCode(
                    product,
                    variant,
                    size,
                    color,
                    basketItem.Quantity);

            var isAvailable =
                availabilityCode ==
                "available";

            itemModels.Add(
                new BasketItemModel(
                    basketItem.Id,
                    product?.Id ??
                        variant.ProductId,
                    variant.Id,
                    productName,
                    product?.ProductCode ??
                        string.Empty,
                    product is null
                        ? 0
                        : (int)product.ProductType,
                    brandName,
                    imageUrl,
                    variant.SizeId,
                    sizeValue,
                    variant.ColorId,
                    colorName,
                    color?.HexCode,
                    originalUnitPrice,
                    unitPrice,
                    unitDiscountAmount,
                    basketItem.Quantity,
                    lineSubtotal,
                    lineDiscountAmount,
                    lineTotal,
                    variant.StockCount,
                    isAvailable,
                    availabilityCode));
        }

        var subtotal =
            itemModels.Sum(item =>
                item.LineSubtotal);

        var discountAmount =
            itemModels.Sum(item =>
                item.LineDiscountAmount);

        var total =
            itemModels.Sum(item =>
                item.LineTotal);

        var canCheckout =
            itemModels.Count > 0 &&
            itemModels.All(item =>
                item.IsAvailable);

        return new BasketModel(
            basket.Id,
            GetLanguageCode(language),
            itemModels.Count,
            itemModels.Sum(item =>
                item.Quantity),
            subtotal,
            discountAmount,
            total,
            canCheckout,
            itemModels);
    }

    private static BasketModel Empty(
        Guid basketId,
        SupportedLanguage language)
    {
        return new BasketModel(
            basketId,
            GetLanguageCode(language),
            0,
            0,
            0m,
            0m,
            0m,
            false,
            []);
    }

    private static BasketItemModel MissingVariant(
        BasketItem basketItem)
    {
        return new BasketItemModel(
            basketItem.Id,
            Guid.Empty,
            basketItem.ProductVariantId,
            string.Empty,
            string.Empty,
            0,
            string.Empty,
            null,
            Guid.Empty,
            string.Empty,
            Guid.Empty,
            string.Empty,
            null,
            0m,
            0m,
            0m,
            basketItem.Quantity,
            0m,
            0m,
            0m,
            0,
            false,
            "variant-not-found");
    }

    private static string GetProductName(
        Product? product,
        IReadOnlyDictionary<
            Guid,
            ProductTranslation> translations)
    {
        if (product is null)
        {
            return string.Empty;
        }

        return translations.TryGetValue(
            product.Id,
            out var translation)
                ? translation.Name
                : product.Name;
    }

    private static string GetSizeValue(
        Size? size,
        IReadOnlyDictionary<
            Guid,
            SizeTranslation> translations)
    {
        if (size is null)
        {
            return string.Empty;
        }

        return translations.TryGetValue(
            size.Id,
            out var translation)
                ? translation.Value
                : size.Value;
    }

    private static string GetColorName(
        Color? color,
        IReadOnlyDictionary<
            Guid,
            ColorTranslation> translations)
    {
        if (color is null)
        {
            return string.Empty;
        }

        return translations.TryGetValue(
            color.Id,
            out var translation)
                ? translation.Name
                : color.Name;
    }

    private static string GetAvailabilityCode(
        Product? product,
        ProductVariant variant,
        Size? size,
        Color? color,
        int requestedQuantity)
    {
        if (product is null ||
            !product.IsActive)
        {
            return "product-inactive";
        }

        if (!variant.IsActive ||
            size is null ||
            !size.IsActive ||
            color is null ||
            !color.IsActive)
        {
            return "variant-inactive";
        }

        if (variant.StockCount <= 0)
        {
            return "out-of-stock";
        }

        if (requestedQuantity >
            variant.StockCount)
        {
            return "insufficient-stock";
        }

        return "available";
    }

    private static string GetLanguageCode(
        SupportedLanguage language)
    {
        return language switch
        {
            SupportedLanguage.Russian => "ru",
            SupportedLanguage.English => "en",
            _ => "az"
        };
    }
}