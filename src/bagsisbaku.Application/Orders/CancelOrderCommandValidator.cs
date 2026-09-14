using FluentValidation;

namespace bagsisbaku.Application.Orders;

public sealed class CancelOrderCommandValidator
    : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        RuleFor(command => command.Reason)
            .NotEmpty()
            .WithMessage(
                "Ləğv etmə səbəbi yazılmalıdır.")
            .MaximumLength(500)
            .WithMessage(
                "Ləğv etmə səbəbi 500 simvoldan çox ola bilməz.");
    }
}