using Microsoft.AspNetCore.Mvc;
using RainsCoAspire.ApiService.Services;

namespace RainsCoAspire.ApiService.Controllers;

public static class ControllerBaseExtensions
{
    // Produces the same response shape as the automatic [ApiController] model validation errors.
    public static ActionResult ToValidationProblem(this ControllerBase controller, RequestValidationException exception)
    {
        foreach (var (field, messages) in exception.Errors)
        {
            foreach (var message in messages)
            {
                controller.ModelState.AddModelError(field, message);
            }
        }

        return controller.ValidationProblem(controller.ModelState);
    }
}
