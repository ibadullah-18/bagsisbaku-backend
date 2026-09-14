using bagsisbaku.Api.Extensions;
using bagsisbaku.Application.Orders;
using bagsisbaku.Contracts.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bagsisbaku.Api.Controllers.Orders;

[ApiController]
[Authorize]
[Route("api/orders")]
public sealed class CustomerOrdersController(
    ICustomerOrderService customerOrderService)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<CustomerOrderPageResponse>>
        GetAllAsync(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
    {
        var result =
            await customerOrderService.GetAllAsync(
                page,
                pageSize,
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
    public async Task<ActionResult<CustomerOrderDetailsResponse>>
        GetByIdAsync(
            Guid orderId,
            [FromQuery] string? lang,
            CancellationToken cancellationToken)
    {
        _ = lang;

        var language =
            Request.ResolveLanguage();

        var result =
            await customerOrderService.GetByIdAsync(
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

    [HttpPost("{orderId:guid}/cancel")]
    public async Task<ActionResult<CustomerOrderDetailsResponse>>
        CancelAsync(
            Guid orderId,
            [FromBody] CancelOrderRequest request,
            [FromQuery] string? lang,
            CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        _ = lang;

        var language =
            Request.ResolveLanguage();

        var command =
            new CancelOrderCommand(
                request.Reason);

        var result =
            await customerOrderService.CancelAsync(
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

    private static CustomerOrderPageResponse
        ToPageResponse(
            CustomerOrderPageModel model)
    {
        var items =
            model.Items
                .Select(ToSummaryResponse)
                .ToArray();

        return new CustomerOrderPageResponse(
            model.Page,
            model.PageSize,
            model.TotalCount,
            model.TotalPages,
            model.HasPreviousPage,
            model.HasNextPage,
            items);
    }

    private static CustomerOrderSummaryResponse
        ToSummaryResponse(
            CustomerOrderSummaryModel model)
    {
        return new CustomerOrderSummaryResponse(
            model.Id,
            model.OrderNumber,
            model.Status,
            model.DeliveryType,
            model.PaymentMethod,
            model.UniqueItemCount,
            model.TotalQuantity,
            model.Total,
            model.PreviewImageUrl,
            model.PlacedAtUtc,
            model.CanBeCancelled);
    }

    private static CustomerOrderDetailsResponse
        ToDetailsResponse(
            CustomerOrderDetailsModel model)
    {
        var items =
            model.Items
                .Select(ToItemResponse)
                .ToArray();

        var statusHistory =
            model.StatusHistory
                .Select(ToStatusHistoryResponse)
                .ToArray();

        return new CustomerOrderDetailsResponse(
            model.Id,
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