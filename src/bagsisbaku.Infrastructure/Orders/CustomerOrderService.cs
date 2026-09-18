using System.Data;
using bagsisbaku.Application.Abstractions.Authentication;
using bagsisbaku.Application.Abstractions.Time;
using bagsisbaku.Application.Common.Results;
using bagsisbaku.Application.Orders;
using bagsisbaku.Domain.Common;
using bagsisbaku.Domain.Localization;
using bagsisbaku.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace bagsisbaku.Infrastructure.Orders;

internal sealed class CustomerOrderService(
    ApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IClock clock,
    IValidator<CancelOrderCommand> cancelValidator)
    : ICustomerOrderService
{
    private const int MaximumPageSize = 50;

    public async Task<Result<CustomerOrderPageModel>>
        GetAllAsync(
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
    {
        if (page < 1 ||
            pageSize < 1 ||
            pageSize > MaximumPageSize)
        {
            return Result.Failure<CustomerOrderPageModel>(
                CustomerOrderErrors.InvalidPagination);
        }

        var customerResult =
            await GetCurrentCustomerIdAsync(
                cancellationToken);

        if (customerResult.IsFailure)
        {
            return Result.Failure<CustomerOrderPageModel>(
                customerResult.Error);
        }

        var customerId =
            customerResult.Value;

        var query =
            dbContext.Orders
                .AsNoTracking()
                .Where(
                    order =>
                        order.UserId == customerId);

        var totalCount =
            await query.CountAsync(
                cancellationToken);

        var orders =
            await query
                .Include(order => order.Items)
                .OrderByDescending(
                    order => order.PlacedAtUtc)
                .ThenByDescending(
                    order => order.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToArrayAsync(
                    cancellationToken);

        var items =
            orders
                .Select(
                    CustomerOrderModelMapper.ToSummary)
                .ToArray();

        var totalPages =
            totalCount == 0
                ? 0
                : (int)Math.Ceiling(
                    totalCount /
                    (double)pageSize);

        var model =
            new CustomerOrderPageModel(
                page,
                pageSize,
                totalCount,
                totalPages,
                page > 1 && totalCount > 0,
                page < totalPages,
                items);

        return Result.Success(model);
    }

    public async Task<Result<CustomerOrderDetailsModel>>
        GetByIdAsync(
            Guid orderId,
            SupportedLanguage language,
            CancellationToken cancellationToken = default)
    {
        if (orderId == Guid.Empty)
        {
            return Result.Failure<CustomerOrderDetailsModel>(
                CustomerOrderErrors.InvalidOrderId);
        }

        var customerResult =
            await GetCurrentCustomerIdAsync(
                cancellationToken);

        if (customerResult.IsFailure)
        {
            return Result.Failure<CustomerOrderDetailsModel>(
                customerResult.Error);
        }

        var order =
            await dbContext.Orders
                .AsNoTracking()
                .Include(currentOrder => currentOrder.Items)
                .Include(
                    currentOrder =>
                        currentOrder.StatusHistory)
                .SingleOrDefaultAsync(
                    currentOrder =>
                        currentOrder.Id == orderId &&
                        currentOrder.UserId ==
                        customerResult.Value,
                    cancellationToken);

        if (order is null)
        {
            return Result.Failure<CustomerOrderDetailsModel>(
                CustomerOrderErrors.OrderNotFound);
        }

        return Result.Success(
            CustomerOrderModelMapper.ToDetails(
                order,
                language));
    }

    public async Task<Result<CustomerOrderDetailsModel>>
        CancelAsync(
            Guid orderId,
            CancelOrderCommand command,
            SupportedLanguage language,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (orderId == Guid.Empty)
        {
            return Result.Failure<CustomerOrderDetailsModel>(
                CustomerOrderErrors.InvalidOrderId);
        }

        var validationResult =
            await cancelValidator.ValidateAsync(
                command,
                cancellationToken);

        if (!validationResult.IsValid)
        {
            var description =
                string.Join(
                    " ",
                    validationResult.Errors
                        .Select(error => error.ErrorMessage)
                        .Distinct(
                            StringComparer.Ordinal));

            return Result.Failure<CustomerOrderDetailsModel>(
                Error.Validation(
                    "orders.invalid-cancellation",
                    description));
        }

        var customerResult =
            await GetCurrentCustomerIdAsync(
                cancellationToken);

        if (customerResult.IsFailure)
        {
            return Result.Failure<CustomerOrderDetailsModel>(
                customerResult.Error);
        }

        var customerId =
            customerResult.Value;

        var executionStrategy =
            dbContext.Database
                .CreateExecutionStrategy();

        try
        {
            return await executionStrategy.ExecuteAsync(
                async () =>
                {
                    dbContext.ChangeTracker.Clear();

                    await using var transaction =
                        await dbContext.Database
                            .BeginTransactionAsync(
                                IsolationLevel.Serializable,
                                cancellationToken);

                    var order =
                        await dbContext.Orders
                            .Include(
                                currentOrder =>
                                    currentOrder.Items)
                            .Include(
                                currentOrder =>
                                    currentOrder.StatusHistory)
                            .SingleOrDefaultAsync(
                                currentOrder =>
                                    currentOrder.Id == orderId &&
                                    currentOrder.UserId ==
                                    customerId,
                                cancellationToken);

                    if (order is null)
                    {
                        return Result.Failure<
                            CustomerOrderDetailsModel>(
                            CustomerOrderErrors.OrderNotFound);
                    }

                    if (!order.CanBeCancelled)
                    {
                        return Result.Failure<
                            CustomerOrderDetailsModel>(
                            CustomerOrderErrors.CannotCancel);
                    }

                    var quantitiesByVariant =
                        order.Items
                            .GroupBy(
                                item =>
                                    item.ProductVariantId)
                            .ToDictionary(
                                group => group.Key,
                                group =>
                                    group.Sum(
                                        item =>
                                            item.Quantity));

                    var variantIds =
                        quantitiesByVariant.Keys
                            .ToArray();

                    var variants =
                        await dbContext.ProductVariants
                            .Where(
                                variant =>
                                    variantIds.Contains(
                                        variant.Id))
                            .ToDictionaryAsync(
                                variant => variant.Id,
                                cancellationToken);

                    if (variants.Count !=
                        quantitiesByVariant.Count)
                    {
                        return Result.Failure<
                            CustomerOrderDetailsModel>(
                            CustomerOrderErrors
                                .ProductVariantNotFound);
                    }

                    foreach (var entry in
                        quantitiesByVariant)
                    {
                        variants[entry.Key]
                            .IncreaseStock(entry.Value);
                    }

                    var cancelledAtUtc =
                        clock.UtcNow;

                    order.Cancel(
                        customerId,
                        command.Reason.Trim(),
                        cancelledAtUtc);

                    await OrderPromotionReleaseHelper
                        .ReleaseAsync(
                            dbContext,
                            order,
                            cancelledAtUtc,
                            cancellationToken);

                    await dbContext.SaveChangesAsync(
                        cancellationToken);

                    await transaction.CommitAsync(
                        cancellationToken);

                    return Result.Success(
                        CustomerOrderModelMapper.ToDetails(
                            order,
                            language));
                });
        }
        catch (DbUpdateConcurrencyException)
        {
            dbContext.ChangeTracker.Clear();

            return Result.Failure<CustomerOrderDetailsModel>(
                CustomerOrderErrors.ConcurrencyConflict);
        }
        catch (DomainException)
        {
            dbContext.ChangeTracker.Clear();

            return Result.Failure<CustomerOrderDetailsModel>(
                CustomerOrderErrors.CannotCancel);
        }
    }

    private async Task<Result<Guid>>
        GetCurrentCustomerIdAsync(
            CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated ||
            !currentUser.UserId.HasValue)
        {
            return Result.Failure<Guid>(
                CustomerOrderErrors
                    .AuthenticationRequired);
        }

        var userId =
            currentUser.UserId.Value;

        var customer =
            await dbContext.Users
                .AsNoTracking()
                .Where(user => user.Id == userId)
                .Select(
                    user =>
                        new
                        {
                            user.Id,
                            user.IsActive
                        })
                .SingleOrDefaultAsync(
                    cancellationToken);

        if (customer is null)
        {
            return Result.Failure<Guid>(
                CustomerOrderErrors.CustomerNotFound);
        }

        if (!customer.IsActive)
        {
            return Result.Failure<Guid>(
                CustomerOrderErrors.CustomerInactive);
        }

        return Result.Success(customer.Id);
    }
}