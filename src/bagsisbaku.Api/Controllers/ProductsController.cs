using bagsisbaku.Api.Extensions;
using bagsisbaku.Application.Catalog.Products;
using bagsisbaku.Application.Catalog.Products.Public;
using bagsisbaku.Contracts.Products;
using Microsoft.AspNetCore.Mvc;

namespace bagsisbaku.Api.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController(
    IProductCatalogQuery productQuery)
    : ControllerBase
{
    [HttpGet("{productId:guid}")]
    public async Task<ActionResult<PublicProductDetailsResponse>>
        GetByIdAsync(
            Guid productId,
            CancellationToken cancellationToken)
    {
        var language = Request.ResolveLanguage();

        var product = await productQuery.GetByIdAsync(
            productId,
            language,
            cancellationToken);

        if (product is null)
        {
            return this.ToProblemResult(
                ProductErrors.NotFound);
        }

        return Ok(
            new PublicProductDetailsResponse(
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
                    .ToArray()));
    }
}
