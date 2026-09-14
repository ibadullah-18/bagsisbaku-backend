using bagsisbaku.Application.Common.Results;
using bagsisbaku.Domain.Catalog;

namespace bagsisbaku.Application.Catalog.Products;

public static class ProductErrors
{
    public static readonly Error NotFound =
        Error.NotFound(
            "products.not-found",
            "Məhsul tapılmadı.");

    public static readonly Error ProductCodeConflict =
        Error.Conflict(
            "products.product-code-conflict",
            "Bu məhsul kodu artıq istifadə olunur.");

    public static readonly Error BrandNotFound =
        Error.Validation(
            "products.brand-not-found",
            "Seçilmiş brand tapılmadı və ya aktiv deyil.");

    public static readonly Error CategoryNotFound =
        Error.Validation(
            "products.category-not-found",
            "Seçilmiş kateqoriya tapılmadı və ya aktiv deyil.");

    public static readonly Error CategoryTypeMismatch =
        Error.Validation(
            "products.category-type-mismatch",
            "Kateqoriya seçilmiş məhsul tipinə aid deyil.");

    public static readonly Error SizeNotFound =
        Error.Validation(
            "products.size-not-found",
            "Seçilmiş ölçü tapılmadı və ya aktiv deyil.");

    public static readonly Error SizeTypeMismatch =
        Error.Validation(
            "products.size-type-mismatch",
            "Ölçü seçilmiş məhsul tipinə aid deyil.");

    public static readonly Error ColorNotFound =
        Error.Validation(
            "products.color-not-found",
            "Seçilmiş rəng tapılmadı və ya aktiv deyil.");

    public static Error DefaultConfigurationMissing(
        ProductType productType)
    {
        return Error.Validation(
            "products.default-configuration-missing",
            $"{productType} üçün default category və size seçilməyib.");
    }

    public static Error Invalid(string description)
    {
        return Error.Validation(
            "products.invalid",
            description);
    }
}
