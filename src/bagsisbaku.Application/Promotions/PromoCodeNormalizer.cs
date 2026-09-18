namespace bagsisbaku.Application.Promotions;

public static class PromoCodeNormalizer
{
    public const int MaximumCodeLength = 40;

    public static string? Normalize(
        string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        return code
            .Trim()
            .ToUpperInvariant();
    }
}