using bagsisbaku.Api.Extensions;
using bagsisbaku.Application.Security;
using bagsisbaku.Api.Authorization;
using bagsisbaku.Application.Catalog.Administration;
using bagsisbaku.Application.Catalog.Administration.Models;
using bagsisbaku.Contracts.Catalog;
using bagsisbaku.Domain.Catalog;
using Microsoft.AspNetCore.Mvc;

namespace bagsisbaku.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/catalog")]
public sealed class CatalogController(
    ICatalogAdministrationService catalogService)
    : ControllerBase
{
    [HttpPost("brands")]
    [HasPermission(PermissionNames.Catalog.Manage)]
    public async Task<ActionResult<CreatedIdResponse>>
        CreateBrandAsync(
            CreateBrandRequest request,
            CancellationToken cancellationToken)
    {
        var result = await catalogService.CreateBrandAsync(
            new CreateBrandCommand(request.Name),
            cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(result.Error);
        }

        return StatusCode(
            StatusCodes.Status201Created,
            new CreatedIdResponse(result.Value));
    }

    [HttpPost("categories")]
    [HasPermission(PermissionNames.Catalog.Manage)]
    public async Task<ActionResult<CreatedIdResponse>>
        CreateCategoryAsync(
            CreateCategoryRequest request,
            CancellationToken cancellationToken)
    {
        var result =
            await catalogService.CreateCategoryAsync(
                new CreateCategoryCommand(
                    request.NameAz,
                    request.NameRu,
                    request.NameEn,
                    request.ProductType),
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(result.Error);
        }

        return StatusCode(
            StatusCodes.Status201Created,
            new CreatedIdResponse(result.Value));
    }

    [HttpPost("sizes")]
    [HasPermission(PermissionNames.Catalog.Manage)]
    public async Task<ActionResult<CreatedIdResponse>>
        CreateSizeAsync(
            CreateSizeRequest request,
            CancellationToken cancellationToken)
    {
        var result =
            await catalogService.CreateSizeAsync(
                new CreateSizeCommand(
                    request.ValueAz,
                    request.ValueRu,
                    request.ValueEn,
                    request.ProductType,
                    request.SortOrder),
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(result.Error);
        }

        return StatusCode(
            StatusCodes.Status201Created,
            new CreatedIdResponse(result.Value));
    }

    [HttpPost("colors")]
    [HasPermission(PermissionNames.Catalog.Manage)]
    public async Task<ActionResult<CreatedIdResponse>>
        CreateColorAsync(
            CreateColorRequest request,
            CancellationToken cancellationToken)
    {
        var result =
            await catalogService.CreateColorAsync(
                new CreateColorCommand(
                    request.NameAz,
                    request.NameRu,
                    request.NameEn,
                    request.HexCode),
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(result.Error);
        }

        return StatusCode(
            StatusCodes.Status201Created,
            new CreatedIdResponse(result.Value));
    }

    [HttpPut("defaults/{productType:int}")]
    [HasPermission(PermissionNames.Catalog.Manage)]
    public async Task<IActionResult> SetDefaultsAsync(
        int productType,
        SetCatalogDefaultRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await catalogService.SetDefaultsAsync(
                new SetCatalogDefaultCommand(
                    productType,
                    request.CategoryId,
                    request.SizeId),
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(result.Error);
        }

        return NoContent();
    }

    [HttpGet("options/{productType:int}")]
    [HasPermission(PermissionNames.Catalog.View)]
    public async Task<ActionResult<CatalogOptionsResponse>>
        GetOptionsAsync(
            int productType,
            CancellationToken cancellationToken)
    {
        var result = await catalogService.GetOptionsAsync(
            productType,
            cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(result.Error);
        }

        return Ok(ToResponse(result.Value));
    }

    private static CatalogOptionsResponse ToResponse(
        CatalogOptionsModel model)
    {
        var productTypeName =
            model.ProductType switch
            {
                ProductType.Shoe => "Ayaqqabı",
                ProductType.Bag => "Çanta",
                _ => "Naməlum"
            };

        return new CatalogOptionsResponse(
            (int)model.ProductType,
            productTypeName,
            model.DefaultCategoryId,
            model.DefaultSizeId,
            model.Brands
                .Select(
                    brand => new BrandOptionResponse(
                        brand.Id,
                        brand.Name,
                        brand.ImageUrl))
                .ToArray(),
            model.Categories
                .Select(
                    category => new CategoryOptionResponse(
                        category.Id,
                        category.Name,
                        category.IconUrl))
                .ToArray(),
            model.Sizes
                .Select(
                    size => new SizeOptionResponse(
                        size.Id,
                        size.Value,
                        size.SortOrder))
                .ToArray(),
            model.Colors
                .Select(
                    color => new ColorOptionResponse(
                        color.Id,
                        color.Name,
                        color.HexCode))
                .ToArray());
    }
}








