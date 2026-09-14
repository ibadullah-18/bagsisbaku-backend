using bagsisbaku.Application.Common.Results;

namespace bagsisbaku.Application.Administration.Admins;

public interface IAdminManagementService
{
    Task<Result<AdminModel>> CreateAsync(
        CreateAdminCommand command,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<AdminModel>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<Result<AdminModel>> GetByIdAsync(
        Guid adminId,
        CancellationToken cancellationToken = default);

    Task<Result> UpdatePermissionsAsync(
        UpdateAdminPermissionsCommand command,
        CancellationToken cancellationToken = default);

    Task<Result> SetStatusAsync(
        SetAdminStatusCommand command,
        CancellationToken cancellationToken = default);
}
