namespace bagsisbaku.Infrastructure.Authentication;

public sealed record AuthenticationUrlSettings(
    Uri FrontendBaseUri)
{
    public void Validate()
    {
        ArgumentNullException.ThrowIfNull(
            FrontendBaseUri);

        if (!FrontendBaseUri.IsAbsoluteUri)
        {
            throw new InvalidOperationException(
                "Frontend:BaseUrl absolute URL olmalıdır.");
        }

        var isHttp =
            string.Equals(
                FrontendBaseUri.Scheme,
                Uri.UriSchemeHttp,
                StringComparison.OrdinalIgnoreCase);

        var isHttps =
            string.Equals(
                FrontendBaseUri.Scheme,
                Uri.UriSchemeHttps,
                StringComparison.OrdinalIgnoreCase);

        if (!isHttp && !isHttps)
        {
            throw new InvalidOperationException(
                "Frontend:BaseUrl yalnız HTTP və ya HTTPS ola bilər.");
        }
    }
}
