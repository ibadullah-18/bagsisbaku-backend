using bagsisbaku.Api.Extensions;
using bagsisbaku.Application.Authentication;
using bagsisbaku.Contracts.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bagsisbaku.Api.Controllers.Authentication;

[ApiController]
[AllowAnonymous]
[ResponseCache(
    NoStore = true,
    Location = ResponseCacheLocation.None)]
[Route("api/auth")]
public sealed class AuthenticationController(
    IAuthenticationService authenticationService)
    : ControllerBase
{
    private const string RefreshTokenCookieName =
        "bagsisbaku.refresh_token";

    [HttpPost("register")]
    public async Task<ActionResult<RegistrationResponse>>
        RegisterAsync(
            RegisterRequest request,
            CancellationToken cancellationToken)
    {
        var result =
            await authenticationService.RegisterAsync(
                new RegisterUserCommand(
                    request.FullName,
                    request.Email,
                    request.Password),
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        return StatusCode(
            StatusCodes.Status201Created,
            new RegistrationResponse(
                result.Value.UserId,
                result.Value.Email,
                result.Value
                    .RequiresEmailConfirmation));
    }

    [HttpPost("confirm-email")]
    public async Task<ActionResult<MessageResponse>>
        ConfirmEmailAsync(
            ConfirmEmailRequest request,
            CancellationToken cancellationToken)
    {
        var result =
            await authenticationService.ConfirmEmailAsync(
                new ConfirmEmailCommand(
                    request.UserId,
                    request.Token),
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        return Ok(
            new MessageResponse(
                "Email uğurla təsdiqləndi."));
    }

    [HttpPost("resend-email-confirmation")]
    public async Task<ActionResult<MessageResponse>>
        ResendEmailConfirmationAsync(
            ResendEmailConfirmationRequest request,
            CancellationToken cancellationToken)
    {
        var result =
            await authenticationService
                .ResendEmailConfirmationAsync(
                    new ResendEmailConfirmationCommand(
                        request.Email),
                    cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        return Ok(
            new MessageResponse(
                "Email mövcuddursa, təsdiq məktubu göndərildi."));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthenticationResponse>>
        LoginAsync(
            LoginRequest request,
            CancellationToken cancellationToken)
    {
        var result =
            await authenticationService.LoginAsync(
                new LoginUserCommand(
                    request.Email,
                    request.Password),
                GetClientIpAddress(),
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        AppendRefreshTokenCookie(
            result.Value.RefreshToken,
            result.Value.RefreshTokenExpiresAtUtc);

        return Ok(
            ToResponse(result.Value));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthenticationResponse>>
        RefreshAsync(
            CancellationToken cancellationToken)
    {
        Request.Cookies.TryGetValue(
            RefreshTokenCookieName,
            out var refreshToken);

        var result =
            await authenticationService
                .RefreshSessionAsync(
                    refreshToken ?? string.Empty,
                    GetClientIpAddress(),
                    cancellationToken);

        if (result.IsFailure)
        {
            DeleteRefreshTokenCookie();

            return this.ToProblemResult(
                result.Error);
        }

        AppendRefreshTokenCookie(
            result.Value.RefreshToken,
            result.Value.RefreshTokenExpiresAtUtc);

        return Ok(
            ToResponse(result.Value));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> LogoutAsync(
        CancellationToken cancellationToken)
    {
        Request.Cookies.TryGetValue(
            RefreshTokenCookieName,
            out var refreshToken);

        var result =
            await authenticationService.LogoutAsync(
                refreshToken ?? string.Empty,
                GetClientIpAddress(),
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        DeleteRefreshTokenCookie();

        return NoContent();
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<MessageResponse>>
        ForgotPasswordAsync(
            ForgotPasswordRequest request,
            CancellationToken cancellationToken)
    {
        var result =
            await authenticationService
                .ForgotPasswordAsync(
                    new ForgotPasswordCommand(
                        request.Email),
                    cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        return Ok(
            new MessageResponse(
                "Email mövcuddursa, şifrə yeniləmə məktubu göndərildi."));
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<MessageResponse>>
        ResetPasswordAsync(
            ResetPasswordRequest request,
            CancellationToken cancellationToken)
    {
        var result =
            await authenticationService.ResetPasswordAsync(
                new ResetPasswordCommand(
                    request.UserId,
                    request.Token,
                    request.NewPassword),
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        DeleteRefreshTokenCookie();

        return Ok(
            new MessageResponse(
                "Şifrə uğurla yeniləndi."));
    }

    private void AppendRefreshTokenCookie(
        string refreshToken,
        DateTimeOffset expiresAtUtc)
    {
        Response.Cookies.Append(
            RefreshTokenCookieName,
            refreshToken,
            CreateCookieOptions(expiresAtUtc));
    }

    private void DeleteRefreshTokenCookie()
    {
        Response.Cookies.Delete(
            RefreshTokenCookieName,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                IsEssential = true,
                Path = "/api/auth"
            });
    }

    private CookieOptions CreateCookieOptions(
        DateTimeOffset expiresAtUtc)
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            IsEssential = true,
            Path = "/api/auth",
            Expires = expiresAtUtc
        };
    }

    private string? GetClientIpAddress()
    {
        return HttpContext
            .Connection
            .RemoteIpAddress?
            .ToString();
    }

    private static AuthenticationResponse ToResponse(
        AuthenticationSessionModel session)
    {
        return new AuthenticationResponse(
            session.AccessToken,
            session.AccessTokenExpiresAtUtc,
            new AuthenticatedUserResponse(
                session.User.Id,
                session.User.FullName,
                session.User.Email,
                session.User.Roles,
                session.User.Permissions));
    }
}
