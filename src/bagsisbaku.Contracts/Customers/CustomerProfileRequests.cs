namespace bagsisbaku.Contracts.Customers;

public sealed record UpdateCustomerProfileRequest
{
    public required string FullName { get; init; }

    public string? PhoneNumber { get; init; }
}

public sealed record CreateCustomerAddressRequest
{
    public required string Title { get; init; }

    public required string RecipientFullName { get; init; }

    public required string PhoneNumber { get; init; }

    public required string City { get; init; }

    public string? District { get; init; }

    public required string AddressLine { get; init; }

    public string? PostalCode { get; init; }

    public string? DeliveryNote { get; init; }

    public decimal? Latitude { get; init; }

    public decimal? Longitude { get; init; }

    public bool IsDefault { get; init; }
}

public sealed record UpdateCustomerAddressRequest
{
    public required string Title { get; init; }

    public required string RecipientFullName { get; init; }

    public required string PhoneNumber { get; init; }

    public required string City { get; init; }

    public string? District { get; init; }

    public required string AddressLine { get; init; }

    public string? PostalCode { get; init; }

    public string? DeliveryNote { get; init; }

    public decimal? Latitude { get; init; }

    public decimal? Longitude { get; init; }
}