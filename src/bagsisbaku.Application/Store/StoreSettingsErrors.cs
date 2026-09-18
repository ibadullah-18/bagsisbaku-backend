using bagsisbaku.Application.Common.Results;

namespace bagsisbaku.Application.Store;

public static class StoreSettingsErrors
{
    public static readonly Error NotFound =
        Error.NotFound(
            "store-settings.not-found",
            "Mağaza məlumatları tapılmadı.");

    public static readonly Error InvalidRowVersion =
        Error.Validation(
            "store-settings.invalid-row-version",
            "Göndərilən məlumat versiyası düzgün deyil.");

    public static readonly Error ConcurrencyConflict =
        Error.Conflict(
            "store-settings.concurrency-conflict",
            "Mağaza məlumatları başqa istifadəçi tərəfindən dəyişdirilib. Məlumatları yeniləyib təkrar cəhd edin.");

    public static readonly Error AzerbaijaniTranslationNotAllowed =
        Error.Validation(
            "store-settings.az-translation-not-allowed",
            "Azərbaycan dili əsas mağaza məlumatında saxlanılır. Tərcümə hissəsində yalnız rus və ingilis dili seçilə bilər.");

    public static readonly Error UnsupportedLanguage =
        Error.Validation(
            "store-settings.unsupported-language",
            "Dəstəklənməyən dil seçilib.");
}