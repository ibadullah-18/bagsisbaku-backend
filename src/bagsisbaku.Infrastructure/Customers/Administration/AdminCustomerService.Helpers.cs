using bagsisbaku.Application.Common.Results;
using bagsisbaku.Application.Customers.Administration;
using bagsisbaku.Application.Security;
using bagsisbaku.Infrastructure.Identity;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace bagsisbaku.Infrastructure.Customers.Administration;

internal sealed partial class AdminCustomerService
{
    private IQueryable<AppUser> CustomerUsersQuery(
        bool asNoTracking)
    {
        var normalizedCustomerRole =
            SystemRoles.Customer.ToUpperInvariant();

        var query =
            from user in _dbContext.Users

            join userRole in
                _dbContext.UserRoles
                on user.Id equals userRole.UserId

            join role in
                _dbContext.Roles
                on userRole.RoleId equals role.Id

            where role.NormalizedName ==
                  normalizedCustomerRole

            select user;

        return asNoTracking
            ? query.AsNoTracking()
            : query.AsTracking();
    }

    private async Task<Result<AppUser>>
        GetAuthorizedAdminAsync(
            string requiredPermission,
            CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            _currentUser.UserId is not Guid adminId ||
            adminId == Guid.Empty)
        {
            return Result.Failure<AppUser>(
                AdminCustomerErrors
                    .AdminAuthenticationRequired);
        }

        if (!_currentUser.HasPermission(
            requiredPermission))
        {
            return Result.Failure<AppUser>(
                AdminCustomerErrors
                    .PermissionDenied);
        }

        var admin =
            await _dbContext.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    user =>
                        user.Id == adminId,
                    cancellationToken);

        if (admin is null)
        {
            return Result.Failure<AppUser>(
                AdminCustomerErrors.AdminNotFound);
        }

        if (!admin.IsActive)
        {
            return Result.Failure<AppUser>(
                AdminCustomerErrors.AdminInactive);
        }

        return Result.Success(admin);
    }

    private async Task RevokeActiveRefreshTokensAsync(
        Guid customerId,
        DateTimeOffset utcNow,
        CancellationToken cancellationToken)
    {
        var activeRefreshTokens =
            await _dbContext.RefreshTokens
                .Where(token =>
                    token.UserId == customerId &&
                    token.RevokedAtUtc == null &&
                    token.ExpiresAtUtc > utcNow)
                .ToArrayAsync(
                    cancellationToken);

        foreach (var refreshToken
                 in activeRefreshTokens)
        {
            refreshToken.Revoke(
                utcNow,
                revokedByIp: null,
                replacedByTokenHash: null,
                reason:
                    "customer-deactivated-by-admin");
        }

        if (activeRefreshTokens.Length > 0)
        {
            await _dbContext.SaveChangesAsync(
                cancellationToken);
        }
    }

    private static Error ToCustomerIdentityError(
        IdentityResult identityResult)
    {
        ArgumentNullException.ThrowIfNull(
            identityResult);

        var hasConcurrencyFailure =
            identityResult.Errors.Any(error =>
                string.Equals(
                    error.Code,
                    "ConcurrencyFailure",
                    StringComparison.OrdinalIgnoreCase));

        if (hasConcurrencyFailure)
        {
            return AdminCustomerErrors
                .ConcurrencyConflict;
        }

        var descriptions =
            identityResult.Errors
                .Select(error =>
                    error.Description)
                .Where(description =>
                    !string.IsNullOrWhiteSpace(
                        description))
                .Distinct(
                    StringComparer.Ordinal)
                .ToArray();

        if (descriptions.Length == 0)
        {
            return AdminCustomerErrors
                .UnexpectedFailure;
        }

        return AdminCustomerErrors.Validation(
            string.Join(
                " ",
                descriptions));
    }

    private static string CreateValidationMessage(
        IEnumerable<ValidationFailure> failures)
    {
        var messages =
            failures
                .Select(failure =>
                    failure.ErrorMessage)
                .Where(message =>
                    !string.IsNullOrWhiteSpace(
                        message))
                .Distinct(
                    StringComparer.Ordinal)
                .ToArray();

        return messages.Length == 0
            ? "Göndərilən məlumatlar yanlışdır."
            : string.Join(
                " ",
                messages);
    }
}
