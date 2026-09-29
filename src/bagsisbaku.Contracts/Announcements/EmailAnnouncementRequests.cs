using System.ComponentModel.DataAnnotations;

namespace bagsisbaku.Contracts.Announcements;

public sealed record QueueEmailAnnouncementRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Subject
    {
        get;
        init;
    } = string.Empty;

    [Required]
    [StringLength(20_000, MinimumLength = 1)]
    public string HtmlBody
    {
        get;
        init;
    } = string.Empty;

    [Required]
    [StringLength(10_000, MinimumLength = 1)]
    public string TextBody
    {
        get;
        init;
    } = string.Empty;

    public bool SendToAllCustomers
    {
        get;
        init;
    }

    public IReadOnlyCollection<Guid> CustomerIds
    {
        get;
        init;
    } = Array.Empty<Guid>();

    public IReadOnlyCollection<string> AdditionalEmails
    {
        get;
        init;
    } = Array.Empty<string>();
}