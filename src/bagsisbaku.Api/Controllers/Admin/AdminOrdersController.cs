using bagsisbaku.Api.Authorization;
using bagsisbaku.Api.Extensions;
using bagsisbaku.Application.Orders;
using bagsisbaku.Application.Orders.Administration;
using bagsisbaku.Application.Security;
using bagsisbaku.Contracts.Orders;
using bagsisbaku.Contracts.Orders.Administration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bagsisbaku.Api.Controllers.Admin;

[ApiController]
[Authorize]
[Route("api/admin/orders")]
public sealed class AdminOrdersController(
    IAdminOrderService adminOrderService)
    : ControllerBase
{
    [HttpGet]
    [HasPermission(PermissionNames.Orders.View)]
    [ProducesResponseType(
        typeof(AdminOrderPageResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AdminOrderPageResponse>>
        GetAllAsync(
            [FromQuery] int? status,
            [FromQuery] int? deliveryType,
            [FromQuery] string? search,
            [FromQuery] DateTimeOffset? fromUtc,
            [FromQuery] DateTimeOffset? toUtc,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
    {
        var filter =
            new AdminOrderFilter(
                status,
                deliveryType,
                search,
                fromUtc,
                toUtc,
                page,
                pageSize);

        var result =
            await adminOrderService.GetAllAsync(
                filter,
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        return Ok(
            ToPageResponse(result.Value));
    }

    [HttpGet("{orderId:guid}")]
    [HasPermission(PermissionNames.Orders.View)]
    [ProducesResponseType(
        typeof(AdminOrderDetailsResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status403Forbidden)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminOrderDetailsResponse>>
        GetByIdAsync(
            Guid orderId,
            [FromQuery(Name = "lang")] string? languageCode,
            CancellationToken cancellationToken)
    {
        _ = languageCode;

        var language =
            Request.ResolveLanguage();

        var result =
            await adminOrderService.GetByIdAsync(
                orderId,
                language,
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        return Ok(
            ToDetailsResponse(result.Value));
    }

    [HttpPatch("{orderId:guid}/status")]
    [HasPermission(PermissionNames.Orders.View)]
    [HasPermission(
        PermissionNames.Orders.UpdateStatus)]
    [ProducesResponseType(
        typeof(AdminOrderDetailsResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status403Forbidden)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AdminOrderDetailsResponse>>
        ChangeStatusAsync(
            Guid orderId,
            [FromBody] ChangeOrderStatusRequest request,
            [FromQuery(Name = "lang")] string? languageCode,
            CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        _ = languageCode;

        var language =
            Request.ResolveLanguage();

        var command =
            new ChangeOrderStatusCommand(
                request.Status,
                request.Note);

        var result =
            await adminOrderService.ChangeStatusAsync(
                orderId,
                command,
                language,
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        return Ok(
            ToDetailsResponse(result.Value));
    }

    private static AdminOrderPageResponse
        ToPageResponse(
            AdminOrderPageModel model)
    {
        var items =
            model.Items
                .Select(ToSummaryResponse)
                .ToArray();

        return new AdminOrderPageResponse(
            model.Page,
            model.PageSize,
            model.TotalCount,
            model.TotalPages,
            model.HasPreviousPage,
            model.HasNextPage,
            items);
    }

    private static AdminOrderSummaryResponse
        ToSummaryResponse(
            AdminOrderSummaryModel model)
    {
        return new AdminOrderSummaryResponse(
            model.Id,
            model.CustomerId,
            model.CustomerFullName,
            model.CustomerEmail,
            model.OrderNumber,
            model.RecipientFullName,
            model.PhoneNumber,
            model.Status,
            model.DeliveryType,
            model.PaymentMethod,
            model.UniqueItemCount,
            model.TotalQuantity,
            model.Total,
            model.PlacedAtUtc,
            model.CanBeCancelled);
    }

    private static AdminOrderDetailsResponse
        ToDetailsResponse(
            AdminOrderDetailsModel model)
    {
        var items =
            model.Items
                .Select(ToItemResponse)
                .ToArray();

        var statusHistory =
            model.StatusHistory
                .Select(ToStatusHistoryResponse)
                .ToArray();

        return new AdminOrderDetailsResponse(
            model.Id,
            model.CustomerId,
            model.CustomerFullName,
            model.CustomerEmail,
            model.OrderNumber,
            model.Language,
            model.Status,
            model.DeliveryType,
            model.PaymentMethod,
            model.CustomerAddressId,
            model.RecipientFullName,
            model.PhoneNumber,
            model.City,
            model.District,
            model.AddressLine,
            model.PostalCode,
            model.DeliveryNote,
            model.CustomerNote,
            model.Subtotal,
            model.DiscountAmount,
            model.PromoCode,
            model.PromoDiscountAmount,
            model.TotalDiscountAmount,
            model.DeliveryFee,
            model.Total,
            model.PlacedAtUtc,
            model.ConfirmedAtUtc,
            model.OnDeliveryAtUtc,
            model.DeliveredAtUtc,
            model.CancelledAtUtc,
            model.CanBeCancelled,
            items,
            statusHistory);
    }

    private static PlacedOrderItemResponse
        ToItemResponse(
            PlacedOrderItemModel model)
    {
        return new PlacedOrderItemResponse(
            model.Id,
            model.ProductId,
            model.ProductVariantId,
            model.ProductName,
            model.ProductCode,
            model.ProductType,
            model.BrandName,
            model.ImageUrl,
            model.SizeId,
            model.Size,
            model.ColorId,
            model.Color,
            model.HexCode,
            model.OriginalUnitPrice,
            model.UnitPrice,
            model.UnitDiscountAmount,
            model.Quantity,
            model.LineSubtotal,
            model.LineDiscountAmount,
            model.LineTotal);
    }

    private static CustomerOrderStatusHistoryResponse
        ToStatusHistoryResponse(
            CustomerOrderStatusHistoryModel model)
    {
        return new CustomerOrderStatusHistoryResponse(
            model.Id,
            model.PreviousStatus,
            model.NewStatus,
            model.Note,
            model.ChangedAtUtc);
    }
}