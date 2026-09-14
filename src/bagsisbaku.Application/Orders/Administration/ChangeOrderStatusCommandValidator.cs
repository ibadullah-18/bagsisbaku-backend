using bagsisbaku.Domain.Orders;
using FluentValidation;

namespace bagsisbaku.Application.Orders.Administration;

public sealed class ChangeOrderStatusCommandValidator
    : AbstractValidator<ChangeOrderStatusCommand>
{
    public ChangeOrderStatusCommandValidator()
    {
        RuleFor(command => command.Status)
            .Must(
                status =>
                    Enum.IsDefined(
                        typeof(OrderStatus),
                        status))
            .WithMessage(
                "Sifariş statusu düzgün deyil.");

        RuleFor(command => command.Note)
            .MaximumLength(500)
            .When(
                command =>
                    !string.IsNullOrWhiteSpace(
                        command.Note))
            .WithMessage(
                "Status qeydi maksimum 500 simvol ola bilər.");

        RuleFor(command => command.Note)
            .NotEmpty()
            .When(
                command =>
                    command.Status ==
                    (int)OrderStatus.Cancelled)
            .WithMessage(
                "Sifariş ləğv ediləndə səbəb yazılmalıdır.");
    }
}