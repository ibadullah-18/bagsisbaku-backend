using System.Security.Claims;
using bagsisbaku.Application.Administration.Admins;
using bagsisbaku.Application.Common.Results;
using bagsisbaku.Application.Security;
using bagsisbaku.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace bagsisbaku.Infrastructure.Administration.Admins;

internal sealed partial class AdminManagementService
{
    public async Task<Result<AdminModel>> CreateAsync(
        CreateAdminCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var permissionResult =
            ValidatePermissions(command.Permissions);

        if (permissionResult.IsFailure)
        {
            return Result.Failure<AdminModel>(
                permissionResult.Error);
        }

        var permissions =
            permissionResult.Value;

        var executionStrategy =
            _dbContext.Database.CreateExecutionStrategy();

        return await executionStrategy.ExecuteAsync(
            async () =>
            {
                await using var transaction =
                    await _dbContext.Database
                        .BeginTransactionAsync(
                            cancellationToken);

                var email = command.Email.Trim();

                var existingUser =
                    await _userManager.FindByEmailAsync(
                        email);

                if (existingUser is not null)
                {
                    return Result.Failure<AdminModel>(
                        AdminManagementErrors
                            .EmailAlreadyRegistered);
                }

                if (
                    !await _roleManager.RoleExistsAsync(
                        SystemRoles.Admin)
                )
                {
                    return Result.Failure<AdminModel>(
                        AdminManagementErrors
                            .AdminRoleNotFound);
                }

                AppUser admin;

                try
                {
                    admin = AppUser.Create(
                        command.FullName,
                        email,
                        _clock.UtcNow);

                    admin.EmailConfirmed = true;
                }
                catch (ArgumentException exception)
                {
                    return Result.Failure<AdminModel>(
                        AdminManagementErrors
                            .IdentityValidation(
                                exception.Message));
                }

                var createResult =
                    await _userManager.CreateAsync(
                        admin,
                        command.Password);

                if (!createResult.Succeeded)
                {
                    return Result.Failure<AdminModel>(
                        ToAdminIdentityError(
                            createResult));
                }

                var roleResult =
                    await _userManager.AddToRoleAsync(
                        admin,
                        SystemRoles.Admin);

                if (!roleResult.Succeeded)
                {
                    return Result.Failure<AdminModel>(
                        ToAdminIdentityError(
                            roleResult));
                }

                if (permissions.Length > 0)
                {
                    var claims =
                        permissions
                            .Select(
                                permission =>
                                    new Claim(
                                        CustomClaimTypes
                                            .Permission,
                                        permission))
                            .ToArray();

                    var claimResult =
                        await _userManager.AddClaimsAsync(
                            admin,
                            claims);

                    if (!claimResult.Succeeded)
                    {
                        return Result.Failure<AdminModel>(
                            ToAdminIdentityError(
                                claimResult));
                    }
                }

                await transaction.CommitAsync(
                    cancellationToken);

                return Result.Success(
                    new AdminModel(
                        admin.Id,
                        admin.FullName,
                        admin.Email!,
                        admin.IsActive,
                        admin.CreatedAtUtc,
                        admin.LastLoginAtUtc,
                        permissions));
            });
    }

    public async Task<Result> UpdatePermissionsAsync(
        UpdateAdminPermissionsCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var permissionResult =
            ValidatePermissions(command.Permissions);

        if (permissionResult.IsFailure)
        {
            return Result.Failure(
                permissionResult.Error);
        }

        var requestedPermissions =
            permissionResult.Value;

        var executionStrategy =
            _dbContext.Database.CreateExecutionStrategy();

        return await executionStrategy.ExecuteAsync(
            async () =>
            {
                await using var transaction =
                    await _dbContext.Database
                        .BeginTransactionAsync(
                            cancellationToken);

                var admin =
                    await _userManager.FindByIdAsync(
                        command.AdminId.ToString());

                var targetError =
                    await GetTargetErrorAsync(admin);

                if (targetError is not null)
                {
                    return Result.Failure(targetError);
                }

                var currentClaims =
                    await _userManager.GetClaimsAsync(
                        admin!);

                var currentPermissionClaims =
                    currentClaims
                        .Where(
                            claim =>
                                claim.Type ==
                                    CustomClaimTypes
                                        .Permission)
                        .ToArray();

                var currentPermissions =
                    currentPermissionClaims
                        .Where(
                            claim =>
                                PermissionNames.All
                                    .Contains(
                                        claim.Value))
                        .Select(claim => claim.Value)
                        .ToHashSet(
                            StringComparer.Ordinal);

                if (
                    currentPermissions.SetEquals(
                        requestedPermissions)
                )
                {
                    await transaction.CommitAsync(
                        cancellationToken);

                    return Result.Success();
                }

                if (currentPermissionClaims.Length > 0)
                {
                    var removeResult =
                        await _userManager
                            .RemoveClaimsAsync(
                                admin!,
                                currentPermissionClaims);

                    if (!removeResult.Succeeded)
                    {
                        return Result.Failure(
                            ToAdminIdentityError(
                                removeResult));
                    }
                }

                if (requestedPermissions.Length > 0)
                {
                    var newClaims =
                        requestedPermissions
                            .Select(
                                permission =>
                                    new Claim(
                                        CustomClaimTypes
                                            .Permission,
                                        permission))
                            .ToArray();

                    var addResult =
                        await _userManager.AddClaimsAsync(
                            admin!,
                            newClaims);

                    if (!addResult.Succeeded)
                    {
                        return Result.Failure(
                            ToAdminIdentityError(
                                addResult));
                    }
                }

                await RevokeActiveRefreshTokensAsync(
                    admin!.Id,
                    "permissions-changed",
                    cancellationToken);

                await transaction.CommitAsync(
                    cancellationToken);

                return Result.Success();
            });
    }

    public async Task<Result> SetStatusAsync(
        SetAdminStatusCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var executionStrategy =
            _dbContext.Database.CreateExecutionStrategy();

        return await executionStrategy.ExecuteAsync(
            async () =>
            {
                await using var transaction =
                    await _dbContext.Database
                        .BeginTransactionAsync(
                            cancellationToken);

                var admin =
                    await _userManager.FindByIdAsync(
                        command.AdminId.ToString());

                var targetError =
                    await GetTargetErrorAsync(admin);

                if (targetError is not null)
                {
                    return Result.Failure(targetError);
                }

                if (admin!.IsActive == command.IsActive)
                {
                    await transaction.CommitAsync(
                        cancellationToken);

                    return Result.Success();
                }

                if (command.IsActive)
                {
                    admin.Activate(_clock.UtcNow);
                }
                else
                {
                    admin.Deactivate(_clock.UtcNow);
                }

                var updateResult =
                    await _userManager.UpdateAsync(admin);

                if (!updateResult.Succeeded)
                {
                    return Result.Failure(
                        ToAdminIdentityError(
                            updateResult));
                }

                if (!command.IsActive)
                {
                    await RevokeActiveRefreshTokensAsync(
                        admin.Id,
                        "admin-deactivated",
                        cancellationToken);
                }

                await transaction.CommitAsync(
                    cancellationToken);

                return Result.Success();
            });
    }
}
