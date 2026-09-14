namespace bagsisbaku.Application.Authentication;

public sealed record RegisterUserCommand(
    string FullName,
    string Email,
    string Password);

public sealed record LoginUserCommand(
    string Email,
    string Password);

public sealed record ConfirmEmailCommand(
    Guid UserId,
    string Token);

public sealed record ResendEmailConfirmationCommand(
    string Email);

public sealed record ForgotPasswordCommand(
    string Email);

public sealed record ResetPasswordCommand(
    Guid UserId,
    string Token,
    string NewPassword);

public sealed record RegistrationModel(
    Guid UserId,
    string Email,
    bool RequiresEmailConfirmation);

public sealed record AuthenticatedUserModel(
    Guid Id,
    string FullName,
    string Email,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions);

public sealed record AuthenticationSessionModel(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAtUtc,
    AuthenticatedUserModel User);
