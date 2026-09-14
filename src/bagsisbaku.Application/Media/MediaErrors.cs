using bagsisbaku.Application.Common.Results;

namespace bagsisbaku.Application.Media;

public static class MediaErrors
{
    public static readonly Error EmptyFile =
        Error.Validation(
            "media.empty-file",
            "Şəkil faylı boşdur.");

    public static readonly Error UnsupportedFormat =
        Error.Validation(
            "media.unsupported-format",
            "Yalnız JPG, JPEG, PNG, WEBP, HEIC və HEIF qəbul edilir.");

    public static readonly Error StandardFileTooLarge =
        Error.Validation(
            "media.file-too-large",
            "Şəkil maksimum 10 MB ola bilər.");

    public static readonly Error HeicFileTooLarge =
        Error.Validation(
            "media.heic-file-too-large",
            "HEIC və HEIF şəkli maksimum 25 MB ola bilər.");

    public static readonly Error InvalidStream =
        Error.Validation(
            "media.invalid-stream",
            "Şəkil faylı oxuna bilmir.");

    public static readonly Error InvalidSignature =
        Error.Validation(
            "media.invalid-signature",
            "Faylın həqiqi formatı uzantısı ilə uyğun deyil.");

    public static readonly Error BrandNotFound =
        Error.NotFound(
            "media.brand-not-found",
            "Brand tapılmadı.");

    public static readonly Error CategoryNotFound =
        Error.NotFound(
            "media.category-not-found",
            "Kateqoriya tapılmadı.");

    public static readonly Error ProductNotFound =
        Error.NotFound(
            "media.product-not-found",
            "Məhsul tapılmadı.");

    public static readonly Error StorageFailure =
        Error.Failure(
            "media.storage-failure",
            "Şəkil yaddaşa yüklənərkən xəta baş verdi.");

    public static Error Invalid(string description)
    {
        return Error.Validation(
            "media.invalid",
            description);
    }
}
