using Microsoft.AspNetCore.Mvc;
using MongoDB_Web.Server.Filters;

namespace MongoDB_Web.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorization]
    public class DbController : DbBaseController
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Ok("You are authorized");
        }
    }
}
