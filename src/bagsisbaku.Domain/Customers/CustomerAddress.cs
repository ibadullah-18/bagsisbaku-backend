using bagsisbaku.Domain.Common;

namespace bagsisbaku.Domain.Customers;

public sealed class CustomerAddress : AuditableEntity
{
    private CustomerAddress()
    {
    }

    private CustomerAddress(
        Guid id,
        Guid userId,
        string title,
        string recipientFullName,
        string phoneNumber,
        string city,
        string? district,
        string addressLine,
        string? postalCode,
        string? deliveryNote,
        decimal? latitude,
        decimal? longitude,
        bool isDefault)
        : base(id)
    {
        UserId = DomainGuard.NotEmpty(
            userId,
            nameof(UserId));

        Update(
            title,
            recipientFullName,
            phoneNumber,
            city,
            district,
            addressLine,
            postalCode,
            deliveryNote,
            latitude,
            longitude);

        IsDefault = isDefault;
    }

    public Guid UserId { get; private set; }

    public string Title { get; private set; } =
        string.Empty;

    public string RecipientFullName { get; private set; } =
        string.Empty;

    public string PhoneNumber { get; private set; } =
        string.Empty;

    public string City { get; private set; } =
        string.Empty;

    public string? District { get; private set; }

    public string AddressLine { get; private set; } =
        string.Empty;

    public string? PostalCode { get; private set; }

    public string? DeliveryNote { get; private set; }

    public decimal? Latitude { get; private set; }

    public decimal? Longitude { get; private set; }

    public bool IsDefault { get; private set; }

    public static CustomerAddress Create(
        Guid userId,
        string title,
        string recipientFullName,
        string phoneNumber,
        string city,
        string? district,
        string addressLine,
        string? postalCode,
        string? deliveryNote,
        decimal? latitude = null,
        decimal? longitude = null,
        bool isDefault = false)
    {
        return new CustomerAddress(
            Guid.NewGuid(),
            userId,
            title,
            recipientFullName,
            phoneNumber,
            city,
            district,
            addressLine,
            postalCode,
            deliveryNote,
            latitude,
            longitude,
            isDefault);
    }

    public void Update(
        string title,
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
        ValidateCoordinates(
            latitude,
            longitude);

        Title = DomainGuard.Required(
            title,
            nameof(Title),
            50);

        RecipientFullName = DomainGuard.Required(
            recipientFullName,
            nameof(RecipientFullName),
            160);

        PhoneNumber = DomainGuard.Required(
            phoneNumber,
            nameof(PhoneNumber),
            32);

        City = DomainGuard.Required(
            city,
            nameof(City),
            100);

        District = DomainGuard.Optional(
            district,
            nameof(District),
            120);

        AddressLine = DomainGuard.Required(
            addressLine,
            nameof(AddressLine),
            500);

        PostalCode = DomainGuard.Optional(
            postalCode,
            nameof(PostalCode),
            20);

        DeliveryNote = DomainGuard.Optional(
            deliveryNote,
            nameof(DeliveryNote),
            500);

        Latitude = latitude;
        Longitude = longitude;
    }

    public void SetAsDefault()
    {
        IsDefault = true;
    }

    public void RemoveDefault()
    {
        IsDefault = false;
    }

    private static void ValidateCoordinates(
        decimal? latitude,
        decimal? longitude)
    {
        if (latitude.HasValue != longitude.HasValue)
        {
            throw new DomainException(
                "Latitude və longitude birlikdə daxil edilməlidir.");
        }

        if (latitude.HasValue &&
            (latitude.Value < -90m ||
             latitude.Value > 90m))
        {
            throw new DomainException(
                "Latitude -90 və 90 arasında olmalıdır.");
        }

        if (longitude.HasValue &&
            (longitude.Value < -180m ||
             longitude.Value > 180m))
        {
            throw new DomainException(
                "Longitude -180 və 180 arasında olmalıdır.");
        }
    }
}