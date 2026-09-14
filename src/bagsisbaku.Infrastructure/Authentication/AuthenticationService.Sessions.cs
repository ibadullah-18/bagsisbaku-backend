using bagsisbaku.Application.Abstractions.Authentication;
using bagsisbaku.Application.Authentication;
using bagsisbaku.Application.Common.Results;
using bagsisbaku.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace bagsisbaku.Infrastructure.Authentication;

internal sealed partial class AuthenticationService
{
    public async Task<Result<AuthenticationSessionModel>>
        LoginAsync(
            LoginUserCommand command,
            string? ipAddress,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        cancellationToken.ThrowIfCancellationRequested();

        var user =
            await _userManager.FindByEmailAsync(
                command.Email.Trim());

        if (user is null)
        {
            return Result.Failure<AuthenticationSessionModel>(
                AuthenticationErrors.InvalidCredentials);
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            return Result.Failure<AuthenticationSessionModel>(
                AuthenticationErrors.InvalidCredentials);
        }

        var passwordIsCorrect =
            await _userManager.CheckPasswordAsync(
                user,
                command.Password);

        if (!passwordIsCorrect)
        {
            await _userManager.AccessFailedAsync(user);

            return Result.Failure<AuthenticationSessionModel>(
                AuthenticationErrors.InvalidCredentials);
        }

        if (!user.IsActive)
        {
            return Result.Failure<AuthenticationSessionModel>(
                AuthenticationErrors.AccountInactive);
        }

        if (!user.EmailConfirmed)
        {
            return Result.Failure<AuthenticationSessionModel>(
                AuthenticationErrors.EmailNotConfirmed);
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        var session =
            await CreateSessionAsync(
                user,
                ipAddress,
                tokenToRotate: null,
                recordLogin: true,
                cancellationToken);

        return Result.Success(session);
    }

    public async Task<Result<AuthenticationSessionModel>>
        RefreshSessionAsync(
            string refreshToken,
            string? ipAddress,
            CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Result.Failure<AuthenticationSessionModel>(
                AuthenticationErrors.InvalidRefreshToken);
        }

        var tokenHash =
            _refreshTokenGenerator.Hash(
                refreshToken);

        var storedToken =
            await _dbContext.RefreshTokens
                .Include(token => token.User)
                .SingleOrDefaultAsync(
                    token =>
                        token.TokenHash == tokenHash,
                    cancellationToken);

        if (
            storedToken is null ||
            !storedToken.IsActive(_clock.UtcNow)
        )
        {
            return Result.Failure<AuthenticationSessionModel>(
                AuthenticationErrors.InvalidRefreshToken);
        }

        var user = storedToken.User;

        if (!user.IsActive)
        {
            return Result.Failure<AuthenticationSessionModel>(
                AuthenticationErrors.AccountInactive);
        }

        if (!user.EmailConfirmed)
        {
            return Result.Failure<AuthenticationSessionModel>(
                AuthenticationErrors.EmailNotConfirmed);
        }

        var session =
            await CreateSessionAsync(
                user,
                ipAddress,
                storedToken,
                recordLogin: false,
                cancellationToken);

        return Result.Success(session);
    }

    public async Task<Result> LogoutAsync(
        string refreshToken,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Result.Success();
        }

        var tokenHash =
            _refreshTokenGenerator.Hash(
                refreshToken);

        var storedToken =
            await _dbContext.RefreshTokens
                .SingleOrDefaultAsync(
                    token =>
                        token.TokenHash == tokenHash,
                    cancellationToken);

        // Logout idempotent saxlanılır.
        if (storedToken is null)
        {
            return Result.Success();
        }

        if (storedToken.IsActive(_clock.UtcNow))
        {
            storedToken.Revoke(
                _clock.UtcNow,
                ipAddress,
                replacedByTokenHash: null,
                reason: "logout");

            await _dbContext.SaveChangesAsync(
                cancellationToken);
        }

        return Result.Success();
    }

    private async Task<AuthenticationSessionModel>
        CreateSessionAsync(
            AppUser user,
            string? ipAddress,
            RefreshToken? tokenToRotate,
            bool recordLogin,
            CancellationToken cancellationToken)
    {
        var roles =
            (await _userManager.GetRolesAsync(user))
                .ToArray();

        var permissions =
            await GetPermissionsAsync(user, roles);

        var accessToken =
            _accessTokenGenerator.Generate(
                new Application.Abstractions.Authentication
                    .AccessTokenUser(
                        user.Id,
                        user.Email!,
                        user.FullName,
                        roles,
                        permissions));

        var generatedRefreshToken =
            _refreshTokenGenerator.Generate();

        var utcNow = _clock.UtcNow;

        var refreshTokenExpiresAtUtc =
            utcNow.AddDays(
                _jwtSettings.RefreshTokenDays);

        var newRefreshToken =
            RefreshToken.Create(
                user.Id,
                generatedRefreshToken.TokenHash,
                utcNow,
                refreshTokenExpiresAtUtc,
                ipAddress);

        if (tokenToRotate is not null)
        {
            tokenToRotate.Revoke(
                utcNow,
                ipAddress,
                generatedRefreshToken.TokenHash,
                "rotated");
        }

        if (recordLogin)
        {
            user.RecordLogin(utcNow);
        }

        user.AddRefreshToken(newRefreshToken);

        _dbContext.RefreshTokens.Add(
            newRefreshToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return new AuthenticationSessionModel(
            accessToken.Token,
            accessToken.ExpiresAtUtc,
            generatedRefreshToken.Token,
            refreshTokenExpiresAtUtc,
            new AuthenticatedUserModel(
                user.Id,
                user.FullName,
                user.Email!,
                roles,
                permissions));
    }
}



