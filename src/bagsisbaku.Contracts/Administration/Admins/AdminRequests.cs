using System.ComponentModel.DataAnnotations;

namespace bagsisbaku.Contracts.Administration.Admins;

public sealed record CreateAdminRequest
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

    public IReadOnlyCollection<string> Permissions
    {
        get;
        init;
    } = [];
}

public sealed record UpdateAdminPermissionsRequest
{
    public IReadOnlyCollection<string> Permissions
    {
        get;
        init;
    } = [];
}

public sealed record SetAdminStatusRequest
{
    public bool IsActive { get; init; }
}
