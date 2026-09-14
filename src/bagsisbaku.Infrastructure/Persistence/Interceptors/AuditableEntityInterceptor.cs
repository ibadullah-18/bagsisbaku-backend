using bagsisbaku.Application.Abstractions.Time;
using bagsisbaku.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace bagsisbaku.Infrastructure.Persistence.Interceptors;

internal sealed class AuditableEntityInterceptor(
    IClock clock)
    : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        UpdateAuditProperties(eventData.Context);

        return base.SavingChanges(
            eventData,
            result);
    }

    public override ValueTask<InterceptionResult<int>>
        SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
    {
        UpdateAuditProperties(eventData.Context);

        return base.SavingChangesAsync(
            eventData,
            result,
            cancellationToken);
    }

    private void UpdateAuditProperties(
        DbContext? dbContext)
    {
        if (dbContext is null)
        {
            return;
        }

        var utcNow = clock.UtcNow;

        var entries = dbContext.ChangeTracker
            .Entries<IAuditableEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAtUtc = utcNow;
                entry.Entity.UpdatedAtUtc = null;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAtUtc = utcNow;

                entry.Property(
                        nameof(IAuditableEntity.CreatedAtUtc))
                    .IsModified = false;
            }
        }
    }
}
