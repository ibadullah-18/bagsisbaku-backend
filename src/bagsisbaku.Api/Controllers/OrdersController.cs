using bagsisbaku.Api.Extensions;
using bagsisbaku.Application.Orders;
using bagsisbaku.Contracts.Orders;
using bagsisbaku.Domain.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bagsisbaku.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/orders")]
public sealed class OrdersController(
    ICheckoutService checkoutService)
    : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(
        typeof(PlacedOrderResponse),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PlacedOrderResponse>>
        PlaceOrderAsync(
            [FromBody] PlaceOrderRequest request,
            [FromQuery(Name = "lang")] string? languageCode,
            CancellationToken cancellationToken)
    {
        _ = languageCode;

        var language =
            Request.ResolveLanguage();

        var command =
            new PlaceOrderCommand(
                (DeliveryType)request.DeliveryType,
                request.CustomerAddressId,
                request.PickupRecipientFullName,
                request.PickupPhoneNumber,
                request.CustomerNote,
                request.PromoCode);

        var result =
            await checkoutService.PlaceOrderAsync(
                command,
                language,
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        var response =
            ToResponse(result.Value);

        return Created(
            $"/api/orders/{response.Id}",
            response);
    }

    private static PlacedOrderResponse ToResponse(
        PlacedOrderModel order)
    {
        return new PlacedOrderResponse(
            order.Id,
            order.OrderNumber,
            order.Language,
            order.Status,
            order.DeliveryType,
            order.PaymentMethod,
            order.CustomerAddressId,
            order.RecipientFullName,
            order.PhoneNumber,
            order.City,
            order.District,
            order.AddressLine,
            order.PostalCode,
            order.DeliveryNote,
            order.CustomerNote,
            order.Subtotal,
            order.DiscountAmount,
            order.PromoCode,
            order.PromoDiscountAmount,
            order.TotalDiscountAmount,
            order.DeliveryFee,
            order.Total,
            order.PlacedAtUtc,
            order.CanBeCancelled,
            order.Items
                .Select(ToItemResponse)
                .ToArray());
    }

    private static PlacedOrderItemResponse
        ToItemResponse(
            PlacedOrderItemModel item)
    {
        return new PlacedOrderItemResponse(
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
            item.LineTotal);
    }
}