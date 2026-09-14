namespace bagsisbaku.Application.Abstractions.Authentication;

public interface IAccessTokenGenerator
{
    GeneratedAccessToken Generate(
        AccessTokenUser user);
}

public sealed record AccessTokenUser(
    Guid UserId,
    string Email,
    string FullName,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions);

public sealed record GeneratedAccessToken(
    string Token,
    string TokenId,
    DateTimeOffset ExpiresAtUtc);
