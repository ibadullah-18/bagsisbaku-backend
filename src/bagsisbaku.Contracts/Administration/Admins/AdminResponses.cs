namespace bagsisbaku.Contracts.Administration.Admins;

public sealed record AdminResponse(
    Guid Id,
    string FullName,
    string Email,
    bool IsActive,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? LastLoginAtUtc,
    IReadOnlyCollection<string> Permissions);

public sealed record PermissionDefinitionResponse(
    string Name,
    string Group,
    string DisplayName);
