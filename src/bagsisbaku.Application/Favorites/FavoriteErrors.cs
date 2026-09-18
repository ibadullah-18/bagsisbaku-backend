using bagsisbaku.Application.Common.Results;

namespace bagsisbaku.Application.Favorites;

public static class FavoriteErrors
{
    public static readonly Error AuthenticationRequired =
        Error.Unauthorized(
            "favorites.authentication-required",
            "Favoritlərdən istifadə etmək üçün hesaba daxil olmaq lazımdır.");

    public static readonly Error CustomerNotFound =
        Error.NotFound(
            "favorites.customer-not-found",
            "İstifadəçi hesabı tapılmadı.");

    public static readonly Error CustomerInactive =
        Error.Forbidden(
            "favorites.customer-inactive",
            "İstifadəçi hesabı deaktiv edilib.");

    public static readonly Error ProductNotFound =
        Error.NotFound(
            "favorites.product-not-found",
            "Məhsul tapılmadı.");

    public static readonly Error ProductUnavailable =
        Error.Conflict(
            "favorites.product-unavailable",
            "Məhsul hazırda aktiv deyil.");

    public static readonly Error FavoriteNotFound =
        Error.NotFound(
            "favorites.favorite-not-found",
            "Məhsul favoritlərdə tapılmadı.");

    public static readonly Error UnexpectedFailure =
        Error.Failure(
            "favorites.unexpected-failure",
            "Favorit əməliyyatı zamanı gözlənilməyən xəta baş verdi.");
}