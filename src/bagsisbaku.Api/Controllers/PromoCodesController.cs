using bagsisbaku.Api.Extensions;
using bagsisbaku.Application.Promotions.Customer;
using bagsisbaku.Contracts.Promotions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bagsisbaku.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/promotions")]
public sealed class PromoCodesController(
    IPromoCodeValidationService promoCodeService)
    : ControllerBase
{
    [HttpPost("validate")]
    public async Task<
        ActionResult<PromoCodePreviewResponse>>
        ValidateAsync(
            [FromBody]
            ValidatePromoCodeRequest request,
            CancellationToken cancellationToken)
    {
        var command =
            new ValidatePromoCodeCommand(
                request.Code);

        var result =
            await promoCodeService.ValidateAsync(
                command,
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        return Ok(
            ToResponse(result.Value));
    }

    private static PromoCodePreviewResponse
        ToResponse(
            PromoCodePreviewModel model)
    {
        return new PromoCodePreviewResponse(
            model.PromoCodeId,
            model.Code,
            model.DiscountType,
            model.DiscountTypeName,
            model.DiscountValue,
            model.MinimumOrderAmount,
            model.MaximumDiscountAmount,
            model.BasketTotal,
            model.DiscountAmount,
            model.TotalAfterDiscount,
            model.StartsAtUtc,
            model.EndsAtUtc);
    }
}