using bagsisbaku.Domain.Common;

namespace bagsisbaku.Domain.Catalog;

public sealed class Color : AuditableEntity
{
    private Color()
    {
    }

    private Color(
        Guid id,
        string name,
        string? hexCode)
        : base(id)
    {
        Name = DomainGuard.Required(
            name,
            nameof(Name),
            80);

        HexCode = DomainGuard.HexColor(hexCode);
        IsActive = true;
    }

    public string Name { get; private set; } = string.Empty;

    public string? HexCode { get; private set; }

    public bool IsActive { get; private set; }

    public static Color Create(
        string name,
        string? hexCode)
    {
        return new Color(
            Guid.NewGuid(),
            name,
            hexCode);
    }

    public void Update(
        string name,
        string? hexCode)
    {
        Name = DomainGuard.Required(
            name,
            nameof(Name),
            80);

        HexCode = DomainGuard.HexColor(hexCode);
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
