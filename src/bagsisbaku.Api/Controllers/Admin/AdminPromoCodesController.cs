using bagsisbaku.Api.Authorization;
using bagsisbaku.Api.Extensions;
using bagsisbaku.Application.Promotions.Administration;
using bagsisbaku.Application.Security;
using bagsisbaku.Contracts.Promotions.Administration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bagsisbaku.Api.Controllers.Admin;

[ApiController]
[Authorize]
[Route("api/admin/promotions/promo-codes")]
public sealed class AdminPromoCodesController(
    IAdminPromotionService promotionService)
    : ControllerBase
{
    [HttpGet]
    [HasPermission(
        PermissionNames.Promotions.View)]
    public async Task<
        ActionResult<AdminPromoCodePageResponse>>
        GetAllAsync(
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
    {
        var filter =
            new AdminPromoCodeFilter(
                search,
                isActive,
                page,
                pageSize);

        var result =
            await promotionService.GetAllAsync(
                filter,
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        return Ok(
            ToResponse(result.Value));
    }

    [HttpGet("{promoCodeId:guid}")]
    [HasPermission(
        PermissionNames.Promotions.View)]
    public async Task<
        ActionResult<AdminPromoCodeResponse>>
        GetByIdAsync(
            Guid promoCodeId,
            CancellationToken cancellationToken)
    {
        var result =
            await promotionService.GetByIdAsync(
                promoCodeId,
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        return Ok(
            ToResponse(result.Value));
    }

    [HttpPost]
    [HasPermission(
        PermissionNames.Promotions.Manage)]
    public async Task<
        ActionResult<AdminPromoCodeResponse>>
        CreateAsync(
            [FromBody]
            CreatePromoCodeRequest request,
            CancellationToken cancellationToken)
    {
        var command =
            new CreatePromoCodeCommand(
                request.Name,
                request.Code,
                request.DiscountType,
                request.DiscountValue,
                request.MinimumOrderAmount,
                request.MaximumDiscountAmount,
                request.UsageLimit,
                request.PerCustomerUsageLimit,
                request.StartsAtUtc,
                request.EndsAtUtc);

        var result =
            await promotionService.CreateAsync(
                command,
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        var response =
            ToResponse(result.Value);

        return CreatedAtAction(
            nameof(GetByIdAsync),
            new
            {
                promoCodeId =
                    response.Id
            },
            response);
    }

    [HttpPut("{promoCodeId:guid}")]
    [HasPermission(
        PermissionNames.Promotions.Manage)]
    public async Task<
        ActionResult<AdminPromoCodeResponse>>
        UpdateAsync(
            Guid promoCodeId,
            [FromBody]
            UpdatePromoCodeRequest request,
            CancellationToken cancellationToken)
    {
        var command =
            new UpdatePromoCodeCommand(
                request.Name,
                request.Code,
                request.DiscountType,
                request.DiscountValue,
                request.MinimumOrderAmount,
                request.MaximumDiscountAmount,
                request.UsageLimit,
                request.PerCustomerUsageLimit,
                request.StartsAtUtc,
                request.EndsAtUtc);

        var result =
            await promotionService.UpdateAsync(
                promoCodeId,
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

    [HttpPut("{promoCodeId:guid}/status")]
    [HasPermission(
        PermissionNames.Promotions.Manage)]
    public async Task<
        ActionResult<AdminPromoCodeResponse>>
        SetStatusAsync(
            Guid promoCodeId,
            [FromBody]
            SetPromoCodeStatusRequest request,
            CancellationToken cancellationToken)
    {
        var command =
            new SetPromoCodeStatusCommand(
                request.IsActive);

        var result =
            await promotionService.SetStatusAsync(
                promoCodeId,
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

    private static AdminPromoCodePageResponse
        ToResponse(
            AdminPromoCodePageModel model)
    {
        return new AdminPromoCodePageResponse(
            model.Page,
            model.PageSize,
            model.TotalCount,
            model.TotalPages,
            model.HasPreviousPage,
            model.HasNextPage,
            model.Items
                .Select(ToResponse)
                .ToArray());
    }

    private static AdminPromoCodeResponse
        ToResponse(
            AdminPromoCodeModel model)
    {
        return new AdminPromoCodeResponse(
            model.Id,
            model.Name,
            model.Code,
            model.DiscountType,
            model.DiscountTypeName,
            model.DiscountValue,
            model.MinimumOrderAmount,
            model.MaximumDiscountAmount,
            model.UsageLimit,
            model.UsageCount,
            model.RemainingUsageCount,
            model.PerCustomerUsageLimit,
            model.StartsAtUtc,
            model.EndsAtUtc,
            model.IsActive,
            model.IsCurrentlyAvailable,
            model.CreatedAtUtc,
            model.UpdatedAtUtc);
    }
}