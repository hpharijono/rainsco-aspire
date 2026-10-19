using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RainsCoAspire.ApiService.Dtos;
using RainsCoAspire.ApiService.Security;
using RainsCoAspire.ApiService.Services;

namespace RainsCoAspire.ApiService.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = AppRoles.Admin)]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var users = await userService.GetAllAsync(cancellationToken);
        return Ok(users);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var user = await userService.GetByIdAsync(id, cancellationToken);
        return user is null ? NotFound() : user;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserResponse>> Create(CreateUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await userService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }
        catch (RequestValidationException ex)
        {
            return this.ToValidationProblem(ex);
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> Update(int id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await userService.UpdateAsync(id, request, cancellationToken);
            return user is null ? NotFound() : user;
        }
        catch (RequestValidationException ex)
        {
            return this.ToValidationProblem(ex);
        }
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await userService.DeleteAsync(id, User.GetUserId(), cancellationToken);
            return deleted ? NoContent() : NotFound();
        }
        catch (RequestValidationException ex)
        {
            return this.ToValidationProblem(ex);
        }
    }

    // Sets a new password for the user, e.g. when they've forgotten theirs. Their existing sessions are signed out.
    [HttpPost("{id:int}/reset-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetPassword(int id, ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var reset = await userService.ResetPasswordAsync(id, request, cancellationToken);
        return reset ? NoContent() : NotFound();
    }
}
