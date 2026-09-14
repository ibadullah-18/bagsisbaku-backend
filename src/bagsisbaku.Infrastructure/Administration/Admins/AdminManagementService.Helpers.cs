using bagsisbaku.Application.Administration.Admins;
using bagsisbaku.Application.Common.Results;
using bagsisbaku.Application.Security;
using bagsisbaku.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace bagsisbaku.Infrastructure.Administration.Admins;

internal sealed partial class AdminManagementService
{
    private async Task<AdminModel> ToModelAsync(
        AppUser admin)
    {
        var claims =
            await _userManager.GetClaimsAsync(admin);

        var permissions =
            claims
                .Where(
                    claim =>
                        claim.Type ==
                            CustomClaimTypes.Permission &&
                        PermissionNames.All.Contains(
                            claim.Value))
                .Select(claim => claim.Value)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(
                    permission => permission,
                    StringComparer.Ordinal)
                .ToArray();

        return new AdminModel(
            admin.Id,
            admin.FullName,
            admin.Email!,
            admin.IsActive,
            admin.CreatedAtUtc,
            admin.LastLoginAtUtc,
            permissions);
    }

    private async Task<Error?> GetTargetErrorAsync(
        AppUser? user)
    {
        if (user is null)
        {
            return AdminManagementErrors.AdminNotFound;
        }

        if (
            await _userManager.IsInRoleAsync(
                user,
                SystemRoles.SuperAdmin)
        )
        {
            return AdminManagementErrors
                .TargetIsSuperAdmin;
        }

        if (
            !await _userManager.IsInRoleAsync(
                user,
                SystemRoles.Admin)
        )
        {
            return AdminManagementErrors.AdminNotFound;
        }

        return null;
    }

    private async Task RevokeActiveRefreshTokensAsync(
        Guid userId,
        string reason,
        CancellationToken cancellationToken)
    {
        var utcNow = _clock.UtcNow;

        var refreshTokens =
            await _dbContext.RefreshTokens
                .Where(
                    token =>
                        token.UserId == userId &&
                        token.RevokedAtUtc == null &&
                        token.ExpiresAtUtc > utcNow)
                .ToListAsync(cancellationToken);

        foreach (var refreshToken in refreshTokens)
        {
            refreshToken.Revoke(
                utcNow,
                revokedByIp: null,
                replacedByTokenHash: null,
                reason);
        }

        if (refreshTokens.Count > 0)
        {
            await _dbContext.SaveChangesAsync(
                cancellationToken);
        }
    }

    private static Result<string[]>
        ValidatePermissions(
            IReadOnlyCollection<string> permissions)
    {
        ArgumentNullException.ThrowIfNull(permissions);

        var normalizedPermissions =
            permissions
                .Where(
                    permission =>
                        !string.IsNullOrWhiteSpace(
                            permission))
                .Select(
                    permission => permission.Trim())
                .Distinct(StringComparer.Ordinal)
                .OrderBy(
                    permission => permission,
                    StringComparer.Ordinal)
                .ToArray();

        var invalidPermission =
            normalizedPermissions.FirstOrDefault(
                permission =>
                    !PermissionNames.All.Contains(
                        permission));

        if (invalidPermission is not null)
        {
            return Result.Failure<string[]>(
                AdminManagementErrors.InvalidPermission(
                    invalidPermission));
        }

        return Result.Success(
            normalizedPermissions);
    }

    private static Error ToAdminIdentityError(
        IdentityResult identityResult)
    {
        ArgumentNullException.ThrowIfNull(identityResult);

        var duplicateEmail =
            identityResult.Errors.Any(
                error =>
                    string.Equals(
                        error.Code,
                        "DuplicateEmail",
                        StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(
                        error.Code,
                        "DuplicateUserName",
                        StringComparison.OrdinalIgnoreCase));

        if (duplicateEmail)
        {
            return AdminManagementErrors
                .EmailAlreadyRegistered;
        }

        var descriptions =
            identityResult.Errors
                .Select(error => error.Description)
                .Where(
                    description =>
                        !string.IsNullOrWhiteSpace(
                            description))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

        var description =
            descriptions.Length == 0
                ? "Admin əməliyyatı uğursuz oldu."
                : string.Join(" ", descriptions);

        return AdminManagementErrors.IdentityValidation(
            description);
    }
}
