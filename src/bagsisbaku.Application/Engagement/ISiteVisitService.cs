using bagsisbaku.Application.Common.Results;

namespace bagsisbaku.Application.Engagement;

public sealed record SiteVisitModel(
    Guid Id,
    Guid VisitorId,
    string PagePath);

public interface ISiteVisitService
{
    Task<Result<SiteVisitModel>> RecordAsync(
        Guid visitorId,
        string? pagePath,
        CancellationToken cancellationToken = default);
}