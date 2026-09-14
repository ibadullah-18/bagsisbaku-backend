using Microsoft.AspNetCore.Identity;

namespace bagsisbaku.Infrastructure.Identity;

public sealed class AppUser : IdentityUser<Guid>
{
    private readonly List<RefreshToken> _refreshTokens = [];

    private AppUser()
    {
    }

    private AppUser(
        Guid id,
        string fullName,
        string email,
        DateTimeOffset createdAtUtc)
    {
        Id = id;

        SetFullName(fullName);

        var validEmail = ValidateEmail(email);

        Email = validEmail;
        UserName = validEmail;

        IsActive = true;
        CreatedAtUtc = createdAtUtc;

        SecurityStamp = Guid.NewGuid().ToString("N");
        ConcurrencyStamp = Guid.NewGuid().ToString("N");
    }

    public string FullName { get; private set; } =
        string.Empty;

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    public DateTimeOffset? LastLoginAtUtc { get; private set; }

    public IReadOnlyCollection<RefreshToken> RefreshTokens =>
        _refreshTokens;

    public static AppUser Create(
        string fullName,
        string email,
        DateTimeOffset createdAtUtc)
    {
        return new AppUser(
            Guid.NewGuid(),
            fullName,
            email,
            createdAtUtc);
    }

    public void SetFullName(string fullName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            fullName);

        var normalizedFullName = fullName.Trim();

        if (normalizedFullName.Length > 160)
        {
            throw new ArgumentException(
                "Ad və soyad 160 simvoldan çox ola bilməz.",
                nameof(fullName));
        }

        FullName = normalizedFullName;
    }

    public void UpdateProfile(
        string fullName,
        string? phoneNumber,
        DateTimeOffset updatedAtUtc)
    {
        SetFullName(fullName);

        PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber)
            ? null
            : phoneNumber.Trim();

        UpdatedAtUtc = updatedAtUtc;
    }

    public void RecordLogin(
        DateTimeOffset loggedInAtUtc)
    {
        LastLoginAtUtc = loggedInAtUtc;
    }

    public void Activate(
        DateTimeOffset updatedAtUtc)
    {
        IsActive = true;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void Deactivate(
        DateTimeOffset updatedAtUtc)
    {
        IsActive = false;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void AddRefreshToken(
        RefreshToken refreshToken)
    {
        ArgumentNullException.ThrowIfNull(
            refreshToken);

        if (refreshToken.UserId != Id)
        {
            throw new InvalidOperationException(
                "Refresh token başqa istifadəçiyə aiddir.");
        }

        _refreshTokens.Add(refreshToken);
    }

    private static string ValidateEmail(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            email);

        var validEmail = email.Trim();

        if (validEmail.Length > 256)
        {
            throw new ArgumentException(
                "Email 256 simvoldan çox ola bilməz.",
                nameof(email));
        }

        return validEmail;
    }
}
