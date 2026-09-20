using bagsisbaku.Domain.Common;

namespace bagsisbaku.Domain.Home;

public sealed class HomeSection : AuditableEntity
{
    private HomeSection()
    {
    }

    private HomeSection(
        Guid id,
        HomeSectionType type,
        string title,
        string? subtitle,
        string? targetUrl,
        int sortOrder)
        : base(id)
    {
        Type = DomainGuard.DefinedEnum(type, nameof(Type));
        UpdateContent(title, subtitle, targetUrl);
        SetSortOrder(sortOrder);
        IsActive = false;
    }

    public HomeSectionType Type { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string? Subtitle { get; private set; }

    public string? TargetUrl { get; private set; }

    public string? ImageUrl { get; private set; }

    public string? ImagePublicId { get; private set; }

    public int SortOrder { get; private set; }

    public bool IsActive { get; private set; }

    public static HomeSection Create(
        HomeSectionType type,
        string title,
        string? subtitle,
        string? targetUrl,
        int sortOrder)
    {
        return new HomeSection(
            Guid.NewGuid(),
            type,
            title,
            subtitle,
            targetUrl,
            sortOrder);
    }

    public void UpdateContent(
        string title,
        string? subtitle,
        string? targetUrl)
    {
        Title = DomainGuard.Required(
            title,
            nameof(Title),
            160);

        Subtitle = DomainGuard.Optional(
            subtitle,
            nameof(Subtitle),
            500);

        TargetUrl = DomainGuard.Optional(
            targetUrl,
            nameof(TargetUrl),
            2048);
    }

    public void SetSortOrder(int sortOrder)
    {
        if (sortOrder < 0)
        {
            throw new DomainException(
                "Sıralama nömrəsi mənfi ola bilməz.");
        }

        SortOrder = sortOrder;
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
        IsActive = false;
    }

    public void Activate()
    {
        if (ImageUrl is null)
        {
            throw new DomainException(
                "Şəkli olmayan ana səhifə bloku aktiv edilə bilməz.");
        }

        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
