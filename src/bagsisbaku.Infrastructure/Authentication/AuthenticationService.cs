using bagsisbaku.Application.Abstractions.Authentication;
using bagsisbaku.Application.Abstractions.Email;
using bagsisbaku.Application.Abstractions.Time;
using bagsisbaku.Application.Authentication;
using bagsisbaku.Infrastructure.Identity;
using bagsisbaku.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;

namespace bagsisbaku.Infrastructure.Authentication;

internal sealed partial class AuthenticationService
    : IAuthenticationService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly IAccessTokenGenerator _accessTokenGenerator;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly IAuthenticationEmailFactory _emailFactory;
    private readonly IEmailSender _emailSender;
    private readonly IClock _clock;
    private readonly JwtSettings _jwtSettings;

    public AuthenticationService(
        UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager,
        ApplicationDbContext dbContext,
        IAccessTokenGenerator accessTokenGenerator,
        IRefreshTokenGenerator refreshTokenGenerator,
        IAuthenticationEmailFactory emailFactory,
        IEmailSender emailSender,
        IClock clock,
        JwtSettings jwtSettings)
    {
        ArgumentNullException.ThrowIfNull(userManager);
        ArgumentNullException.ThrowIfNull(roleManager);
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(accessTokenGenerator);
        ArgumentNullException.ThrowIfNull(refreshTokenGenerator);
        ArgumentNullException.ThrowIfNull(emailFactory);
        ArgumentNullException.ThrowIfNull(emailSender);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(jwtSettings);

        _userManager = userManager;
        _roleManager = roleManager;
        _dbContext = dbContext;
        _accessTokenGenerator = accessTokenGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
        _emailFactory = emailFactory;
        _emailSender = emailSender;
        _clock = clock;
        _jwtSettings = jwtSettings;
    }
}
