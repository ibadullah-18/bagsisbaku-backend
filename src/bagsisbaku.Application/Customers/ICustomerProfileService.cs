using bagsisbaku.Application.Common.Results;

namespace bagsisbaku.Application.Customers;

public interface ICustomerProfileService
{
    Task<Result<CustomerProfileModel>> GetProfileAsync(
        CancellationToken cancellationToken = default);

    Task<Result<CustomerProfileModel>> UpdateProfileAsync(
        UpdateCustomerProfileCommand command,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<CustomerAddressModel>>>
        GetAddressesAsync(
            CancellationToken cancellationToken = default);

    Task<Result<CustomerAddressModel>> GetAddressByIdAsync(
        Guid addressId,
        CancellationToken cancellationToken = default);

    Task<Result<CustomerAddressModel>> CreateAddressAsync(
        CreateCustomerAddressCommand command,
        CancellationToken cancellationToken = default);

    Task<Result<CustomerAddressModel>> UpdateAddressAsync(
        Guid addressId,
        UpdateCustomerAddressCommand command,
        CancellationToken cancellationToken = default);

    Task<Result> SetDefaultAddressAsync(
        Guid addressId,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteAddressAsync(
        Guid addressId,
        CancellationToken cancellationToken = default);
}