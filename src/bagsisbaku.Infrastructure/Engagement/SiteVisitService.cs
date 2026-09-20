using bagsisbaku.Application.Common.Results;
using bagsisbaku.Application.Engagement;
using bagsisbaku.Domain.Engagement;
using bagsisbaku.Infrastructure.Persistence;

namespace bagsisbaku.Infrastructure.Engagement;

public sealed class SiteVisitService(
    ApplicationDbContext dbContext)
    : ISiteVisitService
{
    public async Task<Result<SiteVisitModel>> RecordAsync(
        Guid visitorId,
        string? pagePath,
        CancellationToken cancellationToken = default)
    {
        if (visitorId == Guid.Empty)
        {
            return Result.Failure<SiteVisitModel>(
                Error.Validation(
                    "visits.invalid-visitor-id",
                    "visitorId düzgün deyil."));
        }

        var normalizedPath =
            pagePath?.Trim();

        if (string.IsNullOrEmpty(normalizedPath) ||
            normalizedPath.Length > 256 ||
            !normalizedPath.StartsWith('/') ||
            normalizedPath.StartsWith(
                "//",
                StringComparison.Ordinal) ||
            normalizedPath.Contains('?') ||
            normalizedPath.Contains('#'))
        {
            return Result.Failure<SiteVisitModel>(
                Error.Validation(
                    "visits.invalid-page-path",
                    "pagePath / ilə başlayan, maksimum 256 simvolluq səhifə yolu olmalıdır."));
        }

        var visit =
            SiteVisit.Create(
                visitorId,
                normalizedPath);

        dbContext.SiteVisits.Add(visit);

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return Result.Success(
            new SiteVisitModel(
                visit.Id,
                visit.VisitorId,
                visit.PagePath));
    }
}