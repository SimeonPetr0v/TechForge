using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechForge.Web.Models;

namespace TechForge.Web.Controllers;

[AllowAnonymous]
public class ErrorController : Controller
{
    [Route("Error/{statusCode:int}")]
    public IActionResult HttpStatusCodeHandler(int statusCode)
    {
        Response.StatusCode = statusCode;

        var viewName = statusCode switch
        {
            400 => "400",
            401 => "401",
            403 => "403",
            404 => "404",
            500 => "500",
            _ => "Error"
        };

        return View(viewName, new ErrorViewModel { StatusCode = statusCode });
    }
}
