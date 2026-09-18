using System.Data;
using bagsisbaku.Application.Abstractions.Authentication;
using bagsisbaku.Application.Abstractions.Time;
using bagsisbaku.Application.Common.Results;
using bagsisbaku.Application.Orders.Administration;
using bagsisbaku.Domain.Common;
using bagsisbaku.Domain.Localization;
using bagsisbaku.Domain.Orders;
using bagsisbaku.Infrastructure.Orders;
using bagsisbaku.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace bagsisbaku.Infrastructure.Orders.Administration;

internal sealed class AdminOrderService(
    ApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IClock clock,
    IValidator<AdminOrderFilter> filterValidator,
    IValidator<ChangeOrderStatusCommand> statusValidator)
    : IAdminOrderService
{
    public async Task<Result<AdminOrderPageModel>>
        GetAllAsync(
            AdminOrderFilter filter,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var validationResult =
            await filterValidator.ValidateAsync(
                filter,
                cancellationToken);

        if (!validationResult.IsValid)
        {
            return Result.Failure<AdminOrderPageModel>(
                AdminOrderErrors.Validation(
                    CreateValidationDescription(
                        validationResult.Errors
                            .Select(
                                error =>
                                    error.ErrorMessage))));
        }

        var administratorResult =
            await GetAdministratorIdAsync(
                cancellationToken);

        if (administratorResult.IsFailure)
        {
            return Result.Failure<AdminOrderPageModel>(
                administratorResult.Error);
        }

        IQueryable<Order> query =
            dbContext.Orders
                .AsNoTracking();

        if (filter.Status.HasValue)
        {
            var status =
                (OrderStatus)filter.Status.Value;

            query =
                query.Where(
                    order =>
                        order.Status == status);
        }

        if (filter.DeliveryType.HasValue)
        {
            var deliveryType =
                (DeliveryType)filter.DeliveryType.Value;

            query =
                query.Where(
                    order =>
                        order.DeliveryType ==
                        deliveryType);
        }

        if (filter.FromUtc.HasValue)
        {
            query =
                query.Where(
                    order =>
                        order.PlacedAtUtc >=
                        filter.FromUtc.Value);
        }

        if (filter.ToUtc.HasValue)
        {
            query =
                query.Where(
                    order =>
                        order.PlacedAtUtc <=
                        filter.ToUtc.Value);
        }

        var search =
            filter.Search?.Trim();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern =
                $"%{search}%";

            query =
                query.Where(
                    order =>
                        EF.Functions.Like(
                            order.OrderNumber,
                            pattern) ||

                        EF.Functions.Like(
                            order.RecipientFullName,
                            pattern) ||

                        EF.Functions.Like(
                            order.PhoneNumber,
                            pattern) ||

                        dbContext.Users.Any(
                            user =>
                                user.Id == order.UserId &&
                                (
                                    EF.Functions.Like(
                                        user.FullName,
                                        pattern) ||

                                    (
                                        user.Email != null &&
                                        EF.Functions.Like(
                                            user.Email,
                                            pattern)
                                    )
                                )));
        }

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
                .Skip(
                    (filter.Page - 1) *
                    filter.PageSize)
                .Take(filter.PageSize)
                .ToArrayAsync(
                    cancellationToken);

        var customerIds =
            orders
                .Select(order => order.UserId)
                .Distinct()
                .ToArray();

        var customers =
            await dbContext.Users
                .AsNoTracking()
                .Where(
                    user =>
                        customerIds.Contains(
                            user.Id))
                .Select(
                    user =>
                        new CustomerLookup(
                            user.Id,
                            user.FullName,
                            user.Email ??
                            string.Empty))
                .ToDictionaryAsync(
                    user => user.Id,
                    cancellationToken);

        var items =
            orders
                .Select(
                    order =>
                    {
                        customers.TryGetValue(
                            order.UserId,
                            out var customer);

                        return AdminOrderModelMapper
                            .ToSummary(
                                order,
                                customer?.FullName ??
                                order.RecipientFullName,
                                customer?.Email ??
                                string.Empty);
                    })
                .ToArray();

        var totalPages =
            totalCount == 0
                ? 0
                : (int)Math.Ceiling(
                    totalCount /
                    (double)filter.PageSize);

        return Result.Success(
            new AdminOrderPageModel(
                filter.Page,
                filter.PageSize,
                totalCount,
                totalPages,
                filter.Page > 1 &&
                totalCount > 0,
                filter.Page < totalPages,
                items));
    }

    public async Task<Result<AdminOrderDetailsModel>>
        GetByIdAsync(
            Guid orderId,
            SupportedLanguage language,
            CancellationToken cancellationToken = default)
    {
        if (orderId == Guid.Empty)
        {
            return Result.Failure<AdminOrderDetailsModel>(
                AdminOrderErrors.InvalidOrderId);
        }

        var administratorResult =
            await GetAdministratorIdAsync(
                cancellationToken);

        if (administratorResult.IsFailure)
        {
            return Result.Failure<AdminOrderDetailsModel>(
                administratorResult.Error);
        }

        var order =
            await dbContext.Orders
                .AsNoTracking()
                .Include(item => item.Items)
                .Include(item => item.StatusHistory)
                .SingleOrDefaultAsync(
                    item => item.Id == orderId,
                    cancellationToken);

        if (order is null)
        {
            return Result.Failure<AdminOrderDetailsModel>(
                AdminOrderErrors.OrderNotFound);
        }

        var customer =
            await GetCustomerAsync(
                order.UserId,
                cancellationToken);

        return Result.Success(
            AdminOrderModelMapper.ToDetails(
                order,
                customer?.FullName ??
                order.RecipientFullName,
                customer?.Email ??
                string.Empty,
                NormalizeLanguage(language)));
    }

    public async Task<Result<AdminOrderDetailsModel>>
        ChangeStatusAsync(
            Guid orderId,
            ChangeOrderStatusCommand command,
            SupportedLanguage language,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (orderId == Guid.Empty)
        {
            return Result.Failure<AdminOrderDetailsModel>(
                AdminOrderErrors.InvalidOrderId);
        }

        var validationResult =
            await statusValidator.ValidateAsync(
                command,
                cancellationToken);

        if (!validationResult.IsValid)
        {
            return Result.Failure<AdminOrderDetailsModel>(
                AdminOrderErrors.Validation(
                    CreateValidationDescription(
                        validationResult.Errors
                            .Select(
                                error =>
                                    error.ErrorMessage))));
        }

        var administratorResult =
            await GetAdministratorIdAsync(
                cancellationToken);

        if (administratorResult.IsFailure)
        {
            return Result.Failure<AdminOrderDetailsModel>(
                administratorResult.Error);
        }

        var administratorId =
            administratorResult.Value;

        var targetStatus =
            (OrderStatus)command.Status;

        var selectedLanguage =
            NormalizeLanguage(language);

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
                            .Include(item => item.Items)
                            .Include(
                                item =>
                                    item.StatusHistory)
                            .SingleOrDefaultAsync(
                                item =>
                                    item.Id == orderId,
                                cancellationToken);

                    if (order is null)
                    {
                        return Result.Failure<
                            AdminOrderDetailsModel>(
                            AdminOrderErrors.OrderNotFound);
                    }

                    var changeResult =
                        await ApplyStatusChangeAsync(
                            order,
                            targetStatus,
                            administratorId,
                            command.Note,
                            cancellationToken);

                    if (changeResult.IsFailure)
                    {
                        return Result.Failure<
                            AdminOrderDetailsModel>(
                            changeResult.Error);
                    }

                    await dbContext.SaveChangesAsync(
                        cancellationToken);

                    await transaction.CommitAsync(
                        cancellationToken);

                    var customer =
                        await GetCustomerAsync(
                            order.UserId,
                            cancellationToken);

                    return Result.Success(
                        AdminOrderModelMapper.ToDetails(
                            order,
                            customer?.FullName ??
                            order.RecipientFullName,
                            customer?.Email ??
                            string.Empty,
                            selectedLanguage));
                });
        }
        catch (DbUpdateConcurrencyException)
        {
            dbContext.ChangeTracker.Clear();

            return Result.Failure<AdminOrderDetailsModel>(
                AdminOrderErrors.ConcurrencyConflict);
        }
        catch (DomainException)
        {
            dbContext.ChangeTracker.Clear();

            return Result.Failure<AdminOrderDetailsModel>(
                AdminOrderErrors.InvalidTransition);
        }
    }

    private async Task<Result>
        ApplyStatusChangeAsync(
            Order order,
            OrderStatus targetStatus,
            Guid administratorId,
            string? note,
            CancellationToken cancellationToken)
    {
        var changedAtUtc =
            clock.UtcNow;

        var normalizedNote =
            string.IsNullOrWhiteSpace(note)
                ? null
                : note.Trim();

        switch (targetStatus)
        {
            case OrderStatus.Confirmed:
                order.Confirm(
                    administratorId,
                    changedAtUtc,
                    normalizedNote);

                return Result.Success();

            case OrderStatus.OnDelivery:
                order.StartDelivery(
                    administratorId,
                    changedAtUtc,
                    normalizedNote);

                return Result.Success();

            case OrderStatus.Delivered:
                order.MarkDelivered(
                    administratorId,
                    changedAtUtc,
                    normalizedNote);

                return Result.Success();

            case OrderStatus.Cancelled:
                if (!order.CanBeCancelled)
                {
                    return Result.Failure(
                        AdminOrderErrors
                            .InvalidTransition);
                }

                var stockResult =
                    await RestoreStockAsync(
                        order,
                        cancellationToken);

                if (stockResult.IsFailure)
                {
                    return stockResult;
                }

                order.Cancel(
                    administratorId,
                    normalizedNote ??
                    "Admin tərəfindən ləğv edildi.",
                    changedAtUtc);

                await OrderPromotionReleaseHelper
                    .ReleaseAsync(
                        dbContext,
                        order,
                        changedAtUtc,
                        cancellationToken);

                return Result.Success();

            default:
                return Result.Failure(
                    AdminOrderErrors.InvalidTransition);
        }
    }

    private async Task<Result> RestoreStockAsync(
        Order order,
        CancellationToken cancellationToken)
    {
        var quantities =
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
            quantities.Keys.ToArray();

        var variants =
            await dbContext.ProductVariants
                .Where(
                    variant =>
                        variantIds.Contains(
                            variant.Id))
                .ToDictionaryAsync(
                    variant => variant.Id,
                    cancellationToken);

        if (variants.Count != quantities.Count)
        {
            return Result.Failure(
                AdminOrderErrors
                    .ProductVariantNotFound);
        }

        foreach (var quantity in quantities)
        {
            variants[quantity.Key]
                .IncreaseStock(quantity.Value);
        }

        return Result.Success();
    }

    private async Task<Result<Guid>>
        GetAdministratorIdAsync(
            CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated ||
            currentUser.UserId is not Guid administratorId ||
            administratorId == Guid.Empty)
        {
            return Result.Failure<Guid>(
                AdminOrderErrors
                    .AuthenticationRequired);
        }

        var administrator =
            await dbContext.Users
                .AsNoTracking()
                .Where(
                    user =>
                        user.Id ==
                        administratorId)
                .Select(
                    user =>
                        new
                        {
                            user.Id,
                            user.IsActive
                        })
                .SingleOrDefaultAsync(
                    cancellationToken);

        if (administrator is null)
        {
            return Result.Failure<Guid>(
                AdminOrderErrors
                    .AdministratorNotFound);
        }

        if (!administrator.IsActive)
        {
            return Result.Failure<Guid>(
                AdminOrderErrors
                    .AdministratorInactive);
        }

        return Result.Success(
            administrator.Id);
    }

    private async Task<CustomerLookup?>
        GetCustomerAsync(
            Guid customerId,
            CancellationToken cancellationToken)
    {
        return await dbContext.Users
            .AsNoTracking()
            .Where(
                user =>
                    user.Id == customerId)
            .Select(
                user =>
                    new CustomerLookup(
                        user.Id,
                        user.FullName,
                        user.Email ??
                        string.Empty))
            .SingleOrDefaultAsync(
                cancellationToken);
    }

    private static SupportedLanguage NormalizeLanguage(
        SupportedLanguage language)
    {
        return Enum.IsDefined(language)
            ? language
            : SupportedLanguage.Azerbaijani;
    }

    private static string CreateValidationDescription(
        IEnumerable<string> messages)
    {
        return string.Join(
            " ",
            messages.Distinct(
                StringComparer.Ordinal));
    }

    private sealed record CustomerLookup(
        Guid Id,
        string FullName,
        string Email);
}