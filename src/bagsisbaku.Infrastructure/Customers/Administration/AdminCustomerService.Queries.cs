using bagsisbaku.Application.Common.Results;
using bagsisbaku.Application.Customers.Administration;
using bagsisbaku.Application.Security;
using bagsisbaku.Domain.Orders;
using bagsisbaku.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace bagsisbaku.Infrastructure.Customers.Administration;

internal sealed partial class AdminCustomerService
{
    public async Task<Result<AdminCustomerPageModel>>
        GetAllAsync(
            AdminCustomerFilter filter,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var validationResult =
            await _filterValidator.ValidateAsync(
                filter,
                cancellationToken);

        if (!validationResult.IsValid)
        {
            return Result.Failure<AdminCustomerPageModel>(
                AdminCustomerErrors.Validation(
                    CreateValidationMessage(
                        validationResult.Errors)));
        }

        var authorizationResult =
            await GetAuthorizedAdminAsync(
                PermissionNames.Customers.View,
                cancellationToken);

        if (authorizationResult.IsFailure)
        {
            return Result.Failure<AdminCustomerPageModel>(
                authorizationResult.Error);
        }

        var query =
            CustomerUsersQuery(
                asNoTracking: true);

        if (filter.IsActive.HasValue)
        {
            query =
                query.Where(customer =>
                    customer.IsActive ==
                    filter.IsActive.Value);
        }

        if (filter.EmailConfirmed.HasValue)
        {
            query =
                query.Where(customer =>
                    customer.EmailConfirmed ==
                    filter.EmailConfirmed.Value);
        }

        if (filter.CreatedFromUtc.HasValue)
        {
            query =
                query.Where(customer =>
                    customer.CreatedAtUtc >=
                    filter.CreatedFromUtc.Value);
        }

        if (filter.CreatedToUtc.HasValue)
        {
            query =
                query.Where(customer =>
                    customer.CreatedAtUtc <=
                    filter.CreatedToUtc.Value);
        }

        if (!string.IsNullOrWhiteSpace(
            filter.Search))
        {
            var search =
                filter.Search.Trim();

            query =
                query.Where(customer =>
                    customer.FullName.Contains(search) ||
                    (
                        customer.Email != null &&
                        customer.Email.Contains(search)
                    ) ||
                    (
                        customer.PhoneNumber != null &&
                        customer.PhoneNumber.Contains(search)
                    ));
        }

        var totalCount =
            await query.CountAsync(
                cancellationToken);

        var skipCount =
            ((long)filter.Page - 1L) *
            filter.PageSize;

        AppUser[] customers;

        if (skipCount > int.MaxValue)
        {
            customers = [];
        }
        else
        {
            customers =
                await query
                    .OrderByDescending(customer =>
                        customer.CreatedAtUtc)
                    .ThenBy(customer =>
                        customer.Id)
                    .Skip((int)skipCount)
                    .Take(filter.PageSize)
                    .ToArrayAsync(
                        cancellationToken);
        }

        var customerIds =
            customers
                .Select(customer => customer.Id)
                .ToArray();

        var statisticsByCustomerId =
            new Dictionary<
                Guid,
                CustomerPageOrderStatistics>();

        if (customerIds.Length > 0)
        {
            var statistics =
                await _dbContext.Orders
                    .AsNoTracking()
                    .Where(order =>
                        customerIds.Contains(
                            order.UserId))
                    .GroupBy(order =>
                        order.UserId)
                    .Select(group =>
                        new CustomerPageOrderStatistics(
                            group.Key,
                            group.Count(),
                            group.Count(order =>
                                order.Status ==
                                OrderStatus.Delivered),
                            group
                                .Where(order =>
                                    order.Status ==
                                    OrderStatus.Delivered)
                                .Sum(order =>
                                    (decimal?)order.Total)
                                ?? 0m))
                    .ToArrayAsync(
                        cancellationToken);

            statisticsByCustomerId =
                statistics.ToDictionary(
                    statisticsItem =>
                        statisticsItem.CustomerId);
        }

        var items =
            customers
                .Select(customer =>
                {
                    statisticsByCustomerId.TryGetValue(
                        customer.Id,
                        out var statistics);

                    return new AdminCustomerSummaryModel(
                        customer.Id,
                        customer.FullName,
                        customer.Email ?? string.Empty,
                        customer.PhoneNumber,
                        customer.EmailConfirmed,
                        customer.IsActive,
                        statistics?.TotalOrders ?? 0,
                        statistics?.DeliveredOrders ?? 0,
                        statistics?.TotalSpent ?? 0m,
                        customer.CreatedAtUtc,
                        customer.LastLoginAtUtc);
                })
                .ToArray();

        var totalPages =
            totalCount == 0
                ? 0
                : (int)Math.Ceiling(
                    totalCount /
                    (double)filter.PageSize);

        return Result.Success(
            new AdminCustomerPageModel(
                items,
                filter.Page,
                filter.PageSize,
                totalCount,
                totalPages));
    }

    public async Task<Result<AdminCustomerDetailsModel>>
        GetByIdAsync(
            Guid customerId,
            CancellationToken cancellationToken = default)
    {
        if (customerId == Guid.Empty)
        {
            return Result.Failure<
                AdminCustomerDetailsModel>(
                    AdminCustomerErrors.CustomerNotFound);
        }

        var authorizationResult =
            await GetAuthorizedAdminAsync(
                PermissionNames.Customers.View,
                cancellationToken);

        if (authorizationResult.IsFailure)
        {
            return Result.Failure<
                AdminCustomerDetailsModel>(
                    authorizationResult.Error);
        }

        return await GetDetailsCoreAsync(
            customerId,
            cancellationToken);
    }

    private async Task<Result<AdminCustomerDetailsModel>>
        GetDetailsCoreAsync(
            Guid customerId,
            CancellationToken cancellationToken)
    {
        var customer =
            await CustomerUsersQuery(
                    asNoTracking: true)
                .SingleOrDefaultAsync(
                    currentCustomer =>
                        currentCustomer.Id ==
                        customerId,
                    cancellationToken);

        if (customer is null)
        {
            return Result.Failure<
                AdminCustomerDetailsModel>(
                    AdminCustomerErrors
                        .CustomerNotFound);
        }

        var addresses =
            await _dbContext.CustomerAddresses
                .AsNoTracking()
                .Where(address =>
                    address.UserId == customerId)
                .OrderByDescending(address =>
                    address.IsDefault)
                .ThenByDescending(address =>
                    address.CreatedAtUtc)
                .Select(address =>
                    new AdminCustomerAddressModel(
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
                        address.UpdatedAtUtc))
                .ToArrayAsync(
                    cancellationToken);

        var orderStatistics =
            await _dbContext.Orders
                .AsNoTracking()
                .Where(order =>
                    order.UserId == customerId)
                .GroupBy(order =>
                    order.UserId)
                .Select(group =>
                    new CustomerOrderStatisticsProjection(
                        group.Count(),
                        group.Count(order =>
                            order.Status ==
                            OrderStatus.Pending),
                        group.Count(order =>
                            order.Status ==
                            OrderStatus.Confirmed),
                        group.Count(order =>
                            order.Status ==
                            OrderStatus.OnDelivery),
                        group.Count(order =>
                            order.Status ==
                            OrderStatus.Delivered),
                        group.Count(order =>
                            order.Status ==
                            OrderStatus.Cancelled),
                        group
                            .Where(order =>
                                order.Status ==
                                OrderStatus.Delivered)
                            .Sum(order =>
                                (decimal?)order.Total)
                            ?? 0m))
                .SingleOrDefaultAsync(
                    cancellationToken);

        var recentOrders =
            await _dbContext.Orders
                .AsNoTracking()
                .Where(order =>
                    order.UserId == customerId)
                .OrderByDescending(order =>
                    order.PlacedAtUtc)
                .ThenByDescending(order =>
                    order.Id)
                .Take(10)
                .Select(order =>
                    new AdminCustomerOrderSummaryModel(
                        order.Id,
                        order.OrderNumber,
                        (int)order.Status,
                        (int)order.DeliveryType,
                        (int)order.PaymentMethod,
                        order.Total,
                        order.PlacedAtUtc))
                .ToArrayAsync(
                    cancellationToken);

        var statisticsModel =
            orderStatistics is null
                ? new AdminCustomerOrderStatisticsModel(
                    TotalOrders: 0,
                    PendingOrders: 0,
                    ConfirmedOrders: 0,
                    OnDeliveryOrders: 0,
                    DeliveredOrders: 0,
                    CancelledOrders: 0,
                    TotalSpent: 0m)
                : new AdminCustomerOrderStatisticsModel(
                    orderStatistics.TotalOrders,
                    orderStatistics.PendingOrders,
                    orderStatistics.ConfirmedOrders,
                    orderStatistics.OnDeliveryOrders,
                    orderStatistics.DeliveredOrders,
                    orderStatistics.CancelledOrders,
                    orderStatistics.TotalSpent);

        return Result.Success(
            new AdminCustomerDetailsModel(
                customer.Id,
                customer.FullName,
                customer.Email ?? string.Empty,
                customer.PhoneNumber,
                customer.EmailConfirmed,
                customer.IsActive,
                customer.CreatedAtUtc,
                customer.UpdatedAtUtc,
                customer.LastLoginAtUtc,
                statisticsModel,
                addresses,
                recentOrders));
    }

    private sealed record CustomerPageOrderStatistics(
        Guid CustomerId,
        int TotalOrders,
        int DeliveredOrders,
        decimal TotalSpent);

    private sealed record CustomerOrderStatisticsProjection(
        int TotalOrders,
        int PendingOrders,
        int ConfirmedOrders,
        int OnDeliveryOrders,
        int DeliveredOrders,
        int CancelledOrders,
        decimal TotalSpent);
}