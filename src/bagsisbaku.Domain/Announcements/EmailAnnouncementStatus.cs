namespace bagsisbaku.Domain.Announcements;

public enum EmailAnnouncementStatus
{
    Draft = 1,
    Queued = 2,
    Processing = 3,
    Completed = 4,
    PartiallyFailed = 5,
    Failed = 6
}