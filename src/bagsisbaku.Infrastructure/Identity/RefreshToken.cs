namespace bagsisbaku.Infrastructure.Identity;

public sealed class RefreshToken
{
    private RefreshToken()
    {
    }

    private RefreshToken(
        Guid id,
        Guid userId,
        string tokenHash,
        DateTimeOffset createdAtUtc,
        DateTimeOffset expiresAtUtc,
        string? createdByIp)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "UserId boş ola bilməz.",
                nameof(userId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(
            tokenHash);

        if (expiresAtUtc <= createdAtUtc)
        {
            throw new ArgumentException(
                "Refresh token bitmə vaxtı yaradılma " +
                "vaxtından sonra olmalıdır.",
                nameof(expiresAtUtc));
        }

        Id = id;
        UserId = userId;
        TokenHash = tokenHash.Trim();
        CreatedAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
        CreatedByIp = NormalizeIp(createdByIp);
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string TokenHash { get; private set; } =
        string.Empty;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset ExpiresAtUtc { get; private set; }

    public string? CreatedByIp { get; private set; }

    public DateTimeOffset? RevokedAtUtc { get; private set; }

    public string? RevokedByIp { get; private set; }

    public string? ReplacedByTokenHash
    {
        get;
        private set;
    }

    public string? RevokeReason { get; private set; }

    public byte[] RowVersion { get; private set; } = [];

    public AppUser User { get; private set; } = null!;

    public static RefreshToken Create(
        Guid userId,
        string tokenHash,
        DateTimeOffset createdAtUtc,
        DateTimeOffset expiresAtUtc,
        string? createdByIp)
    {
        return new RefreshToken(
            Guid.NewGuid(),
            userId,
            tokenHash,
            createdAtUtc,
            expiresAtUtc,
            createdByIp);
    }

    public bool IsActive(
        DateTimeOffset utcNow)
    {
        return RevokedAtUtc is null &&
               ExpiresAtUtc > utcNow;
    }

    public void Revoke(
        DateTimeOffset revokedAtUtc,
        string? revokedByIp,
        string? replacedByTokenHash,
        string? reason)
    {
        if (RevokedAtUtc.HasValue)
        {
            return;
        }

        RevokedAtUtc = revokedAtUtc;
        RevokedByIp = NormalizeIp(revokedByIp);

        ReplacedByTokenHash =
            NormalizeOptional(
                replacedByTokenHash,
                maximumLength: 128,
                nameof(replacedByTokenHash));

        RevokeReason =
            NormalizeOptional(
                reason,
                maximumLength: 300,
                nameof(reason));
    }

    private static string? NormalizeIp(string? ipAddress)
    {
        return NormalizeOptional(
            ipAddress,
            maximumLength: 64,
            nameof(ipAddress));
    }

    private static string? NormalizeOptional(
        string? value,
        int maximumLength,
        string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalizedValue = value.Trim();

        if (normalizedValue.Length > maximumLength)
        {
            throw new ArgumentException(
                $"{parameterName} maksimum " +
                $"{maximumLength} simvol ola bilər.",
                parameterName);
        }

        return normalizedValue;
    }
}
