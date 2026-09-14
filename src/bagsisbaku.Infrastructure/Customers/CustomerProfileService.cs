using bagsisbaku.Application.Abstractions.Authentication;
using bagsisbaku.Application.Abstractions.Time;
using bagsisbaku.Application.Common.Results;
using bagsisbaku.Application.Customers;
using bagsisbaku.Domain.Common;
using bagsisbaku.Domain.Customers;
using bagsisbaku.Infrastructure.Identity;
using bagsisbaku.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace bagsisbaku.Infrastructure.Customers;

internal sealed class CustomerProfileService(
    ApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IClock clock)
    : ICustomerProfileService
{
    public async Task<Result<CustomerProfileModel>>
        GetProfileAsync(
            CancellationToken cancellationToken = default)
    {
        var userResult =
            await GetCurrentUserAsync(
                cancellationToken);

        if (userResult.IsFailure)
        {
            return Result.Failure<CustomerProfileModel>(
                userResult.Error);
        }

        return Result.Success(
            ToProfileModel(userResult.Value));
    }

    public async Task<Result<CustomerProfileModel>>
        UpdateProfileAsync(
            UpdateCustomerProfileCommand command,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var userResult =
            await GetCurrentUserAsync(
                cancellationToken);

        if (userResult.IsFailure)
        {
            return Result.Failure<CustomerProfileModel>(
                userResult.Error);
        }

        var user = userResult.Value;

        try
        {
            user.UpdateProfile(
                command.FullName,
                command.PhoneNumber,
                clock.UtcNow);

            await dbContext.SaveChangesAsync(
                cancellationToken);

            return Result.Success(
                ToProfileModel(user));
        }
        catch (ArgumentException exception)
        {
            return Result.Failure<CustomerProfileModel>(
                CustomerProfileErrors.Validation(
                    exception.Message));
        }
    }

    public async Task<
        Result<IReadOnlyList<CustomerAddressModel>>>
        GetAddressesAsync(
            CancellationToken cancellationToken = default)
    {
        var userResult =
            await GetCurrentUserAsync(
                cancellationToken);

        if (userResult.IsFailure)
        {
            return Result.Failure<
                IReadOnlyList<CustomerAddressModel>>(
                    userResult.Error);
        }

        var userId = userResult.Value.Id;

        var addresses =
            await dbContext.CustomerAddresses
                .AsNoTracking()
                .Where(address =>
                    address.UserId == userId)
                .OrderByDescending(address =>
                    address.IsDefault)
                .ThenBy(address =>
                    address.Title)
                .Select(address =>
                    new CustomerAddressModel(
                        address.Id,
                        address.Title,
                        address.RecipientFullName,
                        address.PhoneNumber,
                        address.City,
                        address.District,
                        address.AddressLine,
                        address.PostalCode,
                        address.DeliveryNote,
                        address.Latitude,
                        address.Longitude,
                        address.IsDefault,
                        address.CreatedAtUtc,
                        address.UpdatedAtUtc))
                .ToArrayAsync(cancellationToken);

        return Result.Success<
            IReadOnlyList<CustomerAddressModel>>(
                addresses);
    }

    public async Task<Result<CustomerAddressModel>>
        GetAddressByIdAsync(
            Guid addressId,
            CancellationToken cancellationToken = default)
    {
        if (addressId == Guid.Empty)
        {
            return Result.Failure<CustomerAddressModel>(
                CustomerProfileErrors.Validation(
                    "AddressId boş ola bilməz."));
        }

        var userResult =
            await GetCurrentUserAsync(
                cancellationToken);

        if (userResult.IsFailure)
        {
            return Result.Failure<CustomerAddressModel>(
                userResult.Error);
        }

        var userId = userResult.Value.Id;

        var address =
            await dbContext.CustomerAddresses
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    currentAddress =>
                        currentAddress.Id == addressId &&
                        currentAddress.UserId == userId,
                    cancellationToken);

        return address is null
            ? Result.Failure<CustomerAddressModel>(
                CustomerProfileErrors.AddressNotFound)
            : Result.Success(
                ToAddressModel(address));
    }

    public async Task<Result<CustomerAddressModel>>
        CreateAddressAsync(
            CreateCustomerAddressCommand command,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var userResult =
            await GetCurrentUserAsync(
                cancellationToken);

        if (userResult.IsFailure)
        {
            return Result.Failure<CustomerAddressModel>(
                userResult.Error);
        }

        var userId = userResult.Value.Id;

        var hasAnyAddress =
            await dbContext.CustomerAddresses
                .AnyAsync(
                    address =>
                        address.UserId == userId,
                    cancellationToken);

        var shouldBeDefault =
            command.IsDefault ||
            !hasAnyAddress;

        CustomerAddress address;

        try
        {
            address = CustomerAddress.Create(
                userId,
                command.Title,
                command.RecipientFullName,
                command.PhoneNumber,
                command.City,
                command.District,
                command.AddressLine,
                command.PostalCode,
                command.DeliveryNote,
                command.Latitude,
                command.Longitude,
                shouldBeDefault);
        }
        catch (Exception exception)
            when (exception is ArgumentException or
                  DomainException)
        {
            return Result.Failure<CustomerAddressModel>(
                CustomerProfileErrors.Validation(
                    exception.Message));
        }

        if (!shouldBeDefault)
        {
            dbContext.CustomerAddresses.Add(address);

            await dbContext.SaveChangesAsync(
                cancellationToken);

            return Result.Success(
                ToAddressModel(address));
        }

        return await ExecuteInTransactionAsync(
            async () =>
            {
                var currentDefaults =
                    await dbContext.CustomerAddresses
                        .Where(currentAddress =>
                            currentAddress.UserId ==
                                userId &&
                            currentAddress.IsDefault)
                        .ToListAsync(
                            cancellationToken);

                foreach (var currentDefault
                         in currentDefaults)
                {
                    currentDefault.RemoveDefault();
                }

                if (currentDefaults.Count > 0)
                {
                    await dbContext.SaveChangesAsync(
                        cancellationToken);
                }

                dbContext.CustomerAddresses.Add(
                    address);

                await dbContext.SaveChangesAsync(
                    cancellationToken);

                return Result.Success(
                    ToAddressModel(address));
            },
            cancellationToken);
    }

    public async Task<Result<CustomerAddressModel>>
        UpdateAddressAsync(
            Guid addressId,
            UpdateCustomerAddressCommand command,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (addressId == Guid.Empty)
        {
            return Result.Failure<CustomerAddressModel>(
                CustomerProfileErrors.Validation(
                    "AddressId boş ola bilməz."));
        }

        var userResult =
            await GetCurrentUserAsync(
                cancellationToken);

        if (userResult.IsFailure)
        {
            return Result.Failure<CustomerAddressModel>(
                userResult.Error);
        }

        var userId = userResult.Value.Id;

        var address =
            await dbContext.CustomerAddresses
                .SingleOrDefaultAsync(
                    currentAddress =>
                        currentAddress.Id == addressId &&
                        currentAddress.UserId == userId,
                    cancellationToken);

        if (address is null)
        {
            return Result.Failure<CustomerAddressModel>(
                CustomerProfileErrors.AddressNotFound);
        }

        try
        {
            address.Update(
                command.Title,
                command.RecipientFullName,
                command.PhoneNumber,
                command.City,
                command.District,
                command.AddressLine,
                command.PostalCode,
                command.DeliveryNote,
                command.Latitude,
                command.Longitude);

            await dbContext.SaveChangesAsync(
                cancellationToken);

            return Result.Success(
                ToAddressModel(address));
        }
        catch (Exception exception)
            when (exception is ArgumentException or
                  DomainException)
        {
            return Result.Failure<CustomerAddressModel>(
                CustomerProfileErrors.Validation(
                    exception.Message));
        }
    }

    public async Task<Result> SetDefaultAddressAsync(
        Guid addressId,
        CancellationToken cancellationToken = default)
    {
        if (addressId == Guid.Empty)
        {
            return Result.Failure(
                CustomerProfileErrors.Validation(
                    "AddressId boş ola bilməz."));
        }

        var userResult =
            await GetCurrentUserAsync(
                cancellationToken);

        if (userResult.IsFailure)
        {
            return Result.Failure(
                userResult.Error);
        }

        var userId = userResult.Value.Id;

        var targetAddress =
            await dbContext.CustomerAddresses
                .SingleOrDefaultAsync(
                    address =>
                        address.Id == addressId &&
                        address.UserId == userId,
                    cancellationToken);

        if (targetAddress is null)
        {
            return Result.Failure(
                CustomerProfileErrors.AddressNotFound);
        }

        if (targetAddress.IsDefault)
        {
            return Result.Success();
        }

        return await ExecuteInTransactionAsync(
            async () =>
            {
                var currentDefaults =
                    await dbContext.CustomerAddresses
                        .Where(address =>
                            address.UserId == userId &&
                            address.IsDefault &&
                            address.Id != addressId)
                        .ToListAsync(
                            cancellationToken);

                foreach (var currentDefault
                         in currentDefaults)
                {
                    currentDefault.RemoveDefault();
                }

                if (currentDefaults.Count > 0)
                {
                    await dbContext.SaveChangesAsync(
                        cancellationToken);
                }

                targetAddress.SetAsDefault();

                await dbContext.SaveChangesAsync(
                    cancellationToken);

                return Result.Success();
            },
            cancellationToken);
    }

    public async Task<Result> DeleteAddressAsync(
        Guid addressId,
        CancellationToken cancellationToken = default)
    {
        if (addressId == Guid.Empty)
        {
            return Result.Failure(
                CustomerProfileErrors.Validation(
                    "AddressId boş ola bilməz."));
        }

        var userResult =
            await GetCurrentUserAsync(
                cancellationToken);

        if (userResult.IsFailure)
        {
            return Result.Failure(
                userResult.Error);
        }

        var userId = userResult.Value.Id;

        var addresses =
            await dbContext.CustomerAddresses
                .Where(address =>
                    address.UserId == userId)
                .OrderBy(address =>
                    address.CreatedAtUtc)
                .ToListAsync(
                    cancellationToken);

        var address =
            addresses.SingleOrDefault(
                currentAddress =>
                    currentAddress.Id == addressId);

        if (address is null)
        {
            return Result.Failure(
                CustomerProfileErrors.AddressNotFound);
        }

        var replacement =
            address.IsDefault
                ? addresses.FirstOrDefault(
                    currentAddress =>
                        currentAddress.Id != addressId)
                : null;

        return await ExecuteInTransactionAsync(
            async () =>
            {
                dbContext.CustomerAddresses.Remove(
                    address);

                await dbContext.SaveChangesAsync(
                    cancellationToken);

                if (replacement is not null)
                {
                    replacement.SetAsDefault();

                    await dbContext.SaveChangesAsync(
                        cancellationToken);
                }

                return Result.Success();
            },
            cancellationToken);
    }

    private async Task<Result<AppUser>>
        GetCurrentUserAsync(
            CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated ||
            currentUser.UserId is not Guid userId ||
            userId == Guid.Empty)
        {
            return Result.Failure<AppUser>(
                CustomerProfileErrors
                    .AuthenticationRequired);
        }

        var user =
            await dbContext.Users
                .SingleOrDefaultAsync(
                    currentUserRecord =>
                        currentUserRecord.Id == userId,
                    cancellationToken);

        if (user is null)
        {
            return Result.Failure<AppUser>(
                CustomerProfileErrors
                    .CustomerNotFound);
        }

        if (!user.IsActive)
        {
            return Result.Failure<AppUser>(
                CustomerProfileErrors
                    .CustomerInactive);
        }

        return Result.Success(user);
    }

    private async Task<T> ExecuteInTransactionAsync<T>(
        Func<Task<T>> operation,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operation);

        var executionStrategy =
            dbContext.Database
                .CreateExecutionStrategy();

        return await executionStrategy.ExecuteAsync(
            async () =>
            {
                await using var transaction =
                    await dbContext.Database
                        .BeginTransactionAsync(
                            cancellationToken);

                var result =
                    await operation();

                await transaction.CommitAsync(
                    cancellationToken);

                return result;
            });
    }

    private static CustomerProfileModel ToProfileModel(
        AppUser user)
    {
        return new CustomerProfileModel(
            user.Id,
            user.FullName,
            user.Email ?? string.Empty,
            user.PhoneNumber,
            user.EmailConfirmed,
            user.IsActive,
            user.CreatedAtUtc,
            user.UpdatedAtUtc);
    }

    private static CustomerAddressModel ToAddressModel(
        CustomerAddress address)
    {
        return new CustomerAddressModel(
            address.Id,
            address.Title,
            address.RecipientFullName,
            address.PhoneNumber,
            address.City,
            address.District,
            address.AddressLine,
            address.PostalCode,
            address.DeliveryNote,
            address.Latitude,
            address.Longitude,
            address.IsDefault,
            address.CreatedAtUtc,
            address.UpdatedAtUtc);
    }
}