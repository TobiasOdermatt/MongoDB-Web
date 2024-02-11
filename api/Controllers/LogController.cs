using api.Helpers;
using api.Models;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LogController : Controller
{
    [HttpGet("CountLog")]
    public ActionResult CountLog(DateTime date)
    {
        if (date == DateTime.MinValue)
            return BadRequest("Invalid or missing date parameter.");

        var (infoCount, warningCount, errorCount) = LogManager.CountLog(date);
        var result = new
        {
            InfoCount = infoCount,
            WarningCount = warningCount,
            ErrorCount = errorCount
        };

        return Ok(result);
    }


    [HttpGet("AvailableLogDates")]
    public ActionResult<List<DateTime>> GetAvailableLogDates()
    {
        var dates = LogManager.GetAvailableLogDates();
        return Ok(dates);
    }

    [HttpGet("ReadLogFiles")]
    public ActionResult<List<LogObject>> ReadLogFiles(string type, DateTime? date)
    {
        if (string.IsNullOrEmpty(type))
            return BadRequest("Type parameter is required.");

        if (!date.HasValue)
            return BadRequest("Date parameter is required.");

        var logObjects = LogManager.ReadLogFiles(type, date.Value);
        return Ok(logObjects);
    }

}