using Microsoft.AspNetCore.Identity;

namespace bagsisbaku.Infrastructure.Identity;

public sealed class AppRole : IdentityRole<Guid>
{
    private AppRole()
    {
    }

    private AppRole(
        Guid id,
        string name,
        string? description)
    {
        Id = id;

        Name = ValidateName(name);
        NormalizedName = Name.ToUpperInvariant();

        Description = NormalizeDescription(
            description);

        ConcurrencyStamp = Guid.NewGuid().ToString("N");
    }

    public string? Description { get; private set; }

    public static AppRole Create(
        string name,
        string? description = null)
    {
        return new AppRole(
            Guid.NewGuid(),
            name,
            description);
    }

    public void UpdateDescription(
        string? description)
    {
        Description = NormalizeDescription(
            description);
    }

    private static string ValidateName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            name);

        var validName = name.Trim();

        if (validName.Length > 64)
        {
            throw new ArgumentException(
                "Rol adı 64 simvoldan çox ola bilməz.",
                nameof(name));
        }

        return validName;
    }

    private static string? NormalizeDescription(
        string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return null;
        }

        var validDescription = description.Trim();

        if (validDescription.Length > 300)
        {
            throw new ArgumentException(
                "Rol açıqlaması 300 simvoldan çox ola bilməz.",
                nameof(description));
        }

        return validDescription;
    }
}
