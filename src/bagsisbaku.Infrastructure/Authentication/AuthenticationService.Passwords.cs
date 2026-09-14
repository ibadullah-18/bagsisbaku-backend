using bagsisbaku.Application.Authentication;
using bagsisbaku.Application.Common.Results;
using Microsoft.EntityFrameworkCore;

namespace bagsisbaku.Infrastructure.Authentication;

internal sealed partial class AuthenticationService
{
    public async Task<Result> ForgotPasswordAsync(
        ForgotPasswordCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        cancellationToken.ThrowIfCancellationRequested();

        var user =
            await _userManager.FindByEmailAsync(
                command.Email.Trim());

        // İstifadəçinin mövcud olub-olmadığını açıqlamırıq.
        if (
            user is null ||
            !user.IsActive ||
            !user.EmailConfirmed
        )
        {
            return Result.Success();
        }

        var identityToken =
            await _userManager
                .GeneratePasswordResetTokenAsync(
                    user);

        var urlSafeToken =
            EncodeIdentityToken(identityToken);

        var message =
            _emailFactory.CreatePasswordReset(
                user.FullName,
                user.Email!,
                user.Id,
                urlSafeToken);

        await _emailSender.SendAsync(
            message,
            cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(
        ResetPasswordCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        cancellationToken.ThrowIfCancellationRequested();

        var user =
            await _userManager.FindByIdAsync(
                command.UserId.ToString());

        if (user is null)
        {
            return Result.Failure(
                AuthenticationErrors
                    .InvalidPasswordResetToken);
        }

        if (
            !TryDecodeIdentityToken(
                command.Token,
                out var identityToken)
        )
        {
            return Result.Failure(
                AuthenticationErrors
                    .InvalidPasswordResetToken);
        }

        var resetResult =
            await _userManager.ResetPasswordAsync(
                user,
                identityToken,
                command.NewPassword);

        if (!resetResult.Succeeded)
        {
            var tokenIsInvalid =
                resetResult.Errors.Any(
                    error =>
                        string.Equals(
                            error.Code,
                            "InvalidToken",
                            StringComparison.OrdinalIgnoreCase));

            return tokenIsInvalid
                ? Result.Failure(
                    AuthenticationErrors
                        .InvalidPasswordResetToken)
                : Result.Failure(
                    ToAuthenticationError(resetResult));
        }

        var activeRefreshTokens =
            await _dbContext.RefreshTokens
                .Where(
                    token =>
                        token.UserId == user.Id &&
                        token.RevokedAtUtc == null)
                .ToListAsync(cancellationToken);

        var utcNow = _clock.UtcNow;

        foreach (var refreshToken in activeRefreshTokens)
        {
            if (refreshToken.IsActive(utcNow))
            {
                refreshToken.Revoke(
                    utcNow,
                    revokedByIp: null,
                    replacedByTokenHash: null,
                    reason: "password-reset");
            }
        }

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }
}
