using bagsisbaku.Application.Orders.Administration;
using bagsisbaku.Domain.Orders;
using Xunit;

namespace bagsisbaku.Application.UnitTests;

public sealed class AdminOrderValidatorTests
{
    [Fact]
    public async Task ValidAdminOrderFilterPasses()
    {
        var validator =
            new AdminOrderFilterValidator();

        var filter =
            new AdminOrderFilter(
                Status:
                    (int)OrderStatus.Pending,
                DeliveryType:
                    (int)DeliveryType.StorePickup,
                Search:
                    "bagsisbaku",
                FromUtc:
                    DateTimeOffset.UtcNow.AddDays(-7),
                ToUtc:
                    DateTimeOffset.UtcNow,
                Page:
                    1,
                PageSize:
                    20);

        var result =
            await validator.ValidateAsync(
                filter,
                TestContext.Current.CancellationToken);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task InvalidPaginationFails()
    {
        var validator =
            new AdminOrderFilterValidator();

        var filter =
            new AdminOrderFilter(
                Status: null,
                DeliveryType: null,
                Search: null,
                FromUtc: null,
                ToUtc: null,
                Page: 0,
                PageSize: 101);

        var result =
            await validator.ValidateAsync(
                filter,
                TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                nameof(AdminOrderFilter.Page));

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                nameof(AdminOrderFilter.PageSize));
    }

    [Fact]
    public async Task InvalidDateRangeFails()
    {
        var validator =
            new AdminOrderFilterValidator();

        var filter =
            new AdminOrderFilter(
                Status: null,
                DeliveryType: null,
                Search: null,
                FromUtc:
                    DateTimeOffset.UtcNow,
                ToUtc:
                    DateTimeOffset.UtcNow.AddDays(-1),
                Page: 1,
                PageSize: 20);

        var result =
            await validator.ValidateAsync(
                filter,
                TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ValidOrderStatusCommandPasses()
    {
        var validator =
            new ChangeOrderStatusCommandValidator();

        var command =
            new ChangeOrderStatusCommand(
                (int)OrderStatus.Confirmed,
                "Sifariş təsdiqləndi.");

        var result =
            await validator.ValidateAsync(
                command,
                TestContext.Current.CancellationToken);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task InvalidOrderStatusCommandFails()
    {
        var validator =
            new ChangeOrderStatusCommandValidator();

        var command =
            new ChangeOrderStatusCommand(
                999,
                new string('x', 501));

        var result =
            await validator.ValidateAsync(
                command,
                TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                nameof(ChangeOrderStatusCommand.Status));

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                nameof(ChangeOrderStatusCommand.Note));
    }
}