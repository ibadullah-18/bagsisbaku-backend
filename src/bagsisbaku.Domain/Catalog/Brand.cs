using bagsisbaku.Domain.Common;

namespace bagsisbaku.Domain.Catalog;

public sealed class Brand : AuditableEntity
{
    private Brand()
    {
    }

    private Brand(Guid id, string name)
        : base(id)
    {
        Name = DomainGuard.Required(
            name,
            nameof(Name),
            120);

        IsActive = true;
    }

    public string Name { get; private set; } = string.Empty;

    public string? ImageUrl { get; private set; }

    public string? ImagePublicId { get; private set; }

    public bool IsActive { get; private set; }

    public static Brand Create(string name)
    {
        return new Brand(Guid.NewGuid(), name);
    }

    public void UpdateName(string name)
    {
        Name = DomainGuard.Required(
            name,
            nameof(Name),
            120);
    }

    public void SetImage(
        string imageUrl,
        string imagePublicId)
    {
        ImageUrl = DomainGuard.Required(
            imageUrl,
            nameof(ImageUrl),
            2048);

        ImagePublicId = DomainGuard.Required(
            imagePublicId,
            nameof(ImagePublicId),
            255);
    }

    public void RemoveImage()
    {
        ImageUrl = null;
        ImagePublicId = null;
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
