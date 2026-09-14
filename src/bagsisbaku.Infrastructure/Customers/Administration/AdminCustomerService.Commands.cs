using bagsisbaku.Application.Common.Results;
using bagsisbaku.Application.Customers.Administration;
using bagsisbaku.Application.Security;
using Microsoft.EntityFrameworkCore;

namespace bagsisbaku.Infrastructure.Customers.Administration;

internal sealed partial class AdminCustomerService
{
    public async Task<Result<AdminCustomerDetailsModel>>
        SetStatusAsync(
            SetCustomerStatusCommand command,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validationResult =
            await _statusValidator.ValidateAsync(
                command,
                cancellationToken);

        if (!validationResult.IsValid)
        {
            return Result.Failure<
                AdminCustomerDetailsModel>(
                    AdminCustomerErrors.Validation(
                        CreateValidationMessage(
                            validationResult.Errors)));
        }

        var authorizationResult =
            await GetAuthorizedAdminAsync(
                PermissionNames.Customers.Manage,
                cancellationToken);

        if (authorizationResult.IsFailure)
        {
            return Result.Failure<
                AdminCustomerDetailsModel>(
                    authorizationResult.Error);
        }

        var executionStrategy =
            _dbContext.Database
                .CreateExecutionStrategy();

        Result updateResult;

        try
        {
            updateResult =
                await executionStrategy.ExecuteAsync(
                    async () =>
                    {
                        await using var transaction =
                            await _dbContext.Database
                                .BeginTransactionAsync(
                                    cancellationToken);

                        var customer =
                            await CustomerUsersQuery(
                                    asNoTracking: false)
                                .SingleOrDefaultAsync(
                                    currentCustomer =>
                                        currentCustomer.Id ==
                                        command.CustomerId,
                                    cancellationToken);

                        if (customer is null)
                        {
                            return Result.Failure(
                                AdminCustomerErrors
                                    .CustomerNotFound);
                        }

                        if (customer.IsActive ==
                            command.IsActive)
                        {
                            await transaction.CommitAsync(
                                cancellationToken);

                            return Result.Success();
                        }

                        var utcNow =
                            _clock.UtcNow;

                        if (command.IsActive)
                        {
                            customer.Activate(utcNow);
                        }
                        else
                        {
                            customer.Deactivate(utcNow);
                        }

                        var identityResult =
                            await _userManager.UpdateAsync(
                                customer);

                        if (!identityResult.Succeeded)
                        {
                            return Result.Failure(
                                ToCustomerIdentityError(
                                    identityResult));
                        }

                        if (!command.IsActive)
                        {
                            await RevokeActiveRefreshTokensAsync(
                                customer.Id,
                                utcNow,
                                cancellationToken);
                        }

                        await transaction.CommitAsync(
                            cancellationToken);

                        return Result.Success();
                    });
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure<
                AdminCustomerDetailsModel>(
                    AdminCustomerErrors
                        .ConcurrencyConflict);
        }

        if (updateResult.IsFailure)
        {
            return Result.Failure<
                AdminCustomerDetailsModel>(
                    updateResult.Error);
        }

        return await GetDetailsCoreAsync(
            command.CustomerId,
            cancellationToken);
    }
}