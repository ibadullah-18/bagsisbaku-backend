using System.ComponentModel.DataAnnotations;

namespace bagsisbaku.Contracts.Customers.Administration;

public sealed class AdminCustomerFilterRequest
{
    [StringLength(200)]
    public string? Search { get; init; }

    public bool? IsActive { get; init; }

    public bool? EmailConfirmed { get; init; }

    public DateTimeOffset? CreatedFromUtc { get; init; }

    public DateTimeOffset? CreatedToUtc { get; init; }

    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, 100)]
    public int PageSize { get; init; } = 20;
}

public sealed record SetCustomerStatusRequest(
    bool IsActive);