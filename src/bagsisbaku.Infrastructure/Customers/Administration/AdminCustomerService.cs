using bagsisbaku.Application.Abstractions.Authentication;
using bagsisbaku.Application.Abstractions.Time;
using bagsisbaku.Application.Customers.Administration;
using bagsisbaku.Infrastructure.Identity;
using bagsisbaku.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;

namespace bagsisbaku.Infrastructure.Customers.Administration;

internal sealed partial class AdminCustomerService
    : IAdminCustomerService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<AppUser> _userManager;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    private readonly AdminCustomerFilterValidator
        _filterValidator = new();

    private readonly SetCustomerStatusCommandValidator
        _statusValidator = new();

    public AdminCustomerService(
        ApplicationDbContext dbContext,
        UserManager<AppUser> userManager,
        ICurrentUser currentUser,
        IClock clock)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(userManager);
        ArgumentNullException.ThrowIfNull(currentUser);
        ArgumentNullException.ThrowIfNull(clock);

        _dbContext = dbContext;
        _userManager = userManager;
        _currentUser = currentUser;
        _clock = clock;
    }
}