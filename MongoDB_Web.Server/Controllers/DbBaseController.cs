using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace MongoDB_Web.Server.Controllers
{
    public class DbBaseController : Controller
    {
        public required MongoClient MongoClient { get; set; }
    }
}
