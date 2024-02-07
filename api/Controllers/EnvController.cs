using Microsoft.AspNetCore.Mvc;
using mongodbweb.Server.Filters;
using mongodbweb.Server.Helpers;

namespace mongodbweb.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EnvController
{
    [HttpGet("IsAdmin")]
    [Authorization(IsRequiredAdmin = true)]
    public ActionResult<bool> IsAdmin()
    {
        return true;
    }
    
    [HttpGet("IsAuthorized")]
    [Authorization]
    public ActionResult<bool> IsAuthorized()
    {
        return true;
    }
    
    [HttpGet("IsAuthorizationEnabled")]
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