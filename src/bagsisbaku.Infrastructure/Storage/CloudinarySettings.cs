namespace bagsisbaku.Infrastructure.Storage;

public sealed record CloudinarySettings(
    string? CloudName,
    string? ApiKey,
    string? ApiSecret)
{
    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(CloudName) &&
        !string.IsNullOrWhiteSpace(ApiKey) &&
        !string.IsNullOrWhiteSpace(ApiSecret);
}
