using System.Net;
using bagsisbaku.Application.Abstractions.Email;
using bagsisbaku.Application.Authentication;

namespace bagsisbaku.Infrastructure.Authentication;

internal sealed class AuthenticationEmailFactory
    : IAuthenticationEmailFactory
{
    private readonly AuthenticationUrlSettings _settings;

    public AuthenticationEmailFactory(
        AuthenticationUrlSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        settings.Validate();
        _settings = settings;
    }

    public EmailMessage CreateEmailConfirmation(
        string fullName,
        string email,
        Guid userId,
        string urlSafeToken)
    {
        ValidateArguments(
            fullName,
            email,
            userId,
            urlSafeToken);

        var confirmationUrl =
            BuildUrl(
                "email-confirmation",
                userId,
                urlSafeToken);

        var safeFullName =
            WebUtility.HtmlEncode(fullName);

        var safeUrl =
            WebUtility.HtmlEncode(
                confirmationUrl);

        var htmlBody = $"""
            <!doctype html>
            <html lang="az">
            <head>
                <meta charset="utf-8">
                <title>Email təsdiqi</title>
            </head>
            <body>
                <h2>bagsisbaku</h2>
                <p>Salam, {safeFullName}.</p>
                <p>
                    bagsisbaku hesabını aktivləşdirmək üçün
                    aşağıdakı keçiddən istifadə et.
                </p>
                <p>
                    <a href="{safeUrl}">
                        Email ünvanını təsdiqlə
                    </a>
                </p>
                <p>
                    Bu keçid 20 dəqiqə ərzində etibarlıdır.
                </p>
                <p>
                    Əgər bu hesabı sən yaratmamısansa,
                    məktubu nəzərə alma.
                </p>
            </body>
            </html>
            """;

        var textBody = $"""
            bagsisbaku

            Salam, {fullName}.

            Hesabını aktivləşdirmək üçün bu keçidi aç:

            {confirmationUrl}

            Bu keçid 20 dəqiqə ərzində etibarlıdır.

            Əgər bu hesabı sən yaratmamısansa,
            məktubu nəzərə alma.
            """;

        return new EmailMessage(
            email,
            "bagsisbaku — email təsdiqi",
            htmlBody,
            textBody);
    }

    public EmailMessage CreatePasswordReset(
        string fullName,
        string email,
        Guid userId,
        string urlSafeToken)
    {
        ValidateArguments(
            fullName,
            email,
            userId,
            urlSafeToken);

        var resetUrl =
            BuildUrl(
                "reset-password",
                userId,
                urlSafeToken);

        var safeFullName =
            WebUtility.HtmlEncode(fullName);

        var safeUrl =
            WebUtility.HtmlEncode(resetUrl);

        var htmlBody = $"""
            <!doctype html>
            <html lang="az">
            <head>
                <meta charset="utf-8">
                <title>Şifrə yeniləmə</title>
            </head>
            <body>
                <h2>bagsisbaku</h2>
                <p>Salam, {safeFullName}.</p>
                <p>
                    Şifrəni yeniləmək üçün aşağıdakı
                    keçiddən istifadə et.
                </p>
                <p>
                    <a href="{safeUrl}">
                        Şifrəni yenilə
                    </a>
                </p>
                <p>
                    Bu keçid 20 dəqiqə ərzində etibarlıdır.
                </p>
                <p>
                    Bu sorğunu sən göndərməmisənsə,
                    məktubu nəzərə alma.
                </p>
            </body>
            </html>
            """;

        var textBody = $"""
            bagsisbaku

            Salam, {fullName}.

            Şifrəni yeniləmək üçün bu keçidi aç:

            {resetUrl}

            Bu keçid 20 dəqiqə ərzində etibarlıdır.

            Bu sorğunu sən göndərməmisənsə,
            məktubu nəzərə alma.
            """;

        return new EmailMessage(
            email,
            "bagsisbaku — şifrə yeniləmə",
            htmlBody,
            textBody);
    }

    private string BuildUrl(
        string path,
        Guid userId,
        string token)
    {
        var baseUrl =
            _settings
                .FrontendBaseUri
                .AbsoluteUri
                .TrimEnd('/');

        var encodedUserId =
            Uri.EscapeDataString(
                userId.ToString("D"));

        var encodedToken =
            Uri.EscapeDataString(token);

        return
            $"{baseUrl}/{path}" +
            $"?userId={encodedUserId}" +
            $"&token={encodedToken}";
    }

    private static void ValidateArguments(
        string fullName,
        string email,
        Guid userId,
        string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            fullName);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            email);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            token);

        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "UserId boş ola bilməz.",
                nameof(userId));
        }
    }
}
