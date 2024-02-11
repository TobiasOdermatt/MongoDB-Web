using Microsoft.AspNetCore.Mvc;
using api.Filters;
using System.Net.Mime;

namespace api.Controllers;

[Route("api/[controller]")]
[Authorization]
[ApiController]
public class FileController : Controller
{
    readonly string userStoragePath = $"{Directory.GetCurrentDirectory()}" + @"\UserStorage\";
    public string userUUID = "";

    [HttpGet("downloadFile/{fileName}")]
    public IActionResult DownloadFile(string fileName)
    {
        string fullPath = Path.Combine(userStoragePath, "download/" + userUUID + "/" + fileName);

        if (!System.IO.File.Exists(fullPath))
            return NotFound("File not found");

        var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);

        try
        {
            return File(stream, MediaTypeNames.Application.Octet, Path.GetFileName(fullPath));
        }
        catch (Exception ex)
        {
            stream.Close();
            return StatusCode(500, $"Internal server error: {ex}");
        }
    }
}