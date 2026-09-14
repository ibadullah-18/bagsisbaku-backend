using bagsisbaku.Application.Orders;
using Xunit;

namespace bagsisbaku.Application.UnitTests;

public sealed class CancelOrderCommandValidatorTests
{
    private readonly CancelOrderCommandValidator
        _validator = new();

    [Fact]
    public void ValidReasonPassesValidation()
    {
        var command =
            new CancelOrderCommand(
                "Məhsulu səhv seçmişəm.");

        var result =
            _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void EmptyReasonFailsValidation()
    {
        var command =
            new CancelOrderCommand(
                string.Empty);

        var result =
            _validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void LongReasonFailsValidation()
    {
        var command =
            new CancelOrderCommand(
                new string('a', 501));

        var result =
            _validator.Validate(command);

        Assert.False(result.IsValid);
    }
}