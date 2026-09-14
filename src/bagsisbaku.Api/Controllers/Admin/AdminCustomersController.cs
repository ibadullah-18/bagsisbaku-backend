using bagsisbaku.Api.Authorization;
using bagsisbaku.Api.Extensions;
using bagsisbaku.Application.Customers.Administration;
using bagsisbaku.Application.Security;
using bagsisbaku.Contracts.Customers.Administration;
using Microsoft.AspNetCore.Mvc;

namespace bagsisbaku.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/customers")]
public sealed class AdminCustomersController(
    IAdminCustomerService adminCustomerService)
    : ControllerBase
{
    [HttpGet]
    [HasPermission(PermissionNames.Customers.View)]
    public async Task<ActionResult<AdminCustomerPageResponse>>
        GetAllAsync(
            [FromQuery] AdminCustomerFilterRequest request,
            CancellationToken cancellationToken)
    {
        var result =
            await adminCustomerService.GetAllAsync(
                new AdminCustomerFilter(
                    request.Search,
                    request.IsActive,
                    request.EmailConfirmed,
                    request.CreatedFromUtc,
                    request.CreatedToUtc,
                    request.Page,
                    request.PageSize),
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        return Ok(
            ToPageResponse(result.Value));
    }

    [HttpGet("{customerId:guid}")]
    [HasPermission(PermissionNames.Customers.View)]
    public async Task<ActionResult<AdminCustomerDetailsResponse>>
        GetByIdAsync(
            Guid customerId,
            CancellationToken cancellationToken)
    {
        var result =
            await adminCustomerService.GetByIdAsync(
                customerId,
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        return Ok(
            ToDetailsResponse(result.Value));
    }

    [HttpPut("{customerId:guid}/status")]
    [HasPermission(PermissionNames.Customers.View)]
    [HasPermission(PermissionNames.Customers.Manage)]
    public async Task<ActionResult<AdminCustomerDetailsResponse>>
        SetStatusAsync(
            Guid customerId,
            SetCustomerStatusRequest request,
            CancellationToken cancellationToken)
    {
        var result =
            await adminCustomerService.SetStatusAsync(
                new SetCustomerStatusCommand(
                    customerId,
                    request.IsActive),
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        return Ok(
            ToDetailsResponse(result.Value));
    }

    private static AdminCustomerPageResponse
        ToPageResponse(
            AdminCustomerPageModel page)
    {
        return new AdminCustomerPageResponse(
            page.Items
                .Select(ToSummaryResponse)
                .ToArray(),
            page.Page,
            page.PageSize,
            page.TotalCount,
            page.TotalPages);
    }

    private static AdminCustomerSummaryResponse
        ToSummaryResponse(
            AdminCustomerSummaryModel customer)
    {
        return new AdminCustomerSummaryResponse(
            customer.Id,
            customer.FullName,
            customer.Email,
            customer.PhoneNumber,
            customer.EmailConfirmed,
            customer.IsActive,
            customer.TotalOrders,
            customer.DeliveredOrders,
            customer.TotalSpent,
            customer.CreatedAtUtc,
            customer.LastLoginAtUtc);
    }

    private static AdminCustomerDetailsResponse
        ToDetailsResponse(
            AdminCustomerDetailsModel customer)
    {
        return new AdminCustomerDetailsResponse(
            customer.Id,
            customer.FullName,
            customer.Email,
            customer.PhoneNumber,
            customer.EmailConfirmed,
            customer.IsActive,
            customer.CreatedAtUtc,
            customer.UpdatedAtUtc,
            customer.LastLoginAtUtc,
            new AdminCustomerOrderStatisticsResponse(
                customer.OrderStatistics.TotalOrders,
                customer.OrderStatistics.PendingOrders,
                customer.OrderStatistics.ConfirmedOrders,
                customer.OrderStatistics.OnDeliveryOrders,
                customer.OrderStatistics.DeliveredOrders,
                customer.OrderStatistics.CancelledOrders,
                customer.OrderStatistics.TotalSpent),
            customer.Addresses
                .Select(ToAddressResponse)
                .ToArray(),
            customer.RecentOrders
                .Select(ToOrderResponse)
                .ToArray());
    }

    private static AdminCustomerAddressResponse
        ToAddressResponse(
            AdminCustomerAddressModel address)
    {
        return new AdminCustomerAddressResponse(
            address.Id,
            address.Title,
            address.RecipientFullName,
            address.PhoneNumber,
            address.City,
            address.District,
            address.AddressLine,
            address.PostalCode,
            address.DeliveryNote,
            address.Latitude,
            address.Longitude,
            address.IsDefault,
            address.CreatedAtUtc,
            address.UpdatedAtUtc);
    }

    private static AdminCustomerOrderSummaryResponse
        ToOrderResponse(
            AdminCustomerOrderSummaryModel order)
    {
        return new AdminCustomerOrderSummaryResponse(
            order.Id,
            order.OrderNumber,
            order.Status,
            order.DeliveryType,
            order.PaymentMethod,
            order.Total,
            order.PlacedAtUtc);
    }
}