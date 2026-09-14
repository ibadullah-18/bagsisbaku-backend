using bagsisbaku.Api.Extensions;
using bagsisbaku.Application.Customers;
using bagsisbaku.Contracts.Customers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bagsisbaku.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/profile")]
public sealed class ProfileController(
    ICustomerProfileService customerProfileService)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<CustomerProfileResponse>>
        GetProfileAsync(
            CancellationToken cancellationToken)
    {
        var result =
            await customerProfileService.GetProfileAsync(
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        return Ok(
            ToProfileResponse(result.Value));
    }

    [HttpPut]
    public async Task<ActionResult<CustomerProfileResponse>>
        UpdateProfileAsync(
            [FromBody] UpdateCustomerProfileRequest request,
            CancellationToken cancellationToken)
    {
        var command =
            new UpdateCustomerProfileCommand(
                request.FullName,
                request.PhoneNumber);

        var result =
            await customerProfileService.UpdateProfileAsync(
                command,
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        return Ok(
            ToProfileResponse(result.Value));
    }

    [HttpGet("addresses")]
    public async Task<
        ActionResult<IReadOnlyList<CustomerAddressResponse>>>
        GetAddressesAsync(
            CancellationToken cancellationToken)
    {
        var result =
            await customerProfileService.GetAddressesAsync(
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        var response =
            result.Value
                .Select(ToAddressResponse)
                .ToArray();

        return Ok(response);
    }

    [HttpGet("addresses/{addressId:guid}")]
    public async Task<ActionResult<CustomerAddressResponse>>
        GetAddressByIdAsync(
            Guid addressId,
            CancellationToken cancellationToken)
    {
        var result =
            await customerProfileService.GetAddressByIdAsync(
                addressId,
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        return Ok(
            ToAddressResponse(result.Value));
    }

    [HttpPost("addresses")]
    public async Task<ActionResult<CustomerAddressResponse>>
        CreateAddressAsync(
            [FromBody] CreateCustomerAddressRequest request,
            CancellationToken cancellationToken)
    {
        var command =
            new CreateCustomerAddressCommand(
                request.Title,
                request.RecipientFullName,
                request.PhoneNumber,
                request.City,
                request.District,
                request.AddressLine,
                request.PostalCode,
                request.DeliveryNote,
                request.Latitude,
                request.Longitude,
                request.IsDefault);

        var result =
            await customerProfileService.CreateAddressAsync(
                command,
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        var response =
            ToAddressResponse(result.Value);

        return Created(
            $"/api/profile/addresses/{response.Id}",
            response);
    }

    [HttpPut("addresses/{addressId:guid}")]
    public async Task<ActionResult<CustomerAddressResponse>>
        UpdateAddressAsync(
            Guid addressId,
            [FromBody] UpdateCustomerAddressRequest request,
            CancellationToken cancellationToken)
    {
        var command =
            new UpdateCustomerAddressCommand(
                request.Title,
                request.RecipientFullName,
                request.PhoneNumber,
                request.City,
                request.District,
                request.AddressLine,
                request.PostalCode,
                request.DeliveryNote,
                request.Latitude,
                request.Longitude);

        var result =
            await customerProfileService.UpdateAddressAsync(
                addressId,
                command,
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        return Ok(
            ToAddressResponse(result.Value));
    }

    [HttpPut("addresses/{addressId:guid}/default")]
    public async Task<IActionResult>
        SetDefaultAddressAsync(
            Guid addressId,
            CancellationToken cancellationToken)
    {
        var result =
            await customerProfileService
                .SetDefaultAddressAsync(
                    addressId,
                    cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        return NoContent();
    }

    [HttpDelete("addresses/{addressId:guid}")]
    public async Task<IActionResult>
        DeleteAddressAsync(
            Guid addressId,
            CancellationToken cancellationToken)
    {
        var result =
            await customerProfileService.DeleteAddressAsync(
                addressId,
                cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblemResult(
                result.Error);
        }

        return NoContent();
    }

    private static CustomerProfileResponse
        ToProfileResponse(
            CustomerProfileModel profile)
    {
        return new CustomerProfileResponse(
            profile.Id,
            profile.FullName,
            profile.Email,
            profile.PhoneNumber,
            profile.EmailConfirmed,
            profile.IsActive,
            profile.CreatedAtUtc,
            profile.UpdatedAtUtc);
    }

    private static CustomerAddressResponse
        ToAddressResponse(
            CustomerAddressModel address)
    {
        return new CustomerAddressResponse(
            address.Id,
            address.Title,
            address.RecipientFullName,
            address.PhoneNumber,
            address.City,
            address.District,
            address.AddressLine,
            address.PostalCode,
            address.DeliveryNote,
            address.Latitude,
            address.Longitude,
            address.IsDefault,
            address.CreatedAtUtc,
            address.UpdatedAtUtc);
    }
}