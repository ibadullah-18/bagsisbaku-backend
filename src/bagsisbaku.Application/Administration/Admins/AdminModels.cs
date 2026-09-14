namespace bagsisbaku.Application.Administration.Admins;

public sealed record CreateAdminCommand(
    string FullName,
    string Email,
    string Password,
    IReadOnlyCollection<string> Permissions);

public sealed record UpdateAdminPermissionsCommand(
    Guid AdminId,
    IReadOnlyCollection<string> Permissions);

public sealed record SetAdminStatusCommand(
    Guid AdminId,
    bool IsActive);

public sealed record AdminModel(
    Guid Id,
    string FullName,
    string Email,
    bool IsActive,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? LastLoginAtUtc,
    IReadOnlyCollection<string> Permissions);
