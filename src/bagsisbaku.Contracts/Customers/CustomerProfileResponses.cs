namespace bagsisbaku.Contracts.Customers;

public sealed record CustomerProfileResponse(
    Guid Id,
    string FullName,
    string Email,
    string? PhoneNumber,
    bool EmailConfirmed,
    bool IsActive,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);

public sealed record CustomerAddressResponse(
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