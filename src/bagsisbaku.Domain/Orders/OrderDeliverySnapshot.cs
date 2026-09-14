using bagsisbaku.Domain.Common;

namespace bagsisbaku.Domain.Orders;

public sealed record OrderDeliverySnapshot
{
    private OrderDeliverySnapshot(
        Guid? customerAddressId,
        string recipientFullName,
        string phoneNumber,
        string? city,
        string? district,
        string? addressLine,
        string? postalCode,
        string? deliveryNote,
        decimal? latitude,
        decimal? longitude)
    {
        CustomerAddressId = customerAddressId;

        RecipientFullName =
            DomainGuard.Required(
                recipientFullName,
                nameof(RecipientFullName),
                160);

        PhoneNumber =
            DomainGuard.Required(
                phoneNumber,
                nameof(PhoneNumber),
                32);

        City = city;
        District = district;
        AddressLine = addressLine;
        PostalCode = postalCode;
        DeliveryNote = deliveryNote;
        Latitude = latitude;
        Longitude = longitude;
    }

    public Guid? CustomerAddressId { get; }

    public string RecipientFullName { get; }

    public string PhoneNumber { get; }

    public string? City { get; }

    public string? District { get; }

    public string? AddressLine { get; }

    public string? PostalCode { get; }

    public string? DeliveryNote { get; }

    public decimal? Latitude { get; }

    public decimal? Longitude { get; }

    public static OrderDeliverySnapshot
        ForStorePickup(
            string recipientFullName,
            string phoneNumber)
    {
        return new OrderDeliverySnapshot(
            null,
            recipientFullName,
            phoneNumber,
            null,
            null,
            null,
            null,
            null,
            null,
            null);
    }

    public static OrderDeliverySnapshot
        ForAddressDelivery(
            Guid customerAddressId,
            string recipientFullName,
            string phoneNumber,
            string city,
            string? district,
            string addressLine,
            string? postalCode,
            string? deliveryNote,
            decimal? latitude,
            decimal? longitude)
    {
        var validAddressId =
            DomainGuard.NotEmpty(
                customerAddressId,
                nameof(customerAddressId));

        var validCity =
            DomainGuard.Required(
                city,
                nameof(city),
                120);

        var validDistrict =
            DomainGuard.Optional(
                district,
                nameof(district),
                120);

        var validAddressLine =
            DomainGuard.Required(
                addressLine,
                nameof(addressLine),
                500);

        var validPostalCode =
            DomainGuard.Optional(
                postalCode,
                nameof(postalCode),
                32);

        var validDeliveryNote =
            DomainGuard.Optional(
                deliveryNote,
                nameof(deliveryNote),
                1000);

        ValidateCoordinates(
            latitude,
            longitude);

        return new OrderDeliverySnapshot(
            validAddressId,
            recipientFullName,
            phoneNumber,
            validCity,
            validDistrict,
            validAddressLine,
            validPostalCode,
            validDeliveryNote,
            latitude,
            longitude);
    }

    private static void ValidateCoordinates(
        decimal? latitude,
        decimal? longitude)
    {
        if (latitude.HasValue != longitude.HasValue)
        {
            throw new DomainException(
                "Latitude və longitude birlikdə verilməlidir.");
        }

        if (latitude is < -90m or > 90m)
        {
            throw new DomainException(
                "Latitude -90 və 90 arasında olmalıdır.");
        }

        if (longitude is < -180m or > 180m)
        {
            throw new DomainException(
                "Longitude -180 və 180 arasında olmalıdır.");
        }
    }
}