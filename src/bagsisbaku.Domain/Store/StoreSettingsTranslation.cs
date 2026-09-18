using bagsisbaku.Domain.Common;
using bagsisbaku.Domain.Localization;

namespace bagsisbaku.Domain.Store;

public sealed class StoreSettingsTranslation
    : AuditableEntity
{
    private StoreSettingsTranslation()
    {
    }

    private StoreSettingsTranslation(
        Guid id,
        Guid storeSettingsId,
        SupportedLanguage language,
        string address,
        string workingHours,
        string deliveryInformation,
        string returnPolicy,
        string aboutText)
        : base(id)
    {
        StoreSettingsId =
            DomainGuard.NotEmpty(
                storeSettingsId,
                nameof(StoreSettingsId));

        Language =
            ValidateLanguage(
                language);

        Apply(
            address,
            workingHours,
            deliveryInformation,
            returnPolicy,
            aboutText);
    }

    public Guid StoreSettingsId { get; private set; }

    public SupportedLanguage Language
    {
        get;
        private set;
    }

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

    public static StoreSettingsTranslation Create(
        Guid storeSettingsId,
        SupportedLanguage language,
        string address,
        string workingHours,
        string deliveryInformation,
        string returnPolicy,
        string aboutText)
    {
        return new StoreSettingsTranslation(
            Guid.NewGuid(),
            storeSettingsId,
            language,
            address,
            workingHours,
            deliveryInformation,
            returnPolicy,
            aboutText);
    }

    public void Update(
        string address,
        string workingHours,
        string deliveryInformation,
        string returnPolicy,
        string aboutText)
    {
        Apply(
            address,
            workingHours,
            deliveryInformation,
            returnPolicy,
            aboutText);
    }

    private void Apply(
        string address,
        string workingHours,
        string deliveryInformation,
        string returnPolicy,
        string aboutText)
    {
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
    }

    private static SupportedLanguage ValidateLanguage(
        SupportedLanguage language)
    {
        var validLanguage =
            DomainGuard.DefinedEnum(
                language,
                nameof(Language));

        if (validLanguage ==
            SupportedLanguage.Azerbaijani)
        {
            throw new DomainException(
                "Azərbaycan dili əsas mağaza məlumatında saxlanılır.");
        }

        return validLanguage;
    }
}