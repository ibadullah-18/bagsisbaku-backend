using bagsisbaku.Application.Administration.Admins;
using bagsisbaku.Application.Common.Results;
using bagsisbaku.Application.Security;

namespace bagsisbaku.Infrastructure.Administration.Admins;

internal sealed partial class AdminManagementService
{
    public async Task<IReadOnlyCollection<AdminModel>>
        GetAllAsync(
            CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var admins =
            await _userManager.GetUsersInRoleAsync(
                SystemRoles.Admin);

        var models =
            new List<AdminModel>(
                admins.Count);

        foreach (
            var admin in admins.OrderByDescending(
                item => item.CreatedAtUtc)
        )
        {
            cancellationToken
                .ThrowIfCancellationRequested();

            models.Add(
                await ToModelAsync(admin));
        }

        return models;
    }

    public async Task<Result<AdminModel>> GetByIdAsync(
        Guid adminId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (adminId == Guid.Empty)
        {
            return Result.Failure<AdminModel>(
                AdminManagementErrors.AdminNotFound);
        }

        var admin =
            await _userManager.FindByIdAsync(
                adminId.ToString());

        var targetError =
            await GetTargetErrorAsync(admin);

        if (targetError is not null)
        {
            return Result.Failure<AdminModel>(
                targetError);
        }

        return Result.Success(
            await ToModelAsync(admin!));
    }
}
