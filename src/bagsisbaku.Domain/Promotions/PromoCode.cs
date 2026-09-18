using bagsisbaku.Domain.Common;

namespace bagsisbaku.Domain.Promotions;

public sealed class PromoCode : AuditableEntity
{
    private PromoCode()
    {
    }

    private PromoCode(
        Guid id,
        string name,
        string code,
        PromotionDiscountType discountType,
        decimal discountValue,
        decimal minimumOrderAmount,
        decimal? maximumDiscountAmount,
        int? usageLimit,
        int? perCustomerUsageLimit,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc)
        : base(id)
    {
        Name =
            NormalizeName(name);

        Code =
            NormalizeCode(code);

        DiscountType =
            DomainGuard.DefinedEnum(
                discountType,
                nameof(DiscountType));

        ValidateRules(
            DiscountType,
            discountValue,
            minimumOrderAmount,
            maximumDiscountAmount,
            usageLimit,
            perCustomerUsageLimit,
            startsAtUtc,
            endsAtUtc);

        DiscountValue = discountValue;
        MinimumOrderAmount = minimumOrderAmount;
        MaximumDiscountAmount = maximumDiscountAmount;
        UsageLimit = usageLimit;
        PerCustomerUsageLimit = perCustomerUsageLimit;
        StartsAtUtc = startsAtUtc;
        EndsAtUtc = endsAtUtc;
        UsageCount = 0;
        IsActive = true;
    }

    public string Name { get; private set; } =
        string.Empty;

    public string Code { get; private set; } =
        string.Empty;

    public PromotionDiscountType DiscountType
    {
        get;
        private set;
    }

    public decimal DiscountValue { get; private set; }

    public decimal MinimumOrderAmount
    {
        get;
        private set;
    }

    public decimal? MaximumDiscountAmount
    {
        get;
        private set;
    }

    public int? UsageLimit { get; private set; }

    public int UsageCount { get; private set; }

    public int? PerCustomerUsageLimit
    {
        get;
        private set;
    }

    public DateTimeOffset StartsAtUtc
    {
        get;
        private set;
    }

    public DateTimeOffset EndsAtUtc
    {
        get;
        private set;
    }

    public bool IsActive { get; private set; }

    public byte[] RowVersion { get; private set; } = [];

    public static PromoCode Create(
        string name,
        string code,
        PromotionDiscountType discountType,
        decimal discountValue,
        decimal minimumOrderAmount,
        decimal? maximumDiscountAmount,
        int? usageLimit,
        int? perCustomerUsageLimit,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc)
    {
        return new PromoCode(
            Guid.NewGuid(),
            name,
            code,
            discountType,
            discountValue,
            minimumOrderAmount,
            maximumDiscountAmount,
            usageLimit,
            perCustomerUsageLimit,
            startsAtUtc,
            endsAtUtc);
    }

    public void Update(
        string name,
        string code,
        PromotionDiscountType discountType,
        decimal discountValue,
        decimal minimumOrderAmount,
        decimal? maximumDiscountAmount,
        int? usageLimit,
        int? perCustomerUsageLimit,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc)
    {
        var normalizedName =
            NormalizeName(name);

        var normalizedCode =
            NormalizeCode(code);

        var validDiscountType =
            DomainGuard.DefinedEnum(
                discountType,
                nameof(discountType));

        ValidateRules(
            validDiscountType,
            discountValue,
            minimumOrderAmount,
            maximumDiscountAmount,
            usageLimit,
            perCustomerUsageLimit,
            startsAtUtc,
            endsAtUtc);

        if (usageLimit.HasValue &&
            usageLimit.Value < UsageCount)
        {
            throw new DomainException(
                "Ümumi istifadə limiti mövcud istifadə sayından az ola bilməz.");
        }

        Name = normalizedName;
        Code = normalizedCode;
        DiscountType = validDiscountType;
        DiscountValue = discountValue;
        MinimumOrderAmount = minimumOrderAmount;
        MaximumDiscountAmount = maximumDiscountAmount;
        UsageLimit = usageLimit;
        PerCustomerUsageLimit = perCustomerUsageLimit;
        StartsAtUtc = startsAtUtc;
        EndsAtUtc = endsAtUtc;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public bool IsAvailableAt(
        DateTimeOffset utcNow)
    {
        if (!IsActive)
        {
            return false;
        }

        if (utcNow < StartsAtUtc ||
            utcNow >= EndsAtUtc)
        {
            return false;
        }

        return !UsageLimit.HasValue ||
            UsageCount < UsageLimit.Value;
    }

    public bool MeetsMinimumOrder(
        decimal orderAmount)
    {
        return orderAmount >= MinimumOrderAmount;
    }

    public decimal CalculateDiscount(
        decimal orderAmount)
    {
        if (orderAmount <= 0)
        {
            throw new DomainException(
                "Sifariş məbləği sıfırdan böyük olmalıdır.");
        }

        if (!MeetsMinimumOrder(orderAmount))
        {
            throw new DomainException(
                "Sifariş məbləği promo kodun minimum məbləğ şərtini ödəmir.");
        }

        var discount =
            DiscountType ==
            PromotionDiscountType.Percentage
                ? orderAmount *
                    DiscountValue /
                    100m
                : DiscountValue;

        if (MaximumDiscountAmount.HasValue &&
            discount >
            MaximumDiscountAmount.Value)
        {
            discount =
                MaximumDiscountAmount.Value;
        }

        if (discount > orderAmount)
        {
            discount = orderAmount;
        }

        return decimal.Round(
            discount,
            2,
            MidpointRounding.AwayFromZero);
    }

    public void RegisterUsage(
        DateTimeOffset usedAtUtc)
    {
        if (!IsAvailableAt(usedAtUtc))
        {
            throw new DomainException(
                "Promo kod hazırda istifadə edilə bilməz.");
        }

        UsageCount++;
    }

    public void ReleaseUsage()
    {
        if (UsageCount <= 0)
        {
            throw new DomainException(
                "Promo kod istifadə sayı sıfırdan aşağı düşə bilməz.");
        }

        UsageCount--;
    }
    private static string NormalizeName(
        string name)
    {
        return DomainGuard.Required(
            name,
            nameof(Name),
            120);
    }

    private static string NormalizeCode(
        string code)
    {
        var normalized =
            DomainGuard.Required(
                    code,
                    nameof(Code),
                    40)
                .ToUpperInvariant();

        if (normalized.Any(
            character =>
                !IsAllowedCodeCharacter(
                    character)))
        {
            throw new DomainException(
                "Promo kod yalnız A-Z hərfləri, rəqəmlər, tire və alt xətdən ibarət ola bilər.");
        }

        return normalized;
    }

    private static bool IsAllowedCodeCharacter(
        char character)
    {
        return character is
            (>= 'A' and <= 'Z') or
            (>= '0' and <= '9') or
            '-' or
            '_';
    }

    private static void ValidateRules(
        PromotionDiscountType discountType,
        decimal discountValue,
        decimal minimumOrderAmount,
        decimal? maximumDiscountAmount,
        int? usageLimit,
        int? perCustomerUsageLimit,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc)
    {
        if (discountValue <= 0)
        {
            throw new DomainException(
                "Endirim dəyəri sıfırdan böyük olmalıdır.");
        }

        if (discountType ==
                PromotionDiscountType.Percentage &&
            discountValue > 100)
        {
            throw new DomainException(
                "Faiz endirimi 100-dən böyük ola bilməz.");
        }

        if (minimumOrderAmount < 0)
        {
            throw new DomainException(
                "Minimum sifariş məbləği mənfi ola bilməz.");
        }

        if (maximumDiscountAmount.HasValue &&
            maximumDiscountAmount.Value <= 0)
        {
            throw new DomainException(
                "Maksimum endirim məbləği sıfırdan böyük olmalıdır.");
        }

        if (usageLimit.HasValue &&
            usageLimit.Value <= 0)
        {
            throw new DomainException(
                "Ümumi istifadə limiti sıfırdan böyük olmalıdır.");
        }

        if (perCustomerUsageLimit.HasValue &&
            perCustomerUsageLimit.Value <= 0)
        {
            throw new DomainException(
                "Müştəri istifadə limiti sıfırdan böyük olmalıdır.");
        }

        if (endsAtUtc <= startsAtUtc)
        {
            throw new DomainException(
                "Promo kodun bitmə vaxtı başlama vaxtından sonra olmalıdır.");
        }
    }
}