using Microsoft.AspNetCore.Mvc;

namespace MongoDB_Web.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        [HttpPost("login")]
        public ActionResult Login(string username, string password)
        {
            return Ok();
        }
    }
}
