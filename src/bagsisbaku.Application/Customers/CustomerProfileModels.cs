namespace bagsisbaku.Application.Customers;

public sealed record CustomerProfileModel(
    Guid Id,
    string FullName,
    string Email,
    string? PhoneNumber,
    bool EmailConfirmed,
    bool IsActive,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);

public sealed record CustomerAddressModel(
    Guid Id,
    string Title,
    string RecipientFullName,
    string PhoneNumber,
    string City,
    string? District,
    string AddressLine,
    string? PostalCode,
    string? DeliveryNote,
    decimal? Latitude,
    decimal? Longitude,
    bool IsDefault,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);

public sealed record UpdateCustomerProfileCommand(
    string FullName,
    string? PhoneNumber);

public sealed record CreateCustomerAddressCommand(
    string Title,
    string RecipientFullName,
    string PhoneNumber,
    string City,
    string? District,
    string AddressLine,
    string? PostalCode,
    string? DeliveryNote,
    decimal? Latitude,
    decimal? Longitude,
    bool IsDefault);

public sealed record UpdateCustomerAddressCommand(
    string Title,
    string RecipientFullName,
    string PhoneNumber,
    string City,
    string? District,
    string AddressLine,
    string? PostalCode,
    string? DeliveryNote,
    decimal? Latitude,
    decimal? Longitude);