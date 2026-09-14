using System.ComponentModel.DataAnnotations;

namespace bagsisbaku.Contracts.Authentication;

public sealed record RegisterRequest
{
    [Required]
    [StringLength(
        160,
        MinimumLength = 2)]
    public string FullName { get; init; } =
        string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; init; } =
        string.Empty;

    [Required]
    [StringLength(
        128,
        MinimumLength = 8)]
    public string Password { get; init; } =
        string.Empty;
}

public sealed record LoginRequest
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; init; } =
        string.Empty;

    [Required]
    [StringLength(
        128,
        MinimumLength = 1)]
    public string Password { get; init; } =
        string.Empty;
}

public sealed record ConfirmEmailRequest
{
    public Guid UserId { get; init; }

    [Required]
    [StringLength(
        4096,
        MinimumLength = 1)]
    public string Token { get; init; } =
        string.Empty;
}

public sealed record ResendEmailConfirmationRequest
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; init; } =
        string.Empty;
}

public sealed record ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; init; } =
        string.Empty;
}

public sealed record ResetPasswordRequest
{
    public Guid UserId { get; init; }

    [Required]
    [StringLength(
        4096,
        MinimumLength = 1)]
    public string Token { get; init; } =
        string.Empty;

    [Required]
    [StringLength(
        128,
        MinimumLength = 8)]
    public string NewPassword { get; init; } =
        string.Empty;
}
