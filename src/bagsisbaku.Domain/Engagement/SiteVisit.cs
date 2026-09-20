using bagsisbaku.Domain.Common;

namespace bagsisbaku.Domain.Engagement;

public sealed class SiteVisit : AuditableEntity
{
    private SiteVisit()
    {
    }

    private SiteVisit(
        Guid id,
        Guid visitorId,
        string pagePath)
        : base(id)
    {
        VisitorId =
            DomainGuard.NotEmpty(
                visitorId,
                nameof(VisitorId));

        var normalizedPath =
            DomainGuard.Required(
                pagePath,
                nameof(PagePath),
                256);

        if (!normalizedPath.StartsWith('/') ||
            normalizedPath.StartsWith(
                "//",
                StringComparison.Ordinal) ||
            normalizedPath.Contains('?') ||
            normalizedPath.Contains('#'))
        {
            throw new DomainException(
                "Səhifə yolu / ilə başlamalı, query və fragment içerməməlidir.");
        }

        PagePath = normalizedPath;
    }

    public Guid VisitorId { get; private set; }

    public string PagePath { get; private set; } =
        string.Empty;

    public static SiteVisit Create(
        Guid visitorId,
        string pagePath)
    {
        return new SiteVisit(
            Guid.NewGuid(),
            visitorId,
            pagePath);
    }
}