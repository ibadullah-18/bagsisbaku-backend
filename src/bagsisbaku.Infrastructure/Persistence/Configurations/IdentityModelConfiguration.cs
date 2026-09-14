using bagsisbaku.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace bagsisbaku.Infrastructure.Persistence.Configurations;

internal static class IdentityModelConfiguration
{
    public static void ConfigureIdentityModel(
        this ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(
            modelBuilder);

        modelBuilder.Entity<AppUser>(
            builder =>
            {
                builder.ToTable(
                    "users",
                    "identity");

                builder.Property(user => user.FullName)
                    .HasMaxLength(160)
                    .IsRequired();

                builder.Property(user => user.IsActive)
                    .IsRequired();

                builder.Property(user => user.CreatedAtUtc)
                    .IsRequired();

                builder.Property(user => user.UpdatedAtUtc);

                builder.Property(user => user.LastLoginAtUtc);

                builder.HasIndex(user => user.IsActive);

                builder
                    .HasMany(user => user.RefreshTokens)
                    .WithOne(token => token.User)
                    .HasForeignKey(token => token.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

        modelBuilder.Entity<AppRole>(
            builder =>
            {
                builder.ToTable(
                    "roles",
                    "identity");

                builder.Property(role => role.Description)
                    .HasMaxLength(300);
            });

        modelBuilder
            .Entity<IdentityUserRole<Guid>>()
            .ToTable(
                "user_roles",
                "identity");

        modelBuilder
            .Entity<IdentityUserClaim<Guid>>()
            .ToTable(
                "user_claims",
                "identity");

        modelBuilder
            .Entity<IdentityRoleClaim<Guid>>()
            .ToTable(
                "role_claims",
                "identity");

        modelBuilder
            .Entity<IdentityUserLogin<Guid>>()
            .ToTable(
                "user_logins",
                "identity");

        modelBuilder
            .Entity<IdentityUserToken<Guid>>()
            .ToTable(
                "user_tokens",
                "identity");

        modelBuilder.Entity<RefreshToken>(
            builder =>
            {
                builder.ToTable(
                    "refresh_tokens",
                    "identity");

                builder.HasKey(token => token.Id);

                builder.Property(token => token.TokenHash)
                    .HasMaxLength(128)
                    .IsRequired();

                builder.Property(token => token.CreatedByIp)
                    .HasMaxLength(64);

                builder.Property(token => token.RevokedByIp)
                    .HasMaxLength(64);

                builder
                    .Property(
                        token =>
                            token.ReplacedByTokenHash)
                    .HasMaxLength(128);

                builder.Property(token => token.RevokeReason)
                    .HasMaxLength(300);

                builder.Property(token => token.RowVersion)
                    .IsRowVersion();

                builder.HasIndex(token => token.TokenHash)
                    .IsUnique();

                builder.HasIndex(
                    token =>
                        new
                        {
                            token.UserId,
                            token.ExpiresAtUtc
                        });

                builder.HasIndex(
                    token =>
                        new
                        {
                            token.UserId,
                            token.RevokedAtUtc
                        });
            });
    }
}
