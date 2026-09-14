namespace bagsisbaku.Application.Common.Pagination;

public sealed record PageRequest
{
    public const int DefaultPageNumber = 1;
    public const int DefaultPageSize = 20;
    public const int MaximumPageSize = 100;

    public PageRequest(
        int pageNumber = DefaultPageNumber,
        int pageSize = DefaultPageSize)
    {
        if (pageNumber < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageNumber),
                "Səhifə nömrəsi minimum 1 olmalıdır.");
        }

        if (pageSize < 1 || pageSize > MaximumPageSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageSize),
                $"Səhifə ölçüsü 1-{MaximumPageSize} arasında olmalıdır.");
        }

        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    public int PageNumber { get; }

    public int PageSize { get; }

    public int Skip => (PageNumber - 1) * PageSize;
}
