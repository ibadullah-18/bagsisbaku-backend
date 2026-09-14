using bagsisbaku.Application.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace bagsisbaku.Api.Extensions;

internal static class ControllerResultExtensions
{
    public static ActionResult ToProblemResult(
        this ControllerBase controller,
        Error error)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentNullException.ThrowIfNull(error);

        var statusCode = error.Type switch
        {
            ErrorType.Validation =>
                StatusCodes.Status400BadRequest,

            ErrorType.Unauthorized =>
                StatusCodes.Status401Unauthorized,

            ErrorType.Forbidden =>
                StatusCodes.Status403Forbidden,

            ErrorType.NotFound =>
                StatusCodes.Status404NotFound,

            ErrorType.Conflict =>
                StatusCodes.Status409Conflict,

            _ =>
                StatusCodes.Status500InternalServerError
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = error.Code,
            Detail = error.Description
        };

        problemDetails.Extensions["code"] =
            error.Code;

        return controller.StatusCode(
            statusCode,
            problemDetails);
    }
}
