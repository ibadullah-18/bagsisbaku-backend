using System.ComponentModel.DataAnnotations;

namespace bagsisbaku.Contracts.Orders;

public sealed record CancelOrderRequest
{
    [Required]
    [MaxLength(500)]
    public string Reason { get; init; } =
        string.Empty;
}