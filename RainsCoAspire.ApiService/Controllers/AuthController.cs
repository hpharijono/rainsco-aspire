using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RainsCoAspire.ApiService.Dtos;
using RainsCoAspire.ApiService.Security;
using RainsCoAspire.ApiService.Services;

namespace RainsCoAspire.ApiService.Controllers;

[ApiController]
[Route("api/auth")]
[Authorize]
public class AuthController(IAuthService authService, IUserService userService) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await authService.LoginAsync(request, cancellationToken);
        if (response is null)
        {
            return Problem(title: "Invalid username or password.", statusCode: StatusCodes.Status401Unauthorized);
        }

        return response;
    }

    // The signed-in user. Clients use it to check that a stored token is still valid.
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserResponse>> GetCurrentUser(CancellationToken cancellationToken)
    {
        var user = await userService.GetByIdAsync(User.GetUserId(), cancellationToken);
        return user is null ? Unauthorized() : user;
    }

    // Changes the signed-in user's own password. This signs out their other sessions, and the response carries the
    // new token to use from now on.
    [HttpPost("change-password")]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> ChangePassword(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await authService.ChangePasswordAsync(User.GetUserId(), request, cancellationToken);
            return response is null ? Unauthorized() : response;
        }
        catch (RequestValidationException ex)
        {
            return this.ToValidationProblem(ex);
        }
    }
}
