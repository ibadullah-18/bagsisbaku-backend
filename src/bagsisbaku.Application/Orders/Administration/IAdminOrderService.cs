using bagsisbaku.Application.Common.Results;
using bagsisbaku.Domain.Localization;

namespace bagsisbaku.Application.Orders.Administration;

public interface IAdminOrderService
{
    Task<Result<AdminOrderPageModel>> GetAllAsync(
        AdminOrderFilter filter,
        CancellationToken cancellationToken = default);

    Task<Result<AdminOrderDetailsModel>> GetByIdAsync(
        Guid orderId,
        SupportedLanguage language,
        CancellationToken cancellationToken = default);

    Task<Result<AdminOrderDetailsModel>> ChangeStatusAsync(
        Guid orderId,
        ChangeOrderStatusCommand command,
        SupportedLanguage language,
        CancellationToken cancellationToken = default);
}