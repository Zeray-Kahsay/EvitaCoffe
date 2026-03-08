using System.Security.Claims;
using EvitaCoffee.Application.Common;
using EvitaCoffee.Contracts.Common;
using Microsoft.AspNetCore.Mvc;

namespace EvitaCoffee.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BaseController : ControllerBase 
{
    protected string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User Id not found in token");
    }

    protected string GetUserEmail()
    {
        return User.FindFirstValue(ClaimTypes.Email)
              ?? throw new UnauthorizedAccessException("User email not found");
    }

    protected string GetUserPhoneNumber()
    {
        return User.FindFirstValue(ClaimTypes.HomePhone)
                ?? throw new UnauthorizedAccessException("User phone number not found");
    }

    protected IActionResult HandleResult<T>(Result<T> result)
    {
        var correlationId = HttpContext.Items["CorrelationId"]?.ToString();
        
        if ( !result.IsSuccess)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Error = result.Error,
                CorrelationId = correlationId!
            });
        }

        return Ok(new ApiResponse<T>
        {
            Success = true,
            Data = result.Value,
            CorrelationId = correlationId!
        });

        

        // return result.Error.Code switch
        // {
        //     "NOT_FOUND" => NotFound(error),
        //     "UNAUTHORIZED" => Unauthorized(error),
        //     "FORBIDDEN" => StatusCode(StatusCodes.Status403Forbidden, error),
        //     "BAD_REQUEST" => BadRequest(error),
        //     _=> StatusCode(StatusCodes.Status500InternalServerError, error)
        // };
    }
}
