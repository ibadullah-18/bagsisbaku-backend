using bagsisbaku.Application.Catalog.Public;
using bagsisbaku.Domain.Catalog;
using bagsisbaku.Domain.Localization;
using Microsoft.EntityFrameworkCore;

namespace bagsisbaku.Infrastructure.Persistence.Queries;

internal sealed class PublicCatalogFilterQuery(
    ApplicationDbContext dbContext)
    : IPublicCatalogFilterQuery
{
    public async Task<PublicCatalogFilterModel> GetAsync(
        ProductType? productType,
        SupportedLanguage language,
        CancellationToken cancellationToken = default)
    {
        var eligibleProducts = dbContext.Products
            .AsNoTracking()
            .Where(
                product =>
                    product.IsActive &&
                    dbContext.Brands.Any(
                        brand =>
                            brand.Id == product.BrandId &&
                            brand.IsActive) &&
                    dbContext.Categories.Any(
                        category =>
                            category.Id == product.CategoryId &&
                            category.IsActive) &&
                    product.Variants.Any(
                        variant =>
                            variant.IsActive &&
                            variant.StockCount > 0 &&
                            dbContext.Sizes.Any(
                                size =>
                                    size.Id == variant.SizeId &&
                                    size.IsActive) &&
                            dbContext.Colors.Any(
                                color =>
                                    color.Id == variant.ColorId &&
                                    color.IsActive)));

        if (productType.HasValue)
        {
            eligibleProducts = eligibleProducts.Where(
                product =>
                    product.ProductType ==
                    productType.Value);
        }

        var productCount =
            await eligibleProducts.CountAsync(
                cancellationToken);

        var priceRange = await eligibleProducts
            .GroupBy(_ => 1)
            .Select(
                group => new
                {
                    MinimumPrice = group.Min(
                        product =>
                            product.DiscountPrice ??
                            product.Price),

                    MaximumPrice = group.Max(
                        product =>
                            product.DiscountPrice ??
                            product.Price)
                })
            .SingleOrDefaultAsync(cancellationToken);

        var brandData = await dbContext.Brands
            .AsNoTracking()
            .Where(
                brand =>
                    brand.IsActive &&
                    eligibleProducts.Any(
                        product =>
                            product.BrandId == brand.Id))
            .OrderBy(brand => brand.Name)
            .Select(
                brand => new
                {
                    brand.Id,
                    brand.Name,
                    brand.ImageUrl,

                    ProductCount =
                        eligibleProducts.Count(
                            product =>
                                product.BrandId ==
                                brand.Id)
                })
            .ToArrayAsync(cancellationToken);

        var brands = brandData
            .Select(
                brand =>
                    new PublicBrandFilterModel(
                        brand.Id,
                        brand.Name,
                        brand.ImageUrl,
                        brand.ProductCount))
            .ToArray();

        var categoryData = await dbContext.Categories
            .AsNoTracking()
            .Where(
                category =>
                    category.IsActive &&
                    eligibleProducts.Any(
                        product =>
                            product.CategoryId ==
                            category.Id))
            .Select(
                category => new
                {
                    category.Id,
                    category.Name,
                    category.IconUrl,
                    category.ProductType,

                    ProductCount =
                        eligibleProducts.Count(
                            product =>
                                product.CategoryId ==
                                category.Id)
                })
            .ToArrayAsync(cancellationToken);

        var colorData = await dbContext.Colors
            .AsNoTracking()
            .Where(
                color =>
                    color.IsActive &&
                    eligibleProducts.Any(
                        product =>
                            product.Variants.Any(
                                variant =>
                                    variant.IsActive &&
                                    variant.StockCount > 0 &&
                                    variant.ColorId ==
                                    color.Id &&
                                    dbContext.Sizes.Any(
                                        size =>
                                            size.Id ==
                                            variant.SizeId &&
                                            size.IsActive))))
            .Select(
                color => new
                {
                    color.Id,
                    color.Name,
                    color.HexCode,

                    ProductCount =
                        eligibleProducts.Count(
                            product =>
                                product.Variants.Any(
                                    variant =>
                                        variant.IsActive &&
                                        variant.StockCount > 0 &&
                                        variant.ColorId ==
                                        color.Id &&
                                        dbContext.Sizes.Any(
                                            size =>
                                                size.Id ==
                                                variant.SizeId &&
                                                size.IsActive)))
                })
            .ToArrayAsync(cancellationToken);

        var sizeData = await dbContext.Sizes
            .AsNoTracking()
            .Where(
                size =>
                    size.IsActive &&
                    (
                        !productType.HasValue ||
                        size.ProductType ==
                        productType.Value
                    ) &&
                    eligibleProducts.Any(
                        product =>
                            product.Variants.Any(
                                variant =>
                                    variant.IsActive &&
                                    variant.StockCount > 0 &&
                                    variant.SizeId ==
                                    size.Id &&
                                    dbContext.Colors.Any(
                                        color =>
                                            color.Id ==
                                            variant.ColorId &&
                                            color.IsActive))))
            .Select(
                size => new
                {
                    size.Id,
                    size.Value,
                    size.ProductType,
                    size.SortOrder,

                    ProductCount =
                        eligibleProducts.Count(
                            product =>
                                product.Variants.Any(
                                    variant =>
                                        variant.IsActive &&
                                        variant.StockCount > 0 &&
                                        variant.SizeId ==
                                        size.Id &&
                                        dbContext.Colors.Any(
                                            color =>
                                                color.Id ==
                                                variant.ColorId &&
                                                color.IsActive)))
                })
            .ToArrayAsync(cancellationToken);

        var categoryTranslations =
            new Dictionary<Guid, string>();

        var colorTranslations =
            new Dictionary<Guid, string>();

        var sizeTranslations =
            new Dictionary<Guid, string>();

        if (language != SupportedLanguage.Azerbaijani)
        {
            var categoryIds = categoryData
                .Select(category => category.Id)
                .ToArray();

            var colorIds = colorData
                .Select(color => color.Id)
                .ToArray();

            var sizeIds = sizeData
                .Select(size => size.Id)
                .ToArray();

            categoryTranslations =
                await dbContext.CategoryTranslations
                    .AsNoTracking()
                    .Where(
                        translation =>
                            categoryIds.Contains(
                                translation.CategoryId) &&
                            translation.Language ==
                            language)
                    .ToDictionaryAsync(
                        translation =>
                            translation.CategoryId,
                        translation => translation.Name,
                        cancellationToken);

            colorTranslations =
                await dbContext.ColorTranslations
                    .AsNoTracking()
                    .Where(
                        translation =>
                            colorIds.Contains(
                                translation.ColorId) &&
                            translation.Language ==
                            language)
                    .ToDictionaryAsync(
                        translation =>
                            translation.ColorId,
                        translation => translation.Name,
                        cancellationToken);

            sizeTranslations =
                await dbContext.SizeTranslations
                    .AsNoTracking()
                    .Where(
                        translation =>
                            sizeIds.Contains(
                                translation.SizeId) &&
                            translation.Language ==
                            language)
                    .ToDictionaryAsync(
                        translation =>
                            translation.SizeId,
                        translation => translation.Value,
                        cancellationToken);
        }

        var categories = categoryData
            .Select(
                category =>
                    new PublicCategoryFilterModel(
                        category.Id,
                        categoryTranslations
                            .GetValueOrDefault(category.Id) ??
                        category.Name,
                        category.IconUrl,
                        (int)category.ProductType,
                        category.ProductCount))
            .OrderBy(
                category => category.Name,
                StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var colors = colorData
            .Select(
                color =>
                    new PublicColorFilterModel(
                        color.Id,
                        colorTranslations
                            .GetValueOrDefault(color.Id) ??
                        color.Name,
                        color.HexCode,
                        color.ProductCount))
            .OrderBy(
                color => color.Name,
                StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var sizes = sizeData
            .Select(
                size =>
                    new PublicSizeFilterModel(
                        size.Id,
                        sizeTranslations
                            .GetValueOrDefault(size.Id) ??
                        size.Value,
                        (int)size.ProductType,
                        size.SortOrder,
                        size.ProductCount))
            .OrderBy(size => size.ProductType)
            .ThenBy(size => size.SortOrder)
            .ThenBy(
                size => size.Value,
                StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new PublicCatalogFilterModel(
            GetLanguageCode(language),
            productType.HasValue
                ? (int)productType.Value
                : null,
            productCount,
            priceRange?.MinimumPrice,
            priceRange?.MaximumPrice,
            brands,
            categories,
            colors,
            sizes);
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

