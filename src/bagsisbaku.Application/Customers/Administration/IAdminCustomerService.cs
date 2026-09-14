using bagsisbaku.Application.Common.Results;

namespace bagsisbaku.Application.Customers.Administration;

public interface IAdminCustomerService
{
    Task<Result<AdminCustomerPageModel>> GetAllAsync(
        AdminCustomerFilter filter,
        CancellationToken cancellationToken = default);

    Task<Result<AdminCustomerDetailsModel>> GetByIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default);

    Task<Result<AdminCustomerDetailsModel>> SetStatusAsync(
        SetCustomerStatusCommand command,
        CancellationToken cancellationToken = default);
}