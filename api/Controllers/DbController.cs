using Microsoft.AspNetCore.Mvc;
using mongodbweb.Server.Filters;
using mongodbweb.Server.Helpers;
using mongodbweb.Server.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace mongodbweb.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorization]
    public class DbController : Controller
    {
        public readonly MongoDbOperations mongoDbOperations;

        public DbController(IHubContext<ProgressHub> hubContext)
        {
            mongoDbOperations = new MongoDbOperations(hubContext) { client = new MongoClient("mongodb://invalid:27017") };
        }

        [HttpGet("listDB")]
        public IActionResult ListDb()
        {
            var dbList = mongoDbOperations.ListAllDatabases();
            if (dbList == null)
                return BadRequest("Failed to retrieve database list.");

            var jsonList = dbList.Select(doc => doc.Elements.ToDictionary(element => element.Name, element => BsonTypeMapper.MapToDotNetValue(element.Value))).ToList();

            return Ok(new { databases = jsonList });
        }

        [HttpGet("listCollections/{dbName}")]
        public IActionResult ListCollections(string dbName)
        {
            if (string.IsNullOrEmpty(dbName))
                return BadRequest("Database name is required.");

            var collectionsList = mongoDbOperations.ListAllCollectionsFromDb(dbName);
            if (collectionsList == null)
                return BadRequest($"Failed to retrieve collections from database '{dbName}'.");

            var jsonList = collectionsList.Select(doc => doc.Elements.ToDictionary(element => element.Name, element => BsonTypeMapper.MapToDotNetValue(element.Value))).ToList();

            return Ok(new { collections = jsonList });
        }
        
        [HttpGet("numberOfCollections/{dbName}")]
        public IActionResult GetNumberOfCollections(string dbName)
        {
            if (string.IsNullOrEmpty(dbName))
                return BadRequest("Database name is required.");

            var collectionCount = mongoDbOperations.GetNumberOfCollections(dbName);
            if (collectionCount == -1)
                return Unauthorized("User is not authenticated or an error occurred.");

            return Ok(new { count = collectionCount });
        }
        
        [HttpGet("getCollection/{dbName}/{collectionName}")]
        public IActionResult GetCollection(string dbName, string collectionName)
        {
            if (string.IsNullOrEmpty(dbName))
                return BadRequest("Database name is required.");

            if (string.IsNullOrEmpty(collectionName))
                return BadRequest("Collection name is required.");

            var collectionData = mongoDbOperations.GetCollection(dbName, collectionName);
            if (collectionData == null)
                return NotFound($"Collection '{collectionName}' not found in database '{dbName}'.");

            return Ok(new { collection = collectionData });
        }

        [HttpGet("collectionAttributes/{dbName}/{collectionName}")]
        public IActionResult GetCollectionAttributes(string dbName, string collectionName)
        {
            if (string.IsNullOrEmpty(dbName))
                return BadRequest("Database name is required.");

            if (string.IsNullOrEmpty(collectionName))
                return BadRequest("Collection name is required.");

            var attributes = mongoDbOperations.GetCollectionAttributes(dbName, collectionName);
            if (!attributes.Any())
                return NotFound($"No attributes found for collection '{collectionName}' in database '{dbName}'.");

            return Ok(new { attributes });
        }

        [HttpPost("renameAttributes/{dbName}/{collectionName}")]
        public async Task<IActionResult> RenameAttributesInCollectionAsync(string dbName, string collectionName, [FromBody] Dictionary<string, string>? renameMap)
        {
            if (string.IsNullOrEmpty(dbName))
                return BadRequest("Database name is required.");

            if (string.IsNullOrEmpty(collectionName))
                return BadRequest("Collection name is required.");

            if (renameMap == null || renameMap.Count == 0)
                return BadRequest("Rename map is empty.");

            var result = await mongoDbOperations.RenameAttributeInCollectionAsync(dbName, collectionName, renameMap);
            if (!result)
                return BadRequest($"Failed to rename attributes in collection '{collectionName}' in database '{dbName}'.");

            return Ok("Attributes renamed successfully.");
        }
        
        [HttpGet("totalCount/{dbName}/{collectionName}")]
        public IActionResult GetTotalCount(string dbName, string collectionName, [FromQuery] string selectedKey, [FromQuery] string searchValue)
        {
            if (string.IsNullOrEmpty(dbName))
                return BadRequest("Database name is required.");

            if (string.IsNullOrEmpty(collectionName))
                return BadRequest("Collection name is required.");

            var count = mongoDbOperations.GetTotalCount(dbName, collectionName, selectedKey, searchValue);

            return Ok(new { totalCount = count });
        }

        [HttpGet("getPaginatedCollection/{dbName}/{collectionName}")]
        public IActionResult GetPaginatedCollection(string dbName, string collectionName, [FromQuery] int skip, [FromQuery] int limit, [FromQuery] string selectedKey, [FromQuery] string searchValue)
        {
            if (string.IsNullOrEmpty(dbName))
                return BadRequest("Database name is required.");

            if (string.IsNullOrEmpty(collectionName))
                return BadRequest("Collection name is required.");

            var collectionData = mongoDbOperations.GetCollection(dbName, collectionName, skip, limit, selectedKey, searchValue);
            if (!collectionData.Any())
                return NotFound($"No data found in collection '{collectionName}' in database '{dbName}'.");

            return Ok(new { data = collectionData });
        }
        
        [HttpGet("collectionCount/{dbName}/{collectionName}")]
        public IActionResult GetCollectionCount(string dbName, string collectionName, [FromQuery] string selectedKey, [FromQuery] string searchValue)
        {
            if (string.IsNullOrEmpty(dbName))
                return BadRequest("Database name is required.");

            if (string.IsNullOrEmpty(collectionName))
                return BadRequest("Collection name is required.");

            var count = mongoDbOperations.GetCollectionCount(dbName, collectionName, selectedKey, searchValue);

            return Ok(new { count });
        }
        
        [HttpPost("insertDocument/{dbName}/{collectionName}")]
        public async Task<IActionResult> InsertDocumentAsync(string dbName, string collectionName, [FromBody] dynamic document)
        {
            if (string.IsNullOrEmpty(dbName))
                return BadRequest("Database name is required.");

            if (string.IsNullOrEmpty(collectionName))
                return BadRequest("Collection name is required.");

            if (document == null)
                return BadRequest("Document is required.");

            bool result = await mongoDbOperations.InsertDocumentAsync(dbName, collectionName, document);

            if (!result)
                return BadRequest("Failed to insert document.");

            return Ok("Document inserted successfully.");
        }

        [HttpDelete("deleteAllDatabases")]
        public IActionResult DeleteAllDatabases()
        {
            var result = mongoDbOperations.DeleteAllDatabases();

            if (!result)
                return BadRequest("Failed to delete all databases.");

            return Ok("All databases deleted successfully.");
        }
        
        [HttpDelete("deleteDatabase/{dbName}")]
        public IActionResult DeleteDatabase(string dbName)
        {
            if (string.IsNullOrEmpty(dbName))
                return BadRequest("Database name is required.");

            var result = mongoDbOperations.DeleteDb(dbName);

            if (!result)
                return BadRequest($"Failed to delete database '{dbName}'.");

            return Ok($"Database '{dbName}' deleted successfully.");
        }
        
        [HttpPost("createCollection/{dbName}/{collectionName}")]
        public IActionResult CreateCollection(string dbName, string collectionName)
        {
            if (string.IsNullOrEmpty(dbName))
                return BadRequest("Database name is required.");

            if (string.IsNullOrEmpty(collectionName))
                return BadRequest("Collection name is required.");

            var result = mongoDbOperations.CreateCollection(dbName, collectionName);

            if (!result)
                return BadRequest($"Failed to create collection '{collectionName}' in database '{dbName}'.");

            return Ok($"Collection '{collectionName}' created successfully in database '{dbName}'.");
        }
        
        [HttpDelete("deleteCollection/{dbName}/{collectionName}")]
        public IActionResult DeleteCollection(string dbName, string collectionName)
        {
            if (string.IsNullOrEmpty(dbName))
                return BadRequest("Database name is required.");

            if (string.IsNullOrEmpty(collectionName))
                return BadRequest("Collection name is required.");

            var result = mongoDbOperations.DeleteCollection(dbName, collectionName);

            if (!result)
                return BadRequest($"Failed to delete collection '{collectionName}' in database '{dbName}'.");

            return Ok($"Collection '{collectionName}' deleted successfully from database '{dbName}'.");
        }
        
        [HttpGet("databaseStatistics/{dbName}")]
        public IActionResult GetDatabaseStatistics(string dbName)
        {
            if (string.IsNullOrEmpty(dbName))
                return BadRequest("Database name is required.");

            var stats = mongoDbOperations.GetDatabaseStatistics(dbName);
            if (stats == null)
                return NotFound($"Failed to fetch statistics for database '{dbName}'.");

            return Ok(stats.ToJson());
        }

        [HttpGet("collectionStatistics/{dbName}/{collectionName}")]
        public IActionResult GetCollectionStatistics(string dbName, string collectionName)
        {
            if (string.IsNullOrEmpty(dbName))
                return BadRequest("Database name is required.");

            if (string.IsNullOrEmpty(collectionName))
                return BadRequest("Collection name is required.");

            var stats = mongoDbOperations.GetCollectionStatistics(dbName, collectionName);
            if (stats == null)
                return NotFound($"Failed to fetch statistics for collection '{collectionName}' in database '{dbName}'.");

            return Ok(stats.ToJson());
        }

        [HttpGet("globalStatistics")]
        public IActionResult GetGlobalStatistics()
        {
            var stats = mongoDbOperations.GetGlobalStatistics();
            if (stats == null)
                return NotFound("Failed to fetch global statistics.");

            return Ok(stats.ToJson());
        }

        [HttpGet("checkDatabaseExistence/{dbName}")]
        public IActionResult CheckIfDatabaseExists(string dbName)
        {
            if (string.IsNullOrEmpty(dbName))
                return BadRequest("Database name is required.");

            var result = mongoDbOperations.CheckIfDbExist(dbName);

            return Ok(new { exists = result });
        }
        
        [HttpPost("uploadJson/{dbName}/{collectionName}")]
        public async Task<IActionResult> UploadJsonAsync(string dbName, string collectionName, [FromBody] JToken? json, [FromQuery] bool adaptOid)
        {
            if (string.IsNullOrEmpty(dbName))
                return BadRequest("Database name is required.");

            if (string.IsNullOrEmpty(collectionName))
                return BadRequest("Collection name is required.");

            if (json == null)
                return BadRequest("JSON content is required.");

            var result = await mongoDbOperations.UploadJsonAsync(dbName, collectionName, json, adaptOid);

            if (!result)
                return BadRequest($"Failed to upload JSON to collection '{collectionName}' in database '{dbName}'.");

            return Ok("JSON uploaded successfully.");
        }
        
        [HttpPost("executeQuery/{dbName}/{collectionName}")]
        public async Task<IActionResult> ExecuteMongoQuery(string dbName, string collectionName, [FromBody] string query)
        {
            if (string.IsNullOrEmpty(dbName))
                return BadRequest("Database name is required.");

            if (string.IsNullOrEmpty(collectionName))
                return BadRequest("Collection name is required.");

            if (string.IsNullOrEmpty(query))
                return BadRequest("Query is required.");

            var result = await mongoDbOperations.ExecuteMongoQuery(dbName, collectionName, query);

            if (result.StartsWith("Error:"))
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("updateMongoDb/{dbName}/{collectionName}/{id}")]
        public async Task<IActionResult> UpdateMongoDb(string dbName, string collectionName, string id, [FromBody] UpdateMongoDbObject data)
        {
            if (string.IsNullOrEmpty(dbName))
                return BadRequest("Database name is required.");

            if (string.IsNullOrEmpty(collectionName))
                return BadRequest("Collection name is required.");

            if (string.IsNullOrEmpty(id))
                return BadRequest("Document ID is required.");

            if (data.Differences == null || data.RenameMap == null)
                return BadRequest("Differences and rename map are required.");

            var result = await mongoDbOperations.UpdateMongoDb(dbName, collectionName, data.Differences, data.RenameMap, id);

            if (!result)
                return BadRequest($"Failed to update document with ID '{id}' in collection '{collectionName}' of database '{dbName}'.");

            return Ok($"Document with ID '{id}' updated successfully in collection '{collectionName}' of database '{dbName}'.");
        }

        [HttpGet("prepareDatabaseDownload/{dbName}/{downloadGuid}")]
        public async Task<IActionResult> PrepareDatabaseDownload(string dbName, Guid downloadGuid)
        {
            if (string.IsNullOrEmpty(dbName))
                return BadRequest("Database name is required.");

            if (downloadGuid == Guid.Empty)
                return BadRequest("Valid download GUID is required.");

            var fileName = $"db-{dbName}-{downloadGuid.ToString()[^4..]}.json";
            var filePath = $"UserStorage/download/{mongoDbOperations.uuid}/{fileName}";

            try
            {
                var directory = Path.GetDirectoryName(filePath);
                if (directory == null)
                    return StatusCode(500, "The path for the directory could not be determined.");

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                try
                {
                    using (var fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
                    {
                        fileStream.Close();
                    }

                    using (var streamWriter = new StreamWriter(filePath))
                    {
                        await mongoDbOperations.StreamAllCollectionExport(streamWriter, dbName, downloadGuid);
                    }

                    return Ok(fileName);
                }
                catch (IOException)
                {
                    return StatusCode(500, "The file is currently in use. Please try again later.");
                }
            }
            catch (Exception)
            {
                return StatusCode(500, "Failed to initiate database download.");
            }
        }



    }
}
