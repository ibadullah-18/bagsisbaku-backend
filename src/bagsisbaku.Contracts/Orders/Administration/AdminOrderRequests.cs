using System.ComponentModel.DataAnnotations;

namespace bagsisbaku.Contracts.Orders.Administration;

public sealed class ChangeOrderStatusRequest
{
    [Range(1, 5)]
    public int Status { get; init; }

    [MaxLength(500)]
    public string? Note { get; init; }
}