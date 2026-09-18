using bagsisbaku.Api.Extensions;
using bagsisbaku.Application.Favorites;
using bagsisbaku.Contracts.Favorites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bagsisbaku.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/favorites")]
public sealed class FavoritesController(
    IFavoriteService favoriteService)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<FavoriteListResponse>>
        GetAllAsync(
            [FromQuery] string? lang,
            CancellationToken cancellationToken)
    {
        _ = lang;

        var language =
            Request.ResolveLanguage();

        var result =
            await favoriteService.GetAllAsync(
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

    [HttpPost("{productId:guid}")]
    public async Task<ActionResult<FavoriteProductResponse>>
        AddAsync(
            Guid productId,
            [FromQuery] string? lang,
            CancellationToken cancellationToken)
    {
        _ = lang;

        var language =
            Request.ResolveLanguage();

        var result =
            await favoriteService.AddAsync(
                productId,
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

    [HttpDelete("{productId:guid}")]
    public async Task<IActionResult> RemoveAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var result =
            await favoriteService.RemoveAsync(
                productId,
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        return NoContent();
    }

    private static FavoriteListResponse ToResponse(
        FavoriteListModel model)
    {
        return new FavoriteListResponse(
            model.Language,
            model.TotalCount,
            model.Items
                .Select(ToResponse)
                .ToArray());
    }

    private static FavoriteProductResponse ToResponse(
        FavoriteProductModel model)
    {
        return new FavoriteProductResponse(
            model.FavoriteId,
            model.ProductId,
            model.Name,
            model.ProductCode,
            model.Model,
            model.ProductType,
            model.BrandName,
            model.Price,
            model.DiscountPrice,
            model.CurrentPrice,
            model.IsDiscounted,
            model.ImageUrl,
            model.IsAvailable,
            model.AddedAtUtc);
    }
}