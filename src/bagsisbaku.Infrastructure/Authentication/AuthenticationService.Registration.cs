using bagsisbaku.Application.Authentication;
using bagsisbaku.Application.Common.Results;
using bagsisbaku.Application.Security;
using bagsisbaku.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace bagsisbaku.Infrastructure.Authentication;

internal sealed partial class AuthenticationService
{
    public async Task<Result<RegistrationModel>> RegisterAsync(
        RegisterUserCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        cancellationToken.ThrowIfCancellationRequested();

        var email = command.Email.Trim();

        var existingUser =
            await _userManager.FindByEmailAsync(email);

        if (existingUser is not null)
        {
            return Result.Failure<RegistrationModel>(
                AuthenticationErrors.EmailAlreadyRegistered);
        }

        AppUser user;

        try
        {
            user = AppUser.Create(
                command.FullName,
                email,
                _clock.UtcNow);
        }
        catch (ArgumentException exception)
        {
            return Result.Failure<RegistrationModel>(
                AuthenticationErrors.IdentityValidation(
                    exception.Message));
        }

        var createResult =
            await _userManager.CreateAsync(
                user,
                command.Password);

        if (!createResult.Succeeded)
        {
            return Result.Failure<RegistrationModel>(
                ToAuthenticationError(createResult));
        }

        var roleResult =
            await EnsureCustomerRoleAsync();

        if (roleResult.IsFailure)
        {
            await _userManager.DeleteAsync(user);

            return Result.Failure<RegistrationModel>(
                roleResult.Error);
        }

        var addRoleResult =
            await _userManager.AddToRoleAsync(
                user,
                SystemRoles.Customer);

        if (!addRoleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);

            return Result.Failure<RegistrationModel>(
                ToAuthenticationError(addRoleResult));
        }

        await SendEmailConfirmationAsync(
            user,
            cancellationToken);

        return Result.Success(
            new RegistrationModel(
                user.Id,
                user.Email!,
                RequiresEmailConfirmation: true));
    }

    public async Task<Result> ConfirmEmailAsync(
        ConfirmEmailCommand command,
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
                AuthenticationErrors.UserNotFound);
        }

        if (user.EmailConfirmed)
        {
            return Result.Failure(
                AuthenticationErrors.EmailAlreadyConfirmed);
        }

        if (
            !TryDecodeIdentityToken(
                command.Token,
                out var identityToken)
        )
        {
            return Result.Failure(
                AuthenticationErrors
                    .InvalidEmailConfirmationToken);
        }

        var confirmationResult =
            await _userManager.ConfirmEmailAsync(
                user,
                identityToken);

        if (!confirmationResult.Succeeded)
        {
            return Result.Failure(
                AuthenticationErrors
                    .InvalidEmailConfirmationToken);
        }

        return Result.Success();
    }

    public async Task<Result> ResendEmailConfirmationAsync(
        ResendEmailConfirmationCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        cancellationToken.ThrowIfCancellationRequested();

        var user =
            await _userManager.FindByEmailAsync(
                command.Email.Trim());

        // Email enumeration hücumunun qarşısını alır.
        if (user is null)
        {
            return Result.Success();
        }

        if (user.EmailConfirmed)
        {
            return Result.Failure(
                AuthenticationErrors.EmailAlreadyConfirmed);
        }

        if (!user.IsActive)
        {
            return Result.Failure(
                AuthenticationErrors.AccountInactive);
        }

        await SendEmailConfirmationAsync(
            user,
            cancellationToken);

        return Result.Success();
    }

    private async Task<Result> EnsureCustomerRoleAsync()
    {
        if (
            await _roleManager.RoleExistsAsync(
                SystemRoles.Customer)
        )
        {
            return Result.Success();
        }

        var role =
            AppRole.Create(
                SystemRoles.Customer,
                "Müştəri hesabı");

        var createResult =
            await _roleManager.CreateAsync(role);

        if (createResult.Succeeded)
        {
            return Result.Success();
        }

        // Paralel qeydiyyat zamanı rol başqa request
        // tərəfindən yaradılmış ola bilər.
        if (
            await _roleManager.RoleExistsAsync(
                SystemRoles.Customer)
        )
        {
            return Result.Success();
        }

        return Result.Failure(
            ToAuthenticationError(createResult));
    }

    private async Task SendEmailConfirmationAsync(
        AppUser user,
        CancellationToken cancellationToken)
    {
        var identityToken =
            await _userManager
                .GenerateEmailConfirmationTokenAsync(
                    user);

        var urlSafeToken =
            EncodeIdentityToken(identityToken);

        var message =
            _emailFactory.CreateEmailConfirmation(
                user.FullName,
                user.Email!,
                user.Id,
                urlSafeToken);

        await _emailSender.SendAsync(
            message,
            cancellationToken);
    }
}
