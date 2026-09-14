using bagsisbaku.Application.Abstractions.Email;

namespace bagsisbaku.Application.Authentication;

public interface IAuthenticationEmailFactory
{
    EmailMessage CreateEmailConfirmation(
        string fullName,
        string email,
        Guid userId,
        string urlSafeToken);

    EmailMessage CreatePasswordReset(
        string fullName,
        string email,
        Guid userId,
        string urlSafeToken);
}
