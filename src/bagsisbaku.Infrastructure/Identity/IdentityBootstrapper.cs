using bagsisbaku.Application.Abstractions.Time;
using bagsisbaku.Application.Security;
using Microsoft.AspNetCore.Identity;

namespace bagsisbaku.Infrastructure.Identity;

internal sealed class IdentityBootstrapper(
    UserManager<AppUser> userManager,
    RoleManager<AppRole> roleManager,
    IClock clock,
    IdentityBootstrapSettings settings)
    : IIdentityBootstrapper
{
    private static readonly (
        string Name,
        string Description)[] RoleDefinitions =
    [
        (
            SystemRoles.SuperAdmin,
            "Bütün sistem icazələrinə sahib əsas idarəçi"
        ),
        (
            SystemRoles.Admin,
            "Superadmin tərəfindən idarə olunan admin"
        ),
        (
            SystemRoles.Customer,
            "Müştəri hesabı"
        )
    ];

    public async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        if (!settings.Enabled)
        {
            return;
        }

        settings.Validate();

        cancellationToken.ThrowIfCancellationRequested();

        foreach (var roleDefinition in RoleDefinitions)
        {
            await EnsureRoleAsync(
                roleDefinition.Name,
                roleDefinition.Description,
                cancellationToken);
        }

        await EnsureSuperAdminAsync(
            cancellationToken);
    }

    private async Task EnsureRoleAsync(
        string roleName,
        string description,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (await roleManager.RoleExistsAsync(roleName))
        {
            return;
        }

        var role =
            AppRole.Create(
                roleName,
                description);

        var result =
            await roleManager.CreateAsync(role);

        if (result.Succeeded)
        {
            return;
        }

        // Eyni anda başqa instance rolu yaratmış ola bilər.
        if (await roleManager.RoleExistsAsync(roleName))
        {
            return;
        }

        throw CreateIdentityException(
            $"'{roleName}' rolu yaradıla bilmədi.",
            result);
    }

    private async Task EnsureSuperAdminAsync(
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var email =
            settings.SuperAdminEmail!.Trim();

        var fullName =
            settings.SuperAdminFullName!.Trim();

        var user =
            await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = AppUser.Create(
                fullName,
                email,
                clock.UtcNow);

            user.EmailConfirmed = true;

            var createResult =
                await userManager.CreateAsync(
                    user,
                    settings.SuperAdminPassword!);

            if (!createResult.Succeeded)
            {
                throw CreateIdentityException(
                    "İlk superadmin yaradıla bilmədi.",
                    createResult);
            }
        }
        else
        {
            var userChanged = false;

            if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                userChanged = true;
            }

            if (!user.IsActive)
            {
                user.Activate(clock.UtcNow);
                userChanged = true;
            }

            if (
                !string.Equals(
                    user.FullName,
                    fullName,
                    StringComparison.Ordinal)
            )
            {
                user.SetFullName(fullName);
                userChanged = true;
            }

            if (userChanged)
            {
                var updateResult =
                    await userManager.UpdateAsync(user);

                if (!updateResult.Succeeded)
                {
                    throw CreateIdentityException(
                        "Superadmin hesabı yenilənə bilmədi.",
                        updateResult);
                }
            }
        }

        if (
            !await userManager.IsInRoleAsync(
                user,
                SystemRoles.SuperAdmin)
        )
        {
            var addRoleResult =
                await userManager.AddToRoleAsync(
                    user,
                    SystemRoles.SuperAdmin);

            if (!addRoleResult.Succeeded)
            {
                throw CreateIdentityException(
                    "Superadmin rolu istifadəçiyə verilə bilmədi.",
                    addRoleResult);
            }
        }

        await RemoveLowerRoleAsync(
            user,
            SystemRoles.Admin);

        await RemoveLowerRoleAsync(
            user,
            SystemRoles.Customer);
    }

    private async Task RemoveLowerRoleAsync(
        AppUser user,
        string roleName)
    {
        if (
            !await userManager.IsInRoleAsync(
                user,
                roleName)
        )
        {
            return;
        }

        var result =
            await userManager.RemoveFromRoleAsync(
                user,
                roleName);

        if (!result.Succeeded)
        {
            throw CreateIdentityException(
                $"'{roleName}' rolu superadmindən silinə bilmədi.",
                result);
        }
    }

    private static InvalidOperationException
        CreateIdentityException(
            string message,
            IdentityResult identityResult)
    {
        var details =
            string.Join(
                " ",
                identityResult.Errors
                    .Select(error => error.Description));

        return new InvalidOperationException(
            $"{message} {details}".Trim());
    }
}
