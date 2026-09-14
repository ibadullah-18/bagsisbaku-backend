using bagsisbaku.Application.Common.Results;
using bagsisbaku.Domain.Localization;

namespace bagsisbaku.Application.Orders;

public interface ICustomerOrderService
{
    Task<Result<CustomerOrderPageModel>> GetAllAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<Result<CustomerOrderDetailsModel>> GetByIdAsync(
        Guid orderId,
        SupportedLanguage language,
        CancellationToken cancellationToken = default);

    Task<Result<CustomerOrderDetailsModel>> CancelAsync(
        Guid orderId,
        CancelOrderCommand command,
        SupportedLanguage language,
        CancellationToken cancellationToken = default);
}