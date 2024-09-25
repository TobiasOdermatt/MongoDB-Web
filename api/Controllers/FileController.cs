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
    public string uuid = "";

    [HttpGet("downloadFile/{fileName}")]
    public IActionResult DownloadFile(string fileName)
    {
        string fullPath = Path.Combine(userStoragePath, "downloads/" + uuid + "/" + fileName);

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

    [HttpPost("uploadFile")]
    public IActionResult UploadFile(IFormFile file)
    {

        if (!int.TryParse(HttpContext.Request.Form["chunkIndex"], out int chunkIndex))
            chunkIndex = 0;

        if (!int.TryParse(HttpContext.Request.Form["totalChunks"], out int totalChunks))
            totalChunks = 0;

        if (file == null || file.Length == 0)
            return BadRequest("Invalid file");

        string userUploadPath = Path.Combine(userStoragePath, "uploads/" + uuid + "/");

        if (!Directory.Exists(userUploadPath))
            Directory.CreateDirectory(userUploadPath);

        string fullPath = Path.Combine(userUploadPath, file.FileName);

        if (!System.IO.File.Exists(fullPath))
            System.IO.File.Create(fullPath).Close();

        FileMode fileMode = (chunkIndex == 0) ? FileMode.Create : FileMode.Append;

        try
        {
            using (FileStream stream = new(fullPath, fileMode))
                file.CopyTo(stream);

            if (chunkIndex == totalChunks - 1)
                return Ok();

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex}");
        }
    }
}