using bagsisbaku.Application.Promotions.Administration;
using bagsisbaku.Domain.Promotions;
using Xunit;

namespace bagsisbaku.Application.UnitTests;

public sealed class AdminPromotionValidatorTests
{
    [Fact]
    public async Task CreateValidatorShouldAcceptValidCommand()
    {
        var validator =
            new CreatePromoCodeCommandValidator();

        var command =
            CreateValidCommand();

        var result =
            await validator.ValidateAsync(
                command,
                TestContext.Current.CancellationToken);

        Assert.True(
            result.IsValid);
    }

    [Fact]
    public async Task CreateValidatorShouldRejectInvalidPercentage()
    {
        var validator =
            new CreatePromoCodeCommandValidator();

        var valid =
            CreateValidCommand();

        var command =
            valid with
            {
                DiscountType =
                    (int)PromotionDiscountType.Percentage,

                DiscountValue = 101m
            };

        var result =
            await validator.ValidateAsync(
                command,
                TestContext.Current.CancellationToken);

        Assert.False(
            result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                nameof(command.DiscountValue));
    }

    [Fact]
    public async Task CreateValidatorShouldRejectInvalidSchedule()
    {
        var validator =
            new CreatePromoCodeCommandValidator();

        var valid =
            CreateValidCommand();

        var command =
            valid with
            {
                EndsAtUtc =
                    valid.StartsAtUtc
            };

        var result =
            await validator.ValidateAsync(
                command,
                TestContext.Current.CancellationToken);

        Assert.False(
            result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                nameof(command.EndsAtUtc));
    }

    [Fact]
    public async Task FilterValidatorShouldRejectLargePageSize()
    {
        var validator =
            new AdminPromoCodeFilterValidator();

        var filter =
            new AdminPromoCodeFilter(
                Search: null,
                IsActive: null,
                Page: 1,
                PageSize: 101);

        var result =
            await validator.ValidateAsync(
                filter,
                TestContext.Current.CancellationToken);

        Assert.False(
            result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                nameof(filter.PageSize));
    }

    private static CreatePromoCodeCommand
        CreateValidCommand()
    {
        var startsAtUtc =
            new DateTimeOffset(
                2026,
                9,
                1,
                0,
                0,
                0,
                TimeSpan.Zero);

        return new CreatePromoCodeCommand(
            Name:
                "Sentyabr endirimi",

            Code:
                "BAGSIS20",

            DiscountType:
                (int)PromotionDiscountType.Percentage,

            DiscountValue:
                20m,

            MinimumOrderAmount:
                50m,

            MaximumDiscountAmount:
                100m,

            UsageLimit:
                100,

            PerCustomerUsageLimit:
                1,

            StartsAtUtc:
                startsAtUtc,

            EndsAtUtc:
                startsAtUtc.AddDays(30));
    }
}