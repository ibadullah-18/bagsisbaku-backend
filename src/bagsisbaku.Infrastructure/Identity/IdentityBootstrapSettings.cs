namespace bagsisbaku.Infrastructure.Identity;

public sealed record IdentityBootstrapSettings(
    bool Enabled,
    string? SuperAdminFullName,
    string? SuperAdminEmail,
    string? SuperAdminPassword)
{
    public void Validate()
    {
        if (!Enabled)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(SuperAdminFullName))
        {
            throw new InvalidOperationException(
                "Bootstrap:Identity:SuperAdmin:FullName tapılmadı.");
        }

        if (string.IsNullOrWhiteSpace(SuperAdminEmail))
        {
            throw new InvalidOperationException(
                "Bootstrap:Identity:SuperAdmin:Email tapılmadı.");
        }

        if (string.IsNullOrWhiteSpace(SuperAdminPassword))
        {
            throw new InvalidOperationException(
                "Bootstrap:Identity:SuperAdmin:Password tapılmadı.");
        }

        if (SuperAdminPassword.Length < 8)
        {
            throw new InvalidOperationException(
                "Superadmin şifrəsi minimum 8 simvol olmalıdır.");
        }
    }
}
