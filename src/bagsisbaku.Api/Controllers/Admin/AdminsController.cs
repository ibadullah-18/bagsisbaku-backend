using bagsisbaku.Api.Extensions;
using bagsisbaku.Application.Administration.Admins;
using bagsisbaku.Application.Security;
using bagsisbaku.Contracts.Administration.Admins;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bagsisbaku.Api.Controllers.Admin;

[ApiController]
[Authorize(Roles = SystemRoles.SuperAdmin)]
[Route("api/admin/admins")]
public sealed class AdminsController(
    IAdminManagementService adminManagementService)
    : ControllerBase
{
    [HttpGet("permissions")]
    public ActionResult<
        IReadOnlyCollection<PermissionDefinitionResponse>>
        GetPermissions()
    {
        var permissions =
            PermissionCatalog.All
                .Select(
                    permission =>
                        new PermissionDefinitionResponse(
                            permission.Name,
                            permission.Group,
                            permission.DisplayName))
                .ToArray();

        return Ok(permissions);
    }

    [HttpGet]
    public async Task<
        ActionResult<IReadOnlyCollection<AdminResponse>>>
        GetAllAsync(
            CancellationToken cancellationToken)
    {
        var admins =
            await adminManagementService.GetAllAsync(
                cancellationToken);

        var response =
            admins
                .Select(ToResponse)
                .ToArray();

        return Ok(response);
    }

    [HttpGet("{adminId:guid}")]
    public async Task<ActionResult<AdminResponse>>
        GetByIdAsync(
            Guid adminId,
            CancellationToken cancellationToken)
    {
        var result =
            await adminManagementService.GetByIdAsync(
                adminId,
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        return Ok(
            ToResponse(result.Value));
    }

    [HttpPost]
    public async Task<ActionResult<AdminResponse>>
        CreateAsync(
            CreateAdminRequest request,
            CancellationToken cancellationToken)
    {
        var result =
            await adminManagementService.CreateAsync(
                new CreateAdminCommand(
                    request.FullName,
                    request.Email,
                    request.Password,
                    request.Permissions),
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        return StatusCode(
            StatusCodes.Status201Created,
            ToResponse(result.Value));
    }

    [HttpPut("{adminId:guid}/permissions")]
    public async Task<IActionResult>
        UpdatePermissionsAsync(
            Guid adminId,
            UpdateAdminPermissionsRequest request,
            CancellationToken cancellationToken)
    {
        var result =
            await adminManagementService
                .UpdatePermissionsAsync(
                    new UpdateAdminPermissionsCommand(
                        adminId,
                        request.Permissions),
                    cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        return NoContent();
    }

    [HttpPut("{adminId:guid}/status")]
    public async Task<IActionResult> SetStatusAsync(
        Guid adminId,
        SetAdminStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await adminManagementService.SetStatusAsync(
                new SetAdminStatusCommand(
                    adminId,
                    request.IsActive),
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        return NoContent();
    }

    private static AdminResponse ToResponse(
        AdminModel admin)
    {
        return new AdminResponse(
            admin.Id,
            admin.FullName,
            admin.Email,
            admin.IsActive,
            admin.CreatedAtUtc,
            admin.LastLoginAtUtc,
            admin.Permissions);
    }
}
