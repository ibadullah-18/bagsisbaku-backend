using bagsisbaku.Application.Customers.Administration;
using Xunit;

namespace bagsisbaku.Application.UnitTests;

public sealed class AdminCustomerValidatorTests
{
    [Fact]
    public void ValidFilterShouldPassValidation()
    {
        var validator =
            new AdminCustomerFilterValidator();

        var filter =
            new AdminCustomerFilter(
                Search: "Ibadulla",
                IsActive: true,
                EmailConfirmed: true,
                CreatedFromUtc:
                    new DateTimeOffset(
                        2026,
                        1,
                        1,
                        0,
                        0,
                        0,
                        TimeSpan.Zero),
                CreatedToUtc:
                    new DateTimeOffset(
                        2026,
                        12,
                        31,
                        23,
                        59,
                        59,
                        TimeSpan.Zero),
                Page: 1,
                PageSize: 20);

        var result =
            validator.Validate(filter);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void PageLessThanOneShouldFailValidation()
    {
        var validator =
            new AdminCustomerFilterValidator();

        var filter =
            CreateValidFilter() with
            {
                Page = 0
            };

        var result =
            validator.Validate(filter);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                nameof(AdminCustomerFilter.Page));
    }

    [Fact]
    public void PageSizeGreaterThanOneHundredShouldFailValidation()
    {
        var validator =
            new AdminCustomerFilterValidator();

        var filter =
            CreateValidFilter() with
            {
                PageSize = 101
            };

        var result =
            validator.Validate(filter);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                nameof(AdminCustomerFilter.PageSize));
    }

    [Fact]
    public void LongSearchShouldFailValidation()
    {
        var validator =
            new AdminCustomerFilterValidator();

        var filter =
            CreateValidFilter() with
            {
                Search = new string('a', 201)
            };

        var result =
            validator.Validate(filter);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                nameof(AdminCustomerFilter.Search));
    }

    [Fact]
    public void InvalidDateRangeShouldFailValidation()
    {
        var validator =
            new AdminCustomerFilterValidator();

        var filter =
            CreateValidFilter() with
            {
                CreatedFromUtc =
                    new DateTimeOffset(
                        2026,
                        12,
                        31,
                        0,
                        0,
                        0,
                        TimeSpan.Zero),

                CreatedToUtc =
                    new DateTimeOffset(
                        2026,
                        1,
                        1,
                        0,
                        0,
                        0,
                        TimeSpan.Zero)
            };

        var result =
            validator.Validate(filter);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                nameof(
                    AdminCustomerFilter
                        .CreatedToUtc));
    }

    [Fact]
    public void EmptyCustomerIdShouldFailValidation()
    {
        var validator =
            new SetCustomerStatusCommandValidator();

        var command =
            new SetCustomerStatusCommand(
                Guid.Empty,
                IsActive: false);

        var result =
            validator.Validate(command);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                nameof(
                    SetCustomerStatusCommand
                        .CustomerId));
    }

    private static AdminCustomerFilter
        CreateValidFilter()
    {
        return new AdminCustomerFilter(
            Search: null,
            IsActive: null,
            EmailConfirmed: null,
            CreatedFromUtc: null,
            CreatedToUtc: null,
            Page: 1,
            PageSize: 20);
    }
}