using bagsisbaku.Application.Common.Results;

namespace bagsisbaku.Application.Catalog.Administration;

public static class CatalogErrors
{
    public static readonly Error InvalidProductType =
        Error.Validation(
            "catalog.invalid-product-type",
            "Məhsul tipi yalnız Ayaqqabı və ya Çanta ola bilər.");

    public static readonly Error BrandNameConflict =
        Error.Conflict(
            "catalog.brand-name-conflict",
            "Bu adda brand artıq mövcuddur.");

    public static readonly Error CategoryNameConflict =
        Error.Conflict(
            "catalog.category-name-conflict",
            "Bu məhsul tipi üçün eyni adda kateqoriya mövcuddur.");

    public static readonly Error SizeValueConflict =
        Error.Conflict(
            "catalog.size-value-conflict",
            "Bu məhsul tipi üçün eyni ölçü mövcuddur.");

    public static readonly Error ColorNameConflict =
        Error.Conflict(
            "catalog.color-name-conflict",
            "Bu adda rəng artıq mövcuddur.");

    public static readonly Error CategoryNotFound =
        Error.NotFound(
            "catalog.category-not-found",
            "Kateqoriya tapılmadı və ya aktiv deyil.");

    public static readonly Error SizeNotFound =
        Error.NotFound(
            "catalog.size-not-found",
            "Ölçü tapılmadı və ya aktiv deyil.");

    public static readonly Error CategoryTypeMismatch =
        Error.Validation(
            "catalog.category-type-mismatch",
            "Kateqoriya seçilmiş məhsul tipinə aid deyil.");

    public static readonly Error SizeTypeMismatch =
        Error.Validation(
            "catalog.size-type-mismatch",
            "Ölçü seçilmiş məhsul tipinə aid deyil.");

    public static Error Invalid(string description)
    {
        return Error.Validation(
            "catalog.invalid",
            description);
    }
}
