namespace bagsisbaku.Application.Abstractions.Authentication;

public interface ICurrentUser
{
    Guid? UserId { get; }

    bool IsAuthenticated { get; }

    bool IsSuperAdmin { get; }

    IReadOnlySet<string> Permissions { get; }

    bool HasPermission(string permission);
}
