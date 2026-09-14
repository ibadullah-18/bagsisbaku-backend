namespace bagsisbaku.Contracts.Authentication;

public sealed record RegistrationResponse(
    Guid UserId,
    string Email,
    bool RequiresEmailConfirmation);

public sealed record AuthenticationResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAtUtc,
    AuthenticatedUserResponse User);

public sealed record AuthenticatedUserResponse(
    Guid Id,
    string FullName,
    string Email,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions);

public sealed record MessageResponse(
    string Message);
