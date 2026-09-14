namespace bagsisbaku.Infrastructure.Identity;

public interface IIdentityBootstrapper
{
    Task InitializeAsync(
        CancellationToken cancellationToken = default);
}
