using bagsisbaku.Application.Catalog.Products.Public;
using bagsisbaku.Contracts.Products;
using bagsisbaku.Domain.Catalog;
using bagsisbaku.Domain.Localization;
using Microsoft.AspNetCore.Mvc;

namespace bagsisbaku.Api.Controllers.Public;

[ApiController]
[Route("api/catalog/products")]
public sealed class ProductsController(
    IProductCatalogQuery productCatalogQuery)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PublicProductListResponse>>
        GetListAsync(
            [FromQuery] PublicProductListRequest request,
            CancellationToken cancellationToken)
    {
        if (!TryParseLanguage(
                request.Language,
                out var language))
        {
            ModelState.AddModelError(
                nameof(request.Language),
                "Dil az, ru və ya en olmalıdır.");

            return ValidationProblem(ModelState);
        }

        if (!TryParseSort(
                request.Sort,
                out var sort))
        {
            ModelState.AddModelError(
                nameof(request.Sort),
                "Sort newest, price-asc, price-desc və ya popular olmalıdır.");

            return ValidationProblem(ModelState);
        }

        if (request.MinPrice.HasValue &&
            request.MaxPrice.HasValue &&
            request.MinPrice.Value >
            request.MaxPrice.Value)
        {
            ModelState.AddModelError(
                nameof(request.MinPrice),
                "Minimum qiymət maksimum qiymətdən böyük ola bilməz.");

            return ValidationProblem(ModelState);
        }

        var productType = request.ProductType.HasValue
            ? (ProductType?)request.ProductType.Value
            : null;

        var filter = new ProductCatalogFilter(
            request.Search,
            productType,
            request.CategoryId,
            request.BrandId,
            request.SizeIds
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToArray(),
            request.ColorIds
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToArray(),
            request.MinPrice,
            request.MaxPrice,
            request.IsDiscounted,
            request.IsFeatured,
            sort,
            request.Page,
            request.PageSize);

        var result = await productCatalogQuery.GetListAsync(
            filter,
            language,
            cancellationToken);

        return Ok(
            new PublicProductListResponse(
                result.Language,
                result.Items
                    .Select(ToListItemResponse)
                    .ToArray(),
                result.Page,
                result.PageSize,
                result.TotalCount,
                result.TotalPages,
                result.HasPreviousPage,
                result.HasNextPage));
    }

    [HttpGet("{productId:guid}")]
    public async Task<ActionResult<PublicProductDetailsResponse>>
        GetByIdAsync(
            Guid productId,
            [FromQuery] string language = "az",
            CancellationToken cancellationToken = default)
    {
        if (!TryParseLanguage(
                language,
                out var supportedLanguage))
        {
            ModelState.AddModelError(
                nameof(language),
                "Dil az, ru və ya en olmalıdır.");

            return ValidationProblem(ModelState);
        }

        var product = await productCatalogQuery.GetByIdAsync(
            productId,
            supportedLanguage,
            cancellationToken);

        if (product is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "catalog.product-not-found",
                detail:
                    "Məhsul tapılmadı və ya satış üçün aktiv deyil.");
        }

        return Ok(ToDetailsResponse(product));
    }

    private static PublicProductListItemResponse
        ToListItemResponse(
            LocalizedProductListItemModel product)
    {
        return new PublicProductListItemResponse(
            product.Id,
            product.Name,
            product.ProductCode,
            product.Model,
            product.Price,
            product.DiscountPrice,
            product.CurrentPrice,
            product.IsDiscounted,
            product.IsFeatured,
            product.ProductType,
            product.ProductTypeName,
            new PublicBrandResponse(
                product.Brand.Id,
                product.Brand.Name,
                product.Brand.LogoUrl),
            new PublicCategoryResponse(
                product.Category.Id,
                product.Category.Name),
            product.PrimaryImageUrl,
            product.TotalStock);
    }

    private static PublicProductDetailsResponse
        ToDetailsResponse(
            LocalizedProductDetailsModel product)
    {
        return new PublicProductDetailsResponse(
            product.Id,
            product.Language,
            product.Name,
            product.Description,
            product.ProductCode,
            product.Model,
            product.Price,
            product.DiscountPrice,
            product.IsDiscounted,
            product.IsFeatured,
            product.ProductType,
            product.ProductTypeName,
            new PublicBrandResponse(
                product.Brand.Id,
                product.Brand.Name,
                product.Brand.LogoUrl),
            new PublicCategoryResponse(
                product.Category.Id,
                product.Category.Name),
            product.Images
                .Select(
                    image =>
                        new PublicProductImageResponse(
                            image.Id,
                            image.ImageUrl,
                            image.SortOrder,
                            image.IsPrimary))
                .ToArray(),
            product.Variants
                .Select(
                    variant =>
                        new PublicProductVariantResponse(
                            variant.Id,
                            variant.SizeId,
                            variant.Size,
                            variant.ColorId,
                            variant.Color,
                            variant.HexCode,
                            variant.StockCount))
                .ToArray());
    }

    private static bool TryParseLanguage(
        string? value,
        out SupportedLanguage language)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            language = SupportedLanguage.Azerbaijani;
            return true;
        }

        var code = value
            .Trim()
            .Split(
                ['-', '_'],
                StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault()
            ?.ToLowerInvariant();

        switch (code)
        {
            case "az":
                language =
                    SupportedLanguage.Azerbaijani;
                return true;

            case "ru":
                language =
                    SupportedLanguage.Russian;
                return true;

            case "en":
                language =
                    SupportedLanguage.English;
                return true;

            default:
                language =
                    SupportedLanguage.Azerbaijani;
                return false;
        }
    }

    private static bool TryParseSort(
        string? value,
        out ProductCatalogSort sort)
    {
        var normalizedValue = value?
            .Trim()
            .ToLowerInvariant()
            .Replace("-", string.Empty)
            .Replace("_", string.Empty);

        switch (normalizedValue)
        {
            case null:
            case "":
            case "newest":
                sort = ProductCatalogSort.Newest;
                return true;

            case "priceasc":
                sort =
                    ProductCatalogSort.PriceAscending;
                return true;

            case "pricedesc":
                sort =
                    ProductCatalogSort.PriceDescending;
                return true;

            case "popular":
                sort = ProductCatalogSort.Popular;
                return true;

            default:
                sort = ProductCatalogSort.Newest;
                return false;
        }
    }
}
