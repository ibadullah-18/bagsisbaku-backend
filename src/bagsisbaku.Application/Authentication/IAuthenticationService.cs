using bagsisbaku.Application.Common.Results;

namespace bagsisbaku.Application.Authentication;

public interface IAuthenticationService
{
    Task<Result<RegistrationModel>> RegisterAsync(
        RegisterUserCommand command,
        CancellationToken cancellationToken = default);

    Task<Result> ConfirmEmailAsync(
        ConfirmEmailCommand command,
        CancellationToken cancellationToken = default);

    Task<Result> ResendEmailConfirmationAsync(
        ResendEmailConfirmationCommand command,
        CancellationToken cancellationToken = default);

    Task<Result<AuthenticationSessionModel>> LoginAsync(
        LoginUserCommand command,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<Result<AuthenticationSessionModel>>
        RefreshSessionAsync(
            string refreshToken,
            string? ipAddress,
            CancellationToken cancellationToken = default);

    Task<Result> LogoutAsync(
        string refreshToken,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<Result> ForgotPasswordAsync(
        ForgotPasswordCommand command,
        CancellationToken cancellationToken = default);

    Task<Result> ResetPasswordAsync(
        ResetPasswordCommand command,
        CancellationToken cancellationToken = default);
}
