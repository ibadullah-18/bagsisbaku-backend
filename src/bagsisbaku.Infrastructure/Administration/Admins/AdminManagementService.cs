using bagsisbaku.Application.Abstractions.Time;
using bagsisbaku.Application.Administration.Admins;
using bagsisbaku.Infrastructure.Identity;
using bagsisbaku.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;

namespace bagsisbaku.Infrastructure.Administration.Admins;

internal sealed partial class AdminManagementService
    : IAdminManagementService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly IClock _clock;

    public AdminManagementService(
        UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager,
        ApplicationDbContext dbContext,
        IClock clock)
    {
        ArgumentNullException.ThrowIfNull(userManager);
        ArgumentNullException.ThrowIfNull(roleManager);
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(clock);

        _userManager = userManager;
        _roleManager = roleManager;
        _dbContext = dbContext;
        _clock = clock;
    }
}
