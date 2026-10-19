using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RainsCoAspire.ApiService.Dtos;
using RainsCoAspire.ApiService.Security;
using RainsCoAspire.ApiService.Services;

namespace RainsCoAspire.ApiService.Controllers;

// Any signed-in user can view rental units; only admins can change them.
[ApiController]
[Route("api/rental-units")]
[Authorize]
public class RentalUnitsController(IRentalUnitService rentalUnitService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RentalUnitResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var units = await rentalUnitService.GetAllAsync(cancellationToken);
        return Ok(units);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RentalUnitResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var unit = await rentalUnitService.GetByIdAsync(id, cancellationToken);
        return unit is null ? NotFound() : unit;
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.Admin)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RentalUnitResponse>> Create([FromForm] CreateRentalUnitRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var unit = await rentalUnitService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = unit.Id }, unit);
        }
        catch (RequestValidationException ex)
        {
            return this.ToValidationProblem(ex);
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = AppRoles.Admin)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RentalUnitResponse>> Update(int id, [FromForm] UpdateRentalUnitRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var unit = await rentalUnitService.UpdateAsync(id, request, cancellationToken);
            return unit is null ? NotFound() : unit;
        }
        catch (RequestValidationException ex)
        {
            return this.ToValidationProblem(ex);
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = AppRoles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await rentalUnitService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
