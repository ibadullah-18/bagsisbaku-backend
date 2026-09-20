using bagsisbaku.Api.Authorization;
using bagsisbaku.Application.Dashboard;
using bagsisbaku.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bagsisbaku.Api.Controllers.Admin;

[ApiController]
[Authorize]
[Route("api/admin/dashboard")]
public sealed class AdminDashboardController(
    IAdminDashboardQuery dashboardQuery)
    : ControllerBase
{
    [HttpGet]
    [HasPermission(PermissionNames.Dashboard.View)]
    public async Task<ActionResult<AdminDashboardModel>> GetAsync(
        CancellationToken cancellationToken)
    {
        var dashboard =
            await dashboardQuery.GetAsync(cancellationToken);

        return Ok(dashboard);
    }
}