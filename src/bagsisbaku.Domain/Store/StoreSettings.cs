using bagsisbaku.Domain.Common;

namespace bagsisbaku.Domain.Store;

public sealed class StoreSettings : AuditableEntity
{
    public const string BrandName =
        "bagsisbaku";

    private StoreSettings()
    {
    }

    private StoreSettings(
        Guid id,
        string primaryPhone,
        string whatsappPhone,
        string? email,
        string? instagramUrl,
        string? tiktokUrl,
        string address,
        string workingHours,
        string deliveryInformation,
        string returnPolicy,
        string aboutText,
        decimal latitude,
        decimal longitude,
        string? mapUrl)
        : base(id)
    {
        StoreName = BrandName;

        Apply(
            primaryPhone,
            whatsappPhone,
            email,
            instagramUrl,
            tiktokUrl,
            address,
            workingHours,
            deliveryInformation,
            returnPolicy,
            aboutText,
            latitude,
            longitude,
            mapUrl);
    }

    public string StoreName { get; private set; } =
        BrandName;

    public string PrimaryPhone { get; private set; } =
        string.Empty;

    public string WhatsAppPhone { get; private set; } =
        string.Empty;

    public string? Email { get; private set; }

    public string? InstagramUrl { get; private set; }

    public string? TikTokUrl { get; private set; }

    public string Address { get; private set; } =
        string.Empty;

    public string WorkingHours { get; private set; } =
        string.Empty;

    public string DeliveryInformation
    {
        get;
        private set;
    } = string.Empty;

    public string ReturnPolicy { get; private set; } =
        string.Empty;

    public string AboutText { get; private set; } =
        string.Empty;

    public decimal Latitude { get; private set; }

    public decimal Longitude { get; private set; }

    public string? MapUrl { get; private set; }

    public string? LogoUrl { get; private set; }

    public string? LogoPublicId { get; private set; }

    public byte[] RowVersion { get; private set; } = [];

    public static StoreSettings Create(
        string primaryPhone,
        string whatsappPhone,
        string? email,
        string? instagramUrl,
        string? tiktokUrl,
        string address,
        string workingHours,
        string deliveryInformation,
        string returnPolicy,
        string aboutText,
        decimal latitude,
        decimal longitude,
        string? mapUrl)
    {
        return new StoreSettings(
            Guid.NewGuid(),
            primaryPhone,
            whatsappPhone,
            email,
            instagramUrl,
            tiktokUrl,
            address,
            workingHours,
            deliveryInformation,
            returnPolicy,
            aboutText,
            latitude,
            longitude,
            mapUrl);
    }

    public void Update(
        string primaryPhone,
        string whatsappPhone,
        string? email,
        string? instagramUrl,
        string? tiktokUrl,
        string address,
        string workingHours,
        string deliveryInformation,
        string returnPolicy,
        string aboutText,
        decimal latitude,
        decimal longitude,
        string? mapUrl)
    {
        Apply(
            primaryPhone,
            whatsappPhone,
            email,
            instagramUrl,
            tiktokUrl,
            address,
            workingHours,
            deliveryInformation,
            returnPolicy,
            aboutText,
            latitude,
            longitude,
            mapUrl);
    }

    public void SetLogo(
        string logoUrl,
        string logoPublicId)
    {
        LogoUrl =
            DomainGuard.Required(
                logoUrl,
                nameof(LogoUrl),
                2048);

        LogoPublicId =
            DomainGuard.Required(
                logoPublicId,
                nameof(LogoPublicId),
                255);
    }

    public void RemoveLogo()
    {
        LogoUrl = null;
        LogoPublicId = null;
    }

    private void Apply(
        string primaryPhone,
        string whatsappPhone,
        string? email,
        string? instagramUrl,
        string? tiktokUrl,
        string address,
        string workingHours,
        string deliveryInformation,
        string returnPolicy,
        string aboutText,
        decimal latitude,
        decimal longitude,
        string? mapUrl)
    {
        ValidateCoordinates(
            latitude,
            longitude);

        StoreName = BrandName;

        PrimaryPhone =
            DomainGuard.Required(
                primaryPhone,
                nameof(PrimaryPhone),
                32);

        WhatsAppPhone =
            DomainGuard.Required(
                whatsappPhone,
                nameof(WhatsAppPhone),
                32);

        Email =
            NormalizeOptional(
                email,
                nameof(Email),
                256);

        InstagramUrl =
            NormalizeOptional(
                instagramUrl,
                nameof(InstagramUrl),
                2048);

        TikTokUrl =
            NormalizeOptional(
                tiktokUrl,
                nameof(TikTokUrl),
                2048);

        Address =
            DomainGuard.Required(
                address,
                nameof(Address),
                500);

        WorkingHours =
            DomainGuard.Required(
                workingHours,
                nameof(WorkingHours),
                250);

        DeliveryInformation =
            DomainGuard.Required(
                deliveryInformation,
                nameof(DeliveryInformation),
                2000);

        ReturnPolicy =
            DomainGuard.Required(
                returnPolicy,
                nameof(ReturnPolicy),
                2000);

        AboutText =
            DomainGuard.Required(
                aboutText,
                nameof(AboutText),
                4000);

        Latitude = latitude;
        Longitude = longitude;

        MapUrl =
            NormalizeOptional(
                mapUrl,
                nameof(MapUrl),
                2048);
    }

    private static string? NormalizeOptional(
        string? value,
        string fieldName,
        int maximumLength)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : DomainGuard.Required(
                value,
                fieldName,
                maximumLength);
    }

    private static void ValidateCoordinates(
        decimal latitude,
        decimal longitude)
    {
        if (latitude is < -90m or > 90m)
        {
            throw new DomainException(
                "Latitude -90 və 90 aralığında olmalıdır.");
        }

        if (longitude is < -180m or > 180m)
        {
            throw new DomainException(
                "Longitude -180 və 180 aralığında olmalıdır.");
        }
    }
}