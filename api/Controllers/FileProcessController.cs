using api.Filters;
using api.Helpers;
using api.Hubs;
using api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [Authorization]
    [ApiController]
    public class FileProcessController : Controller
    {
        readonly string userStoragePath = $"{Directory.GetCurrentDirectory()}" + @"\UserStorage\";
        public string uuid = "";
        public readonly MongoDbOperations mongoDbOperations;
        public readonly IHubContext<ProgressHub>? _hubContext;

        public FileProcessController(IHubContext<ProgressHub> hubContext)
        {
            mongoDbOperations = new MongoDbOperations(hubContext) { client = new MongoClient("mongodb://invalid:27017") };
            _hubContext = hubContext;
        }

        [HttpGet("processImportAsync/{fileName}/{processGuid}")]
        public async Task<IActionResult> ProcessImportAsync(string fileName, Guid processGuid)
        {
            string toImportFilePath = Path.Combine(userStoragePath, "uploads/" + uuid + "/" + fileName);

            if (!System.IO.File.Exists(toImportFilePath))
                return NotFound("File not found");

            List<string> collectionsNames = [];
            string dbName = "";
            bool isRoot = true;

            long totalBytes = new FileInfo(toImportFilePath).Length;
            long bytesRead = 0;
            long lastReportedBytesRead = 0;

            using (var fileStream = new FileStream(toImportFilePath, FileMode.Open, FileAccess.Read))
            using (var streamReader = new StreamReader(fileStream))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                var jsonSerializer = new JsonSerializer();
                while (await jsonReader.ReadAsync())
                {
                    bytesRead = fileStream.Position;
                    double progress = (double)bytesRead / totalBytes * 100;
                    if (Math.Abs(bytesRead - lastReportedBytesRead) >= (totalBytes * 0.01))
                    {
                        double totalMB = totalBytes / (1024.0 * 1024.0);
                        double bytesReadMB = bytesRead / (1024.0 * 1024.0);

                        await _hubContext!.Clients.All.SendAsync("ReceiveProgress", totalMB, bytesReadMB, progress, processGuid.ToString(), "MB", "process");

                        lastReportedBytesRead = bytesRead;
                    }

                    if (jsonReader.TokenType == JsonToken.PropertyName)
                    {
                        var propertyName = jsonReader.Value?.ToString();
                        if (!string.IsNullOrEmpty(propertyName))
                        {
                            if (isRoot)
                            {
                                dbName = propertyName;
                                isRoot = false;
                            }
                            else if (await jsonReader.ReadAsync() && jsonReader.TokenType == JsonToken.StartArray)
                            {
                                collectionsNames.Add(propertyName);
                            }
                        }
                    }
                }
            }

            return Json(new { databaseName = dbName, collectionsNames });
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportAsync([FromBody] ImportRequestObject request)
        {
            string toImportFilePath = Path.Combine(userStoragePath, "uploads/" + uuid + "/" + request.FileName);

            if (!System.IO.File.Exists(toImportFilePath))
                return NotFound("File not found");

            try
            {

                long totalBytes = new FileInfo(toImportFilePath).Length;
                long bytesRead = 0;
                long lastReportedBytesRead = 0;

                using var fileStream = new FileStream(toImportFilePath, FileMode.Open, FileAccess.Read);
                using var streamReader = new StreamReader(fileStream);
                using var jsonReader = new JsonTextReader(streamReader);
                var jsonSerializer = new JsonSerializer();

                while (await jsonReader.ReadAsync())
                {
                    bytesRead = fileStream.Position;
                    double progress = (double)bytesRead / totalBytes * 100;
                    double bytes = ((double)bytesRead) / (1024.0 * 1024.0);
                    double total = ((double)totalBytes) / (1024.0 * 1024.0);

                    if (Math.Abs(bytesRead - lastReportedBytesRead) >= (totalBytes * 0.01))
                    {

                        await _hubContext!.Clients.All.SendAsync("ReceiveProgress", total, bytes, progress, request.Guid, "MB", "import");
                        lastReportedBytesRead = bytesRead;
                    }

                    if (jsonReader.TokenType == JsonToken.PropertyName)
                    {
                        var propertyName = jsonReader.Value?.ToString();
                        if (!string.IsNullOrEmpty(propertyName))
                        {
                            if (await jsonReader.ReadAsync() && jsonReader.TokenType == JsonToken.StartArray)
                            {
                                if (request.CheckedCollectionNames.Contains(propertyName))
                                {
                                    string collectionNameToImport = request.CollectionNameChanges.ContainsKey(propertyName) ? request.CollectionNameChanges[propertyName] : propertyName;

                                    while (await jsonReader.ReadAsync() && jsonReader.TokenType != JsonToken.EndArray)
                                    {
                                        if (jsonReader.TokenType == JsonToken.StartObject)
                                        {
                                            JObject document = await JObject.LoadAsync(jsonReader);
                                            await mongoDbOperations.UploadJSONAsync(request.DbName, collectionNameToImport, document, request.AdoptOid);
                                        }
                                    }
                                }

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }
    }
}