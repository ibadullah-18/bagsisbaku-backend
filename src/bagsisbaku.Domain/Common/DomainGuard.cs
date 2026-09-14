namespace bagsisbaku.Domain.Common;

internal static class DomainGuard
{
    public static string Required(
        string? value,
        string fieldName,
        int maxLength)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxLength);

        var normalizedValue = value?.Trim();

        if (string.IsNullOrWhiteSpace(normalizedValue))
        {
            throw new DomainException(
                $"{fieldName} boş ola bilməz.");
        }

        if (normalizedValue.Length > maxLength)
        {
            throw new DomainException(
                $"{fieldName} maksimum {maxLength} simvol ola bilər.");
        }

        return normalizedValue;
    }

    public static string? Optional(
        string? value,
        string fieldName,
        int maxLength)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxLength);

        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Required(
            value,
            fieldName,
            maxLength);
    }

    public static Guid NotEmpty(
        Guid value,
        string fieldName)
    {
        if (value == Guid.Empty)
        {
            throw new DomainException(
                $"{fieldName} boş ola bilməz.");
        }

        return value;
    }

    public static int NotNegative(
        int value,
        string fieldName)
    {
        if (value < 0)
        {
            throw new DomainException(
                $"{fieldName} mənfi ola bilməz.");
        }

        return value;
    }

    public static int Positive(
        int value,
        string fieldName)
    {
        if (value <= 0)
        {
            throw new DomainException(
                $"{fieldName} sıfırdan böyük olmalıdır.");
        }

        return value;
    }

    public static decimal Positive(
        decimal value,
        string fieldName)
    {
        if (value <= 0)
        {
            throw new DomainException(
                $"{fieldName} sıfırdan böyük olmalıdır.");
        }

        return value;
    }

    public static TEnum DefinedEnum<TEnum>(
        TEnum value,
        string fieldName)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new DomainException(
                $"{fieldName} düzgün deyil.");
        }

        return value;
    }

    public static string? HexColor(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalizedValue = value
            .Trim()
            .ToUpperInvariant();

        if (!normalizedValue.StartsWith('#'))
        {
            normalizedValue = $"#{normalizedValue}";
        }

        var isValid =
            normalizedValue.Length == 7 &&
            normalizedValue
                .Skip(1)
                .All(Uri.IsHexDigit);

        if (!isValid)
        {
            throw new DomainException(
                "Rəng kodu #RRGGBB formatında olmalıdır.");
        }

        return normalizedValue;
    }
}

