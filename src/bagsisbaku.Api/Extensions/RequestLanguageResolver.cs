using bagsisbaku.Domain.Localization;

namespace bagsisbaku.Api.Extensions;

internal static class RequestLanguageResolver
{
    public static SupportedLanguage ResolveLanguage(
        this HttpRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var queryLanguage =
            request.Query["lang"].ToString();

        if (TryParse(
                queryLanguage,
                out var language))
        {
            return language;
        }

        return SupportedLanguage.Azerbaijani;
    }

    private static bool TryParse(
        string? value,
        out SupportedLanguage language)
    {
        language =
            SupportedLanguage.Azerbaijani;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalizedValue =
            value.Trim().ToLowerInvariant();

        if (normalizedValue.StartsWith(
                "az",
                StringComparison.Ordinal))
        {
            language =
                SupportedLanguage.Azerbaijani;

            return true;
        }

        if (normalizedValue.StartsWith(
                "ru",
                StringComparison.Ordinal))
        {
            language =
                SupportedLanguage.Russian;

            return true;
        }

        if (normalizedValue.StartsWith(
                "en",
                StringComparison.Ordinal))
        {
            language =
                SupportedLanguage.English;

            return true;
        }

        return false;
    }
}