using bagsisbaku.Api.Extensions;
using bagsisbaku.Application.Security;
using bagsisbaku.Api.Authorization;
using bagsisbaku.Application.Catalog.Products;
using bagsisbaku.Application.Catalog.Products.Models;
using bagsisbaku.Contracts.Products;
using bagsisbaku.Domain.Catalog;
using Microsoft.AspNetCore.Mvc;

namespace bagsisbaku.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/products")]
public sealed class ProductsController(
    IProductAdministrationService productService)
    : ControllerBase
{
    private const string GetProductByIdRouteName =
        "AdminProducts.GetById";

    [HttpPost]
    [HasPermission(PermissionNames.Products.Create)]
    public async Task<ActionResult<CreateProductResponse>>
        CreateAsync(
            CreateProductRequest request,
            CancellationToken cancellationToken)
    {
        var command = new CreateProductCommand(
            request.NameAz,
            request.DescriptionAz,
            request.NameRu,
            request.DescriptionRu,
            request.NameEn,
            request.DescriptionEn,
            request.ProductCode,
            request.Model,
            request.Price,
            request.DiscountPrice,
            request.ProductType,
            request.CategoryId,
            request.BrandId,
            request.IsFeatured,
            request.Variants
                .Select(
                    variant =>
                        new CreateProductVariantCommand(
                            variant.SizeId,
                            variant.ColorId,
                            variant.StockCount))
                .ToArray());

        var result = await productService.CreateAsync(
            command,
            cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(result.Error);
        }

        var response =
            new CreateProductResponse(result.Value);

        return CreatedAtRoute(
            GetProductByIdRouteName,
            new
            {
                productId = result.Value
            },
            response);
    }

    [HttpGet(
        "{productId:guid}",
        Name = GetProductByIdRouteName)]
    [HasPermission(PermissionNames.Products.View)]
    public async Task<ActionResult<ProductDetailsResponse>>
        GetByIdAsync(
            Guid productId,
            CancellationToken cancellationToken)
    {
        var result = await productService.GetByIdAsync(
            productId,
            cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(result.Error);
        }

        return Ok(ToResponse(result.Value));
    }

    private static ProductDetailsResponse ToResponse(
        ProductDetailsModel product)
    {
        var productTypeName =
            product.ProductType switch
            {
                ProductType.Shoe => "Ayaqqabı",
                ProductType.Bag => "Çanta",
                _ => "Naməlum"
            };

        return new ProductDetailsResponse(
            product.Id,
            product.Name,
            product.Description,
            product.ProductCode,
            product.Model,
            product.Price,
            product.DiscountPrice,
            product.IsDiscounted,
            product.IsFeatured,
            product.IsActive,
            product.ViewCount,
            (int)product.ProductType,
            productTypeName,
            product.CategoryId,
            product.BrandId,
            product.CreatedAtUtc,
            product.UpdatedAtUtc,
            product.Images
                .Select(
                    image => new ProductImageResponse(
                        image.Id,
                        image.ImageUrl,
                        image.SortOrder,
                        image.IsPrimary))
                .ToArray(),
            product.Variants
                .Select(
                    variant => new ProductVariantResponse(
                        variant.Id,
                        variant.SizeId,
                        variant.ColorId,
                        variant.StockCount,
                        variant.IsActive))
                .ToArray());
    }
}




