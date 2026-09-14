using bagsisbaku.Api.Extensions;
using bagsisbaku.Application.Baskets;
using bagsisbaku.Contracts.Baskets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bagsisbaku.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/basket")]
public sealed class BasketController(
    IBasketService basketService)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<BasketResponse>>
        GetBasketAsync(
            CancellationToken cancellationToken)
    {
        var language =
            Request.ResolveLanguage();

        var result =
            await basketService.GetBasketAsync(
                language,
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        return Ok(
            ToResponse(result.Value));
    }

    [HttpPost("items")]
    public async Task<ActionResult<BasketResponse>>
        AddItemAsync(
            [FromBody] AddBasketItemRequest request,
            CancellationToken cancellationToken)
    {
        var language =
            Request.ResolveLanguage();

        var command =
            new AddBasketItemCommand(
                request.ProductVariantId,
                request.Quantity);

        var result =
            await basketService.AddItemAsync(
                command,
                language,
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        return Ok(
            ToResponse(result.Value));
    }

    [HttpPut("items/{basketItemId:guid}")]
    public async Task<ActionResult<BasketResponse>>
        UpdateItemQuantityAsync(
            Guid basketItemId,
            [FromBody]
            UpdateBasketItemQuantityRequest request,
            CancellationToken cancellationToken)
    {
        var language =
            Request.ResolveLanguage();

        var command =
            new UpdateBasketItemQuantityCommand(
                request.Quantity);

        var result =
            await basketService.UpdateItemQuantityAsync(
                basketItemId,
                command,
                language,
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        return Ok(
            ToResponse(result.Value));
    }

    [HttpDelete("items/{basketItemId:guid}")]
    public async Task<ActionResult<BasketResponse>>
        RemoveItemAsync(
            Guid basketItemId,
            CancellationToken cancellationToken)
    {
        var language =
            Request.ResolveLanguage();

        var result =
            await basketService.RemoveItemAsync(
                basketItemId,
                language,
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        return Ok(
            ToResponse(result.Value));
    }

    [HttpDelete]
    public async Task<ActionResult<BasketResponse>>
        ClearBasketAsync(
            CancellationToken cancellationToken)
    {
        var language =
            Request.ResolveLanguage();

        var result =
            await basketService.ClearBasketAsync(
                language,
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        return Ok(
            ToResponse(result.Value));
    }

    private static BasketResponse ToResponse(
        BasketModel basket)
    {
        return new BasketResponse(
            basket.Id,
            basket.Language,
            basket.UniqueItemCount,
            basket.TotalQuantity,
            basket.Subtotal,
            basket.DiscountAmount,
            basket.Total,
            basket.CanCheckout,
            basket.Items
                .Select(ToItemResponse)
                .ToArray());
    }

    private static BasketItemResponse ToItemResponse(
        BasketItemModel item)
    {
        return new BasketItemResponse(
            item.Id,
            item.ProductId,
            item.ProductVariantId,
            item.ProductName,
            item.ProductCode,
            item.ProductType,
            item.BrandName,
            item.ImageUrl,
            item.SizeId,
            item.Size,
            item.ColorId,
            item.Color,
            item.HexCode,
            item.OriginalUnitPrice,
            item.UnitPrice,
            item.UnitDiscountAmount,
            item.Quantity,
            item.LineSubtotal,
            item.LineDiscountAmount,
            item.LineTotal,
            item.StockCount,
            item.IsAvailable,
            item.AvailabilityCode);
    }
}