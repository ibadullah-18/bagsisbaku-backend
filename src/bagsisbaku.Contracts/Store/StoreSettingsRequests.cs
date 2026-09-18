using System.ComponentModel.DataAnnotations;

namespace bagsisbaku.Contracts.Store;

public sealed record UpdateStoreSettingsRequest
{
    [Required]
    [MaxLength(32)]
    public required string PrimaryPhone { get; init; }

    [Required]
    [MaxLength(32)]
    public required string WhatsAppPhone { get; init; }

    [EmailAddress]
    [MaxLength(256)]
    public string? Email { get; init; }

    [MaxLength(2048)]
    public string? InstagramUrl { get; init; }

    [MaxLength(2048)]
    public string? TikTokUrl { get; init; }

    [Required]
    [MaxLength(500)]
    public required string Address { get; init; }

    [Required]
    [MaxLength(250)]
    public required string WorkingHours { get; init; }

    [Required]
    [MaxLength(2000)]
    public required string DeliveryInformation
    {
        get;
        init;
    }

    [Required]
    [MaxLength(2000)]
    public required string ReturnPolicy { get; init; }

    [Required]
    [MaxLength(4000)]
    public required string AboutText { get; init; }

    [Range(-90, 90)]
    public decimal Latitude { get; init; }

    [Range(-180, 180)]
    public decimal Longitude { get; init; }

    [MaxLength(2048)]
    public string? MapUrl { get; init; }

    [Required]
    public required string RowVersion { get; init; }
}

public sealed record UpsertStoreSettingsTranslationRequest
{
    [Required]
    public required string Language { get; init; }

    [Required]
    [MaxLength(500)]
    public required string Address { get; init; }

    [Required]
    [MaxLength(250)]
    public required string WorkingHours { get; init; }

    [Required]
    [MaxLength(2000)]
    public required string DeliveryInformation
    {
        get;
        init;
    }

    [Required]
    [MaxLength(2000)]
    public required string ReturnPolicy { get; init; }

    [Required]
    [MaxLength(4000)]
    public required string AboutText { get; init; }

    [Required]
    public required string RowVersion { get; init; }
}