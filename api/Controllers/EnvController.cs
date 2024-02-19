using api.Helpers;
using Microsoft.AspNetCore.Mvc;
using api.Filters;

namespace api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EnvController
{
    [HttpGet("isAdmin")]
    [Authorization(IsRequiredAdmin = true)]
    public ActionResult<bool> IsAdmin()
    {
        return true;
    }

    [HttpGet("isAuthorized")]
    [Authorization]
    public ActionResult<bool> IsAuthorized()
    {
        return true;
    }

    [HttpGet("isAuthorizationEnabled")]
    public ActionResult<bool> IsAuthorizationEnabled()
    {
        return ConfigManager.useAuthorization;
    }

    [HttpGet("isFirstStart")]
    public ActionResult<bool> IsFirstStart()
    {
        return ConfigManager.firstStart;
    }
}