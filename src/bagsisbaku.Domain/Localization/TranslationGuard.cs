using bagsisbaku.Domain.Common;

namespace bagsisbaku.Domain.Localization;

internal static class TranslationGuard
{
    public static SupportedLanguage SecondaryLanguage(
        SupportedLanguage language)
    {
        DomainGuard.DefinedEnum(
            language,
            nameof(language));

        if (language == SupportedLanguage.Azerbaijani)
        {
            throw new DomainException(
                "Azərbaycan dilində mətn əsas entity-də saxlanmalıdır.");
        }

        return language;
    }
}
