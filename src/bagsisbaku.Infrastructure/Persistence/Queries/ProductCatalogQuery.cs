using bagsisbaku.Application.Catalog.Products.Public;
using bagsisbaku.Domain.Catalog;
using bagsisbaku.Domain.Localization;
using Microsoft.EntityFrameworkCore;

namespace bagsisbaku.Infrastructure.Persistence.Queries;

internal sealed class ProductCatalogQuery(
    ApplicationDbContext dbContext)
    : IProductCatalogQuery
{
    public async Task<ProductCatalogPageModel> GetListAsync(
        ProductCatalogFilter filter,
        SupportedLanguage language,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var products = dbContext.Products
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

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();

            if (language == SupportedLanguage.Azerbaijani)
            {
                products = products.Where(
                    product =>
                        product.Name.Contains(search) ||
                        product.ProductCode.Contains(search) ||
                        (
                            product.Model != null &&
                            product.Model.Contains(search)
                        ));
            }
            else
            {
                products = products.Where(
                    product =>
                        product.Name.Contains(search) ||
                        product.ProductCode.Contains(search) ||
                        (
                            product.Model != null &&
                            product.Model.Contains(search)
                        ) ||
                        dbContext.ProductTranslations.Any(
                            translation =>
                                translation.ProductId ==
                                product.Id &&
                                translation.Language ==
                                language &&
                                translation.Name.Contains(search)));
            }
        }

        if (filter.ProductType.HasValue)
        {
            products = products.Where(
                product =>
                    product.ProductType ==
                    filter.ProductType.Value);
        }

        if (filter.CategoryId.HasValue)
        {
            products = products.Where(
                product =>
                    product.CategoryId ==
                    filter.CategoryId.Value);
        }

        if (filter.BrandId.HasValue)
        {
            products = products.Where(
                product =>
                    product.BrandId ==
                    filter.BrandId.Value);
        }

        if (filter.MinPrice.HasValue)
        {
            products = products.Where(
                product =>
                    (
                        product.DiscountPrice ??
                        product.Price
                    ) >= filter.MinPrice.Value);
        }

        if (filter.MaxPrice.HasValue)
        {
            products = products.Where(
                product =>
                    (
                        product.DiscountPrice ??
                        product.Price
                    ) <= filter.MaxPrice.Value);
        }

        if (filter.IsDiscounted.HasValue)
        {
            products = filter.IsDiscounted.Value
                ? products.Where(
                    product =>
                        product.DiscountPrice.HasValue)
                : products.Where(
                    product =>
                        !product.DiscountPrice.HasValue);
        }

        if (filter.IsFeatured.HasValue)
        {
            products = products.Where(
                product =>
                    product.IsFeatured ==
                    filter.IsFeatured.Value);
        }

        var sizeIds = filter.SizeIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToArray();

        var colorIds = filter.ColorIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToArray();

        if (sizeIds.Length > 0 &&
            colorIds.Length > 0)
        {
            products = products.Where(
                product =>
                    product.Variants.Any(
                        variant =>
                            variant.IsActive &&
                            variant.StockCount > 0 &&
                            sizeIds.Contains(variant.SizeId) &&
                            colorIds.Contains(variant.ColorId) &&
                            dbContext.Sizes.Any(
                                size =>
                                    size.Id == variant.SizeId &&
                                    size.IsActive) &&
                            dbContext.Colors.Any(
                                color =>
                                    color.Id == variant.ColorId &&
                                    color.IsActive)));
        }
        else if (sizeIds.Length > 0)
        {
            products = products.Where(
                product =>
                    product.Variants.Any(
                        variant =>
                            variant.IsActive &&
                            variant.StockCount > 0 &&
                            sizeIds.Contains(variant.SizeId) &&
                            dbContext.Sizes.Any(
                                size =>
                                    size.Id == variant.SizeId &&
                                    size.IsActive)));
        }
        else if (colorIds.Length > 0)
        {
            products = products.Where(
                product =>
                    product.Variants.Any(
                        variant =>
                            variant.IsActive &&
                            variant.StockCount > 0 &&
                            colorIds.Contains(variant.ColorId) &&
                            dbContext.Colors.Any(
                                color =>
                                    color.Id == variant.ColorId &&
                                    color.IsActive)));
        }

        var totalCount = await products.CountAsync(
            cancellationToken);

        var orderedProducts = filter.Sort switch
        {
            ProductCatalogSort.PriceAscending =>
                products
                    .OrderBy(
                        product =>
                            product.DiscountPrice ??
                            product.Price)
                    .ThenByDescending(
                        product => product.CreatedAtUtc)
                    .ThenBy(product => product.Id),

            ProductCatalogSort.PriceDescending =>
                products
                    .OrderByDescending(
                        product =>
                            product.DiscountPrice ??
                            product.Price)
                    .ThenByDescending(
                        product => product.CreatedAtUtc)
                    .ThenBy(product => product.Id),

            ProductCatalogSort.Popular =>
                products
                    .OrderByDescending(
                        product => product.ViewCount)
                    .ThenByDescending(
                        product => product.CreatedAtUtc)
                    .ThenBy(product => product.Id),

            _ =>
                products
                    .OrderByDescending(
                        product => product.CreatedAtUtc)
                    .ThenBy(product => product.Id)
        };

        var pageProducts = await orderedProducts
            .Include(product => product.Images)
            .Include(product => product.Variants)
            .AsSplitQuery()
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        if (pageProducts.Count == 0)
        {
            return new ProductCatalogPageModel(
                GetLanguageCode(language),
                [],
                filter.Page,
                filter.PageSize,
                totalCount);
        }

        var productIds = pageProducts
            .Select(product => product.Id)
            .ToArray();

        var brandIds = pageProducts
            .Select(product => product.BrandId)
            .Distinct()
            .ToArray();

        var categoryIds = pageProducts
            .Select(product => product.CategoryId)
            .Distinct()
            .ToArray();

        var brands = await dbContext.Brands
            .AsNoTracking()
            .Where(
                brand =>
                    brandIds.Contains(brand.Id) &&
                    brand.IsActive)
            .ToDictionaryAsync(
                brand => brand.Id,
                cancellationToken);

        var categories = await dbContext.Categories
            .AsNoTracking()
            .Where(
                category =>
                    categoryIds.Contains(category.Id) &&
                    category.IsActive)
            .ToDictionaryAsync(
                category => category.Id,
                cancellationToken);

        var productTranslations =
            new Dictionary<Guid, ProductTranslation>();

        var categoryTranslations =
            new Dictionary<Guid, CategoryTranslation>();

        if (language != SupportedLanguage.Azerbaijani)
        {
            productTranslations =
                await dbContext.ProductTranslations
                    .AsNoTracking()
                    .Where(
                        translation =>
                            productIds.Contains(
                                translation.ProductId) &&
                            translation.Language == language)
                    .ToDictionaryAsync(
                        translation => translation.ProductId,
                        cancellationToken);

            categoryTranslations =
                await dbContext.CategoryTranslations
                    .AsNoTracking()
                    .Where(
                        translation =>
                            categoryIds.Contains(
                                translation.CategoryId) &&
                            translation.Language == language)
                    .ToDictionaryAsync(
                        translation => translation.CategoryId,
                        cancellationToken);
        }

        var variantSizeIds = pageProducts
            .SelectMany(product => product.Variants)
            .Select(variant => variant.SizeId)
            .Distinct()
            .ToArray();

        var variantColorIds = pageProducts
            .SelectMany(product => product.Variants)
            .Select(variant => variant.ColorId)
            .Distinct()
            .ToArray();

        var activeSizeIds = (
            await dbContext.Sizes
                .AsNoTracking()
                .Where(
                    size =>
                        variantSizeIds.Contains(size.Id) &&
                        size.IsActive)
                .Select(size => size.Id)
                .ToArrayAsync(cancellationToken)
        ).ToHashSet();

        var activeColorIds = (
            await dbContext.Colors
                .AsNoTracking()
                .Where(
                    color =>
                        variantColorIds.Contains(color.Id) &&
                        color.IsActive)
                .Select(color => color.Id)
                .ToArrayAsync(cancellationToken)
        ).ToHashSet();

        var items = pageProducts
            .Select(
                product =>
                {
                    var brand = brands[product.BrandId];
                    var category =
                        categories[product.CategoryId];

                    var productName =
                        productTranslations
                            .GetValueOrDefault(product.Id)
                            ?.Name ??
                        product.Name;

                    var categoryName =
                        categoryTranslations
                            .GetValueOrDefault(category.Id)
                            ?.Name ??
                        category.Name;

                    var primaryImageUrl =
                        product.Images
                            .OrderByDescending(
                                image => image.IsPrimary)
                            .ThenBy(
                                image => image.SortOrder)
                            .Select(image => image.ImageUrl)
                            .FirstOrDefault();

                    var totalStock = product.Variants
                        .Where(
                            variant =>
                                variant.IsActive &&
                                variant.StockCount > 0 &&
                                activeSizeIds.Contains(
                                    variant.SizeId) &&
                                activeColorIds.Contains(
                                    variant.ColorId))
                        .Sum(
                            variant =>
                                (long)variant.StockCount);

                    return new LocalizedProductListItemModel(
                        product.Id,
                        productName,
                        product.ProductCode,
                        product.Model,
                        product.Price,
                        product.DiscountPrice,
                        product.DiscountPrice ??
                        product.Price,
                        product.IsDiscounted,
                        product.IsFeatured,
                        (int)product.ProductType,
                        GetProductTypeName(
                            product.ProductType,
                            language),
                        new BrandSummaryModel(
                            brand.Id,
                            brand.Name,
                            brand.ImageUrl),
                        new CategorySummaryModel(
                            category.Id,
                            categoryName),
                        primaryImageUrl,
                        totalStock);
                })
            .ToArray();

        return new ProductCatalogPageModel(
            GetLanguageCode(language),
            items,
            filter.Page,
            filter.PageSize,
            totalCount);
    }

    public async Task<LocalizedProductDetailsModel?> GetByIdAsync(
        Guid productId,
        SupportedLanguage language,
        CancellationToken cancellationToken = default)
    {
        var product = await dbContext.Products
            .AsNoTracking()
            .Include(currentProduct => currentProduct.Images)
            .Include(currentProduct => currentProduct.Variants)
            .AsSplitQuery()
            .SingleOrDefaultAsync(
                currentProduct =>
                    currentProduct.Id == productId &&
                    currentProduct.IsActive,
                cancellationToken);

        if (product is null)
        {
            return null;
        }

        var brand = await dbContext.Brands
            .AsNoTracking()
            .SingleOrDefaultAsync(
                currentBrand =>
                    currentBrand.Id == product.BrandId &&
                    currentBrand.IsActive,
                cancellationToken);

        var category = await dbContext.Categories
            .AsNoTracking()
            .SingleOrDefaultAsync(
                currentCategory =>
                    currentCategory.Id ==
                    product.CategoryId &&
                    currentCategory.IsActive,
                cancellationToken);

        if (brand is null || category is null)
        {
            return null;
        }

        var candidateVariants = product.Variants
            .Where(
                variant =>
                    variant.IsActive &&
                    variant.StockCount > 0)
            .ToArray();

        if (candidateVariants.Length == 0)
        {
            return null;
        }

        var sizeIds = candidateVariants
            .Select(variant => variant.SizeId)
            .Distinct()
            .ToArray();

        var colorIds = candidateVariants
            .Select(variant => variant.ColorId)
            .Distinct()
            .ToArray();

        var sizes = await dbContext.Sizes
            .AsNoTracking()
            .Where(
                size =>
                    sizeIds.Contains(size.Id) &&
                    size.IsActive)
            .ToDictionaryAsync(
                size => size.Id,
                cancellationToken);

        var colors = await dbContext.Colors
            .AsNoTracking()
            .Where(
                color =>
                    colorIds.Contains(color.Id) &&
                    color.IsActive)
            .ToDictionaryAsync(
                color => color.Id,
                cancellationToken);

        var activeVariants = candidateVariants
            .Where(
                variant =>
                    sizes.ContainsKey(variant.SizeId) &&
                    colors.ContainsKey(variant.ColorId))
            .ToArray();

        if (activeVariants.Length == 0)
        {
            return null;
        }

        var productName = product.Name;
        var productDescription = product.Description;
        var categoryName = category.Name;

        var sizeTranslations =
            new Dictionary<Guid, string>();

        var colorTranslations =
            new Dictionary<Guid, string>();

        if (language != SupportedLanguage.Azerbaijani)
        {
            var productTranslation =
                await dbContext.ProductTranslations
                    .AsNoTracking()
                    .SingleOrDefaultAsync(
                        translation =>
                            translation.ProductId ==
                            product.Id &&
                            translation.Language == language,
                        cancellationToken);

            if (productTranslation is not null)
            {
                productName = productTranslation.Name;

                productDescription =
                    productTranslation.Description ??
                    product.Description;
            }

            var categoryTranslation =
                await dbContext.CategoryTranslations
                    .AsNoTracking()
                    .SingleOrDefaultAsync(
                        translation =>
                            translation.CategoryId ==
                            category.Id &&
                            translation.Language == language,
                        cancellationToken);

            if (categoryTranslation is not null)
            {
                categoryName = categoryTranslation.Name;
            }

            sizeTranslations =
                await dbContext.SizeTranslations
                    .AsNoTracking()
                    .Where(
                        translation =>
                            sizeIds.Contains(
                                translation.SizeId) &&
                            translation.Language == language)
                    .ToDictionaryAsync(
                        translation => translation.SizeId,
                        translation => translation.Value,
                        cancellationToken);

            colorTranslations =
                await dbContext.ColorTranslations
                    .AsNoTracking()
                    .Where(
                        translation =>
                            colorIds.Contains(
                                translation.ColorId) &&
                            translation.Language == language)
                    .ToDictionaryAsync(
                        translation => translation.ColorId,
                        translation => translation.Name,
                        cancellationToken);
        }

        var variants = activeVariants
            .Select(
                variant =>
                {
                    var size = sizes[variant.SizeId];
                    var color = colors[variant.ColorId];

                    var sizeValue =
                        sizeTranslations.GetValueOrDefault(
                            size.Id) ??
                        size.Value;

                    var colorName =
                        colorTranslations.GetValueOrDefault(
                            color.Id) ??
                        color.Name;

                    return new PublicProductVariantModel(
                        variant.Id,
                        size.Id,
                        sizeValue,
                        color.Id,
                        colorName,
                        color.HexCode,
                        variant.StockCount);
                })
            .ToArray();

        return new LocalizedProductDetailsModel(
            product.Id,
            GetLanguageCode(language),
            productName,
            productDescription,
            product.ProductCode,
            product.Model,
            product.Price,
            product.DiscountPrice,
            product.IsDiscounted,
            product.IsFeatured,
            (int)product.ProductType,
            GetProductTypeName(
                product.ProductType,
                language),
            new BrandSummaryModel(
                brand.Id,
                brand.Name,
                brand.ImageUrl),
            new CategorySummaryModel(
                category.Id,
                categoryName),
            product.Images
                .OrderBy(image => image.SortOrder)
                .Select(
                    image => new PublicProductImageModel(
                        image.Id,
                        image.ImageUrl,
                        image.SortOrder,
                        image.IsPrimary))
                .ToArray(),
            variants);
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

    private static string GetProductTypeName(
        ProductType productType,
        SupportedLanguage language)
    {
        return (productType, language) switch
        {
            (
                ProductType.Shoe,
                SupportedLanguage.Russian
            ) => "Обувь",

            (
                ProductType.Shoe,
                SupportedLanguage.English
            ) => "Shoes",

            (
                ProductType.Bag,
                SupportedLanguage.Russian
            ) => "Сумка",

            (
                ProductType.Bag,
                SupportedLanguage.English
            ) => "Bag",

            (
                ProductType.Bag,
                _
            ) => "Çanta",

            _ => "Ayaqqabı"
        };
    }
}
