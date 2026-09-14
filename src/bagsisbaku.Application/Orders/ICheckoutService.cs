using bagsisbaku.Application.Common.Results;
using bagsisbaku.Domain.Localization;

namespace bagsisbaku.Application.Orders;

public interface ICheckoutService
{
    Task<Result<PlacedOrderModel>> PlaceOrderAsync(
        PlaceOrderCommand command,
        SupportedLanguage language,
        CancellationToken cancellationToken = default);
}