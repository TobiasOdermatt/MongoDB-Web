using System.Collections;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using mongodbweb.Server.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

namespace mongodbweb.Server.Tests.Controllers
{
    [TestFixture]
    public class DbControllerTests
    {
        private readonly DbController _dbController = new();
        private HttpContext _validHttpContext = new DefaultHttpContext();

        [SetUp]
        public async Task Setup()
        {
            TestSetup.ConfigureDbConnector();
            _validHttpContext = TestSetup.GetValidHttpContext();
            var isAuthorized = await CheckAuthorization();
            Assert.IsTrue(isAuthorized);
            _dbController.mongoDbOperations.client = new MongoClient(TestSetup.GetConnectionString());
        }

        [Test]
        public void ListDb_ReturnsOk_WithDbList()
        {
            TestSetup.GenerateTestDb();
            var result = _dbController.ListDb() as ObjectResult;
            TestSetup.DropTestDatabase();
            Assert.IsNotNull(result);
            if (result == null) return;
            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);

            var resultValue = result.Value;
            Assert.IsNotNull(resultValue, "Result value should not be null");

            var databasesProperty = resultValue?.GetType().GetProperty("databases");
            Assert.IsNotNull(databasesProperty, "The 'databases' property should exist in the result value.");

            var databases = databasesProperty?.GetValue(resultValue) as IEnumerable;
            Assert.IsNotNull(databases, "Databases should not be null.");
            if (databases == null) return;
            var enumerable = databases as object[] ?? databases.Cast<object>().ToArray();
            Assert.IsTrue(databases != null && enumerable.Any(), "Databases should contain elements.");
            var containsUnitTestDb = enumerable.Cast<Dictionary<string, object>>()
                .Any(db => db.ContainsKey("name") && db["name"].ToString() == "UnitTestDb");
            Assert.IsTrue(containsUnitTestDb, "Databases should contain a database named 'UnitTestDb'.");
        }


        [Test]
        public void ListCollections_ReturnsBadRequest_WhenDbNameIsEmpty()
        {
            var result = _dbController.ListCollections(string.Empty) as ObjectResult;

            Assert.IsNotNull(result);
            if (result == null) return;
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode, "Status code should be 400 OK");
            Assert.AreEqual("Database name is required.", result.Value);
        }

        [Test]
        public void ListCollections_ReturnsOk_WithCollectionList()
        {
            TestSetup.GenerateTestDb();
            var result = _dbController.ListCollections("UnitTestDb") as ObjectResult;
            TestSetup.DropTestDatabase();
            Assert.IsNotNull(result);
            if (result == null) return;
            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);

            var resultValue = result.Value;
            Assert.IsNotNull(resultValue, "Result value should not be null");

            var collectionsProperty = resultValue?.GetType().GetProperty("collections");
            Assert.IsNotNull(collectionsProperty, "The 'collections' property should exist in the result value.");

            var collections = collectionsProperty?.GetValue(resultValue) as IEnumerable;
            Assert.IsNotNull(collections, "Collections should not be null.");
            Assert.IsTrue(collections != null && collections.Cast<object>().Any(),
                "Collections should contain elements.");
        }

        [Test]
        public void GetNumberOfCollections_ReturnsOk_WithCollectionCount()
        {
            TestSetup.GenerateTestDb();
            var result = _dbController.GetNumberOfCollections("UnitTestDb") as ObjectResult;
            TestSetup.DropTestDatabase();

            Assert.IsNotNull(result);
            if (result == null) return;
            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
            var resultValue = result.Value;
            Assert.IsNotNull(resultValue);
            var countProperty = resultValue?.GetType().GetProperty("count");
            Assert.IsNotNull(countProperty, "The 'count' property should exist in the result value.");

            var collectionCount = (int)(countProperty?.GetValue(resultValue) ?? 0);
            Assert.IsTrue(collectionCount >= 1, "Collection count should be greater than 1.");
        }
        
        [Test]
        public void GetNumberOfCollections_ReturnsBadRequest_WithoutDatabaseName()
        {
            var result = _dbController.GetNumberOfCollections("") as ObjectResult;
            Assert.IsNotNull(result);
            if (result == null) return;
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
        }


        [Test]
        public void GetCollection_ReturnsOk_WithCollection()
        {
            TestSetup.GenerateTestDb();
            var result = _dbController.GetCollection("UnitTestDb", "collection") as ObjectResult;
            TestSetup.DropTestDatabase();

            Assert.IsNotNull(result);
            if (result == null) return;
            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
            var resultValue = result.Value;
            Assert.IsNotNull(resultValue);
            var collectionProperty = resultValue?.GetType().GetProperty("collection");
            Assert.IsNotNull(collectionProperty, "The 'collection' property should exist in the result value.");

            var collection = collectionProperty?.GetValue(resultValue) as IEnumerable;
            Assert.IsNotNull(collection, "Collection should not be null.");
            Assert.IsTrue(collection != null && collection.Cast<object>().Any(), "Collection should contain elements.");
        }

        [Test]
        public void GetCollection_ReturnsBadRequest_WhenDbNameIsEmpty()
        {
            var result = _dbController.GetCollection(string.Empty, "collection") as ObjectResult;

            Assert.IsNotNull(result);
            if (result == null) return;
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode, "Status code should be 400 OK");
            Assert.AreEqual("Database name is required.", result.Value);
        }

        [Test]
        public void GetCollection_ReturnsBadRequest_WhenCollectionNameIsEmpty()
        {
            var result = _dbController.GetCollection("UnitTestDb", string.Empty) as ObjectResult;

            Assert.IsNotNull(result);
            if (result == null) return;
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode, "Status code should be 400 OK");
            Assert.AreEqual("Collection name is required.", result.Value);
        }

        [Test]
        public void GetCollectionAttributes_ReturnsOk_WithCollectionAttributes()
        {
            TestSetup.GenerateTestDb();
            var result = _dbController.GetCollectionAttributes("UnitTestDb", "collection") as ObjectResult;
            TestSetup.DropTestDatabase();

            Assert.IsNotNull(result);
            if (result == null) return;
            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
            var resultValue = result.Value;
            Assert.IsNotNull(resultValue);
            var attributesProperty = resultValue?.GetType().GetProperty("attributes");
            Assert.IsNotNull(attributesProperty, "The 'collection' property should exist in the result value.");

            var attributes = attributesProperty?.GetValue(resultValue) as IEnumerable;
            Assert.IsNotNull(attributes, "Collection should not be null.");
            if (attributes == null) return;
            var enumerable = attributes.Cast<object>().ToList();
            Assert.IsTrue(attributes != null && enumerable.Any(), "Collection should contain elements.");

            var attributeList = enumerable.Cast<string>().ToList();
            Assert.IsTrue(attributeList.Contains("_id"), "Attribute list should contain '_id'");
            Assert.IsTrue(attributeList.Contains("name"), "Attribute list should contain 'name'");
            Assert.IsTrue(attributeList.Contains("email"), "Attribute list should contain 'email'");
        }

        [Test]
        public async Task RenameAttributesInCollectionAsync_ReturnsOk_WhenRenamedSuccessfully()
        {
            TestSetup.GenerateTestDb();
            var renameMap = new Dictionary<string, string>
            {
                { "name", "newName" }
            };
            var result =
                await _dbController.RenameAttributesInCollectionAsync("UnitTestDb", "collection", renameMap) as
                    ObjectResult;

            Assert.IsNotNull(result);
            if (result == null)
                return;

            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
            var resultValue = result.Value as string;
            Assert.AreEqual("Attributes renamed successfully.", resultValue);

            var updatedAttributes = _dbController.mongoDbOperations.GetCollectionAttributes("UnitTestDb", "collection");
            TestSetup.DropTestDatabase();
            Assert.IsTrue(updatedAttributes.Contains("newName"), "Attribute 'newName' should exist after renaming.");
            Assert.IsFalse(updatedAttributes.Contains("name"), "Attribute 'name' should not exist after renaming.");
        }


        [Test]
        public async Task RenameAttributesInCollectionAsync_ReturnsBadRequest_WhenDbNameIsEmpty()
        {
            var result =
                await _dbController.RenameAttributesInCollectionAsync("", "collection",
                    new Dictionary<string, string>()) as ObjectResult;
            Assert.IsNotNull(result);
            if (result != null) Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
        }

        [Test]
        public async Task RenameAttributesInCollectionAsync_ReturnsBadRequest_WhenCollectionNameIsEmpty()
        {
            var result =
                await _dbController.RenameAttributesInCollectionAsync("UnitTestDb", "",
                    new Dictionary<string, string>()) as ObjectResult;
            Assert.IsNotNull(result);
            if (result != null) Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
        }

        [Test]
        public async Task RenameAttributesInCollectionAsync_ReturnsBadRequest_WhenRenameMapIsEmpty()
        {
            var result =
                await _dbController.RenameAttributesInCollectionAsync("UnitTestDb", "collection",
                    new Dictionary<string, string>()) as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result?.StatusCode);
        }

        [Test]
        public void GetCollectionAttributes_ReturnsBadRequest_WhenDbNameIsEmpty()
        {
            var result = _dbController.GetCollectionAttributes(string.Empty, "collection") as ObjectResult;

            Assert.IsNotNull(result);
            if (result == null) return;
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode, "Status code should be 400 OK");
            Assert.AreEqual("Database name is required.", result.Value);
        }

        [Test]
        public void GetCollectionAttributes_ReturnsBadRequest_WhenCollectionNameIsEmpty()
        {
            var result = _dbController.GetCollectionAttributes("UnitTestDb", string.Empty) as ObjectResult;

            Assert.IsNotNull(result);
            if (result == null) return;
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode, "Status code should be 400 OK");
            Assert.AreEqual("Collection name is required.", result.Value);
        }

        [Test]
        public void GetTotalCount_ReturnsOk_WithTotalCountWithEmptyKeyAndValue()
        {
            TestSetup.GenerateTestDb();
            var result = _dbController.GetTotalCount("UnitTestDb", "collection", "", "") as ObjectResult;
            TestSetup.DropTestDatabase();

            Assert.IsNotNull(result);
            if (result == null) return;
            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
            var resultValue = result.Value;
            Assert.IsNotNull(resultValue);
            var totalCountProperty = resultValue?.GetType().GetProperty("totalCount");
            Assert.IsNotNull(totalCountProperty, "The 'totalCount' property should exist in the result value.");

            var collectionCount = (long)(totalCountProperty?.GetValue(resultValue) ?? 0);
            Assert.IsTrue(collectionCount >= 1, "totalCount should be greater than 1.");
        }

        [Test]
        public void GetTotalCount_ReturnsOk_WithTotalCountWithKeyAndValue()
        {
            TestSetup.GenerateTestDb();
            var result = _dbController.GetTotalCount("UnitTestDb", "collection", "email", "test") as ObjectResult;
            TestSetup.DropTestDatabase();

            Assert.IsNotNull(result);
            if (result == null) return;
            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
            var resultValue = result.Value;
            Assert.IsNotNull(resultValue);
            var totalCountProperty = resultValue?.GetType().GetProperty("totalCount");
            Assert.IsNotNull(totalCountProperty, "The 'totalCount' property should exist in the result value.");

            var collectionCount = (long)(totalCountProperty?.GetValue(resultValue) ?? 0);
            Assert.IsTrue(collectionCount >= 1, "totalCount should be greater than 1.");
        }

        [Test]
        public void GetTotalCount_ReturnsOk_WithTotalCountWithoutKeyButValue()
        {
            TestSetup.GenerateTestDb();
            var result = _dbController.GetTotalCount("UnitTestDb", "collection", "", "test") as ObjectResult;
            TestSetup.DropTestDatabase();

            Assert.IsNotNull(result);
            if (result == null) return;
            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
            var resultValue = result.Value;
            Assert.IsNotNull(resultValue);
            var totalCountProperty = resultValue?.GetType().GetProperty("totalCount");
            Assert.IsNotNull(totalCountProperty, "The 'totalCount' property should exist in the result value.");

            var collectionCount = (long)(totalCountProperty?.GetValue(resultValue) ?? 0);
            Assert.IsTrue(collectionCount >= 1, "totalCount should be greater than 1.");
        }

        [Test]
        public void GetTotalCount_ReturnsBadRequest_WhenDbNameIsEmpty()
        {
            var result = _dbController.GetTotalCount("", "collection", "key", "value") as ObjectResult;
            Assert.IsNotNull(result);
            if (result != null) Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
        }

        [Test]
        public void GetTotalCount_ReturnsBadRequest_WhenCollectionNameIsEmpty()
        {
            var result = _dbController.GetTotalCount("UnitTestDb", "", "key", "value") as ObjectResult;
            Assert.IsNotNull(result);
            if (result != null) Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
        }

        [Test]
        public void GetPaginatedCollection_ReturnsBadRequest_WhenDbNameIsEmpty()
        {
            var result =
                _dbController.GetPaginatedCollection("", "collectionName", 0, 10, "key", "value") as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result?.StatusCode);
            Assert.AreEqual("Database name is required.", result?.Value);
        }

        [Test]
        public void GetPaginatedCollection_ReturnsBadRequest_WhenCollectionNameIsEmpty()
        {
            var result = _dbController.GetPaginatedCollection("UnitTestDb", "", 0, 10, "key", "value") as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result?.StatusCode);
            Assert.AreEqual("Collection name is required.", result?.Value);
        }

        [Test]
        public void GetPaginatedCollection_ReturnsNotFound_WhenNoData()
        {
            var result =
                _dbController.GetPaginatedCollection("dbName", "collectionName", 0, 10, "key", "value") as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status404NotFound, result?.StatusCode);
            Assert.AreEqual($"No data found in collection 'collectionName' in database 'dbName'.", result?.Value);
        }

        [Test]
        public void GetPaginatedCollection_ReturnsOk_WithKeyAndValue()
        {
            TestSetup.GenerateTestDb();
            var result =
                _dbController.GetPaginatedCollection("UnitTestDb", "collection", 0, 10, "email",
                    "test") as ObjectResult;
            TestSetup.DropTestDatabase();
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status200OK, result?.StatusCode);
            var resultValue = result?.Value as dynamic;
            Assert.IsNotNull(resultValue);
        }

        [Test]
        public void GetPaginatedCollection_ReturnsOk_WithoutKeyButValue()
        {
            TestSetup.GenerateTestDb();
            var result =
                _dbController.GetPaginatedCollection("UnitTestDb", "collection", 0, 10, "", "test") as ObjectResult;
            TestSetup.DropTestDatabase();
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status200OK, result?.StatusCode);
            var resultValue = result?.Value as dynamic;
            Assert.IsNotNull(resultValue);
        }

        [Test]
        public void GetCollectionCount_ReturnsBadRequest_WhenDbNameIsEmpty()
        {
            var result = _dbController.GetCollectionCount("", "collectionName", "key", "value") as ObjectResult;
            Assert.IsNotNull(result);
            if (result == null) return;
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual("Database name is required.", result.Value);
        }

        [Test]
        public void GetCollectionCount_ReturnsBadRequest_WhenCollectionNameIsEmpty()
        {
            var result = _dbController.GetCollectionCount("UnitTestDb", "", "key", "value") as ObjectResult;
            Assert.IsNotNull(result);
            if (result == null) return;
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual("Collection name is required.", result.Value);
        }

        [Test]
        public void GetCollectionCount_ReturnsOk_WithCount()
        {
            TestSetup.GenerateTestDb();
            var result = _dbController.GetCollectionCount("UnitTestDb", "collection", "email", "test") as ObjectResult;
            TestSetup.DropTestDatabase();
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status200OK, result?.StatusCode);
            var resultValue = result?.Value as dynamic;
            Assert.IsNotNull(resultValue);
            var totalCountProperty = resultValue?.GetType().GetProperty("count");
            Assert.IsNotNull(totalCountProperty, "The 'count' property should exist in the result value.");

            var collectionCount = (long)(totalCountProperty?.GetValue(resultValue) ?? 0);
            Assert.IsTrue(collectionCount == 1, "documents count should be 1.");
        }

        [Test]
        public async Task InsertDocumentAsync_InsertsDocumentSuccessfully()
        {
            dynamic document = new { Name = "Test", Value = "123" };
            var result =
                await _dbController.InsertDocumentAsync("UnitTestDb", "newCollection", document) as ObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status200OK, result?.StatusCode);
            Assert.AreEqual("Document inserted successfully.", result?.Value);

            var collection = _dbController.mongoDbOperations.client.GetDatabase("UnitTestDb")
                .GetCollection<BsonDocument>("newCollection");
            var insertedDocument = collection.Find(new BsonDocument("Name", "Test")).FirstOrDefault();
            TestSetup.DropTestDatabase();
            Assert.IsNotNull(insertedDocument);
            Assert.AreEqual("123", insertedDocument["Value"].AsString);
        }


        [Test]
        public async Task InsertDocumentAsync_ReturnsBadRequest_WhenDbNameIsEmpty()
        {
            dynamic document = new { Name = "Test", Value = "123" };

            var result = await _dbController.InsertDocumentAsync("", "test", document) as ObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result?.StatusCode);
            Assert.AreEqual("Database name is required.", result?.Value);
        }

        [Test]
        public async Task InsertDocumentAsync_ReturnsBadRequest_WhenCollectionNameIsEmpty()
        {
            dynamic document = new { Name = "Test", Value = "123" };

            var result = await _dbController.InsertDocumentAsync("test", "", document) as ObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result?.StatusCode);
            Assert.AreEqual("Collection name is required.", result?.Value);
        }

        [Test]
        public async Task InsertDocumentAsync_ReturnsBadRequest_WhenDocumentIsNull()
        {
            dynamic? document = null;
            var result = await _dbController.InsertDocumentAsync("test", "test", document) as ObjectResult;

            Assert.IsNotNull(result, "Result should not be null.");
            Assert.IsInstanceOf<ObjectResult>(result, "Result should be an instance of ObjectResult.");
            Assert.AreEqual(StatusCodes.Status400BadRequest, result?.StatusCode,
                "Status code should be 400 BadRequest.");
            Assert.AreEqual("Document is required.", result?.Value,
                "The error message should indicate that the document is required.");
        }

        [Test]
        public void DeleteDatabase_ReturnsBadRequest_WhenDbNameIsEmpty()
        {
            var result = _dbController.DeleteDatabase(string.Empty) as ObjectResult;

            Assert.IsNotNull(result);
            if (result == null) return;
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual("Database name is required.", result.Value);
        }

        [Test]
        public void DeleteDatabase_ReturnsOk_WhenDeletionSucceeds()
        {
            TestSetup.GenerateTestDb();
            var result = _dbController.DeleteDatabase("UnitTestDb") as ObjectResult;
            TestSetup.DropTestDatabase();

            Assert.IsNotNull(result);
            if (result == null) return;
            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
            Assert.AreEqual($"Database 'UnitTestDb' deleted successfully.", result.Value);
        }

        [Test]
        public void CreateCollection_ReturnsBadRequest_WhenDbNameIsEmpty()
        {
            var result = _dbController.CreateCollection(string.Empty, "TestCollection") as ObjectResult;

            Assert.IsNotNull(result);
            if (result == null) return;
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual("Database name is required.", result.Value);
        }

        [Test]
        public void CreateCollection_ReturnsBadRequest_WhenCollectionNameIsEmpty()
        {
            var result = _dbController.CreateCollection("TestDb", string.Empty) as ObjectResult;

            Assert.IsNotNull(result);
            if (result == null) return;
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual("Collection name is required.", result.Value);
        }

        [Test]
        public void CreateCollection_ReturnsOk_WhenCreationSucceeds()
        {
            TestSetup.DropTestDatabase();
            var result = _dbController.CreateCollection("UnitTestDb", "TestCollection") as ObjectResult;
            TestSetup.DropTestDatabase();
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status200OK, result?.StatusCode);
            Assert.AreEqual("Collection 'TestCollection' created successfully in database 'UnitTestDb'.",
                result?.Value);
        }

        [Test]
        public void DeleteCollection_ReturnsBadRequest_WhenDbNameIsEmpty()
        {
            var result = _dbController.DeleteCollection(string.Empty, "TestCollection") as ObjectResult;

            Assert.IsNotNull(result);
            if (result == null) return;
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual("Database name is required.", result.Value);
        }

        [Test]
        public void DeleteCollection_ReturnsBadRequest_WhenCollectionNameIsEmpty()
        {
            var result = _dbController.DeleteCollection("TestDb", string.Empty) as ObjectResult;

            Assert.IsNotNull(result);
            if (result == null) return;
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual("Collection name is required.", result.Value);
        }

        [Test]
        public void DeleteCollection_ReturnsOk_WhenDeletionSucceeds()
        {
            var result = _dbController.DeleteCollection("TestDb", "TestCollection") as ObjectResult;

            Assert.IsNotNull(result);
            if (result == null) return;
            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
            Assert.AreEqual("Collection 'TestCollection' deleted successfully from database 'TestDb'.",
                result.Value);
        }

        [Test]
        public void GetDatabaseStatistics_ReturnsBadRequest_WhenDbNameIsEmpty()
        {
            var result = _dbController.GetDatabaseStatistics(string.Empty) as ObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result?.StatusCode);
            Assert.AreEqual("Database name is required.", result?.Value);
        }

        [Test]
        public void GetDatabaseStatistics_ReturnsOk_WhenStatsFound()
        {
            var result = _dbController.GetDatabaseStatistics("TestDb") as ObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status200OK, result?.StatusCode);
        }

        [Test]
        public void GetCollectionStatistics_ReturnsBadRequest_WhenDbNameIsEmpty()
        {
            var result = _dbController.GetCollectionStatistics(string.Empty, "TestCollection") as ObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result?.StatusCode);
            Assert.AreEqual("Database name is required.", result?.Value);
        }

        [Test]
        public void GetCollectionStatistics_ReturnsBadRequest_WhenCollectionNameIsEmpty()
        {
            var result = _dbController.GetCollectionStatistics("TestDb", string.Empty) as ObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result?.StatusCode);
            Assert.AreEqual("Collection name is required.", result?.Value);
        }

        [Test]
        public void GetCollectionStatistics_ReturnsOk_WhenStatsFound()
        {
            var result = _dbController.GetCollectionStatistics("TestDb", "TestCollection") as ObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status200OK, result?.StatusCode);
        }

        [Test]
        public void GetGlobalStatistics_ReturnsOk_WhenStatsFound()
        {
            var result = _dbController.GetGlobalStatistics() as ObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status200OK, result?.StatusCode);
        }

        [Test]
        public void CheckIfDatabaseExists_ReturnsBadRequest_WhenDbNameIsEmpty()
        {
            var result = _dbController.CheckIfDatabaseExists(string.Empty) as ObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result?.StatusCode);
            Assert.AreEqual("Database name is required.", result?.Value);
        }

        [Test]
        public void CheckIfDatabaseExists_ReturnsFalse_WhenDbDoesNotExist()
        {
            var result = _dbController.CheckIfDatabaseExists("NonExistentDb") as ObjectResult;

            Assert.IsNotNull(result);
            if (result == null) return;
            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);

            var resultValue = result.Value;
            Assert.IsNotNull(resultValue, "Result value should not be null");

            var existsProperty = resultValue?.GetType().GetProperty("exists");
            Assert.IsNotNull(existsProperty, "The 'exists' property should exist in the result value.");

            var exists = (bool)(existsProperty?.GetValue(resultValue) ?? false);
            Assert.IsFalse(exists, "The database should not exist.");
        }

        [Test]
        public void CheckIfDatabaseExists_ReturnsTrue_WhenDbExists()
        {
            TestSetup.GenerateTestDb();
            var result = _dbController.CheckIfDatabaseExists("UnitTestDb") as ObjectResult;
            TestSetup.DropTestDatabase();
            Assert.IsNotNull(result);
            if (result == null) return;
            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
            if (result.Value == null) return;
            var resultValue = result.Value;
            Assert.IsNotNull(resultValue, "Result value should not be null");

            var existsProperty = resultValue?.GetType().GetProperty("exists");
            Assert.IsNotNull(existsProperty, "The 'exists' property should exist in the result value.");

            var exists = (bool)(existsProperty?.GetValue(resultValue) ?? false);
            Assert.IsTrue(exists, "The database should  exist.");
        }

        [Test]
        public async Task UploadJsonAsync_ReturnsBadRequest_WhenDbNameIsEmpty()
        {
            var result =
                await _dbController.UploadJsonAsync(string.Empty, "TestCollection", new JObject(), false) as
                    ObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result?.StatusCode);
            Assert.AreEqual("Database name is required.", result?.Value);
        }

        [Test]
        public async Task UploadJsonAsync_ReturnsBadRequest_WhenCollectionNameIsEmpty()
        {
            var result =
                await _dbController.UploadJsonAsync("TestDb", string.Empty, new JObject(), false) as ObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result?.StatusCode);
            Assert.AreEqual("Collection name is required.", result?.Value);
        }

        [Test]
        public async Task UploadJsonAsync_ReturnsBadRequest_WhenJsonIsNull()
        {
            var result = await _dbController.UploadJsonAsync("TestDb", "TestCollection", null, false) as ObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result?.StatusCode);
            Assert.AreEqual("JSON content is required.", result?.Value);
        }

        [Test]
        public async Task UploadJsonAsync_ReturnsOk_WhenUploadSucceeds()
        {
            var result =
                await _dbController.UploadJsonAsync("TestDb", "TestCollection", new JObject(), false) as ObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status200OK, result?.StatusCode);
            Assert.AreEqual("JSON uploaded successfully.", result?.Value);
        }

        [Test]
        public async Task ExecuteMongoQuery_ReturnsBadRequest_WhenDbNameIsEmpty()
        {
            var result = await _dbController.ExecuteMongoQuery(string.Empty, "TestCollection", "query") as ObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result?.StatusCode);
            Assert.AreEqual("Database name is required.", result?.Value);
        }

        [Test]
        public async Task ExecuteMongoQuery_ReturnsBadRequest_WhenCollectionNameIsEmpty()
        {
            var result = await _dbController.ExecuteMongoQuery("TestDb", string.Empty, "query") as ObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result?.StatusCode);
            Assert.AreEqual("Collection name is required.", result?.Value);
        }

        [Test]
        public async Task ExecuteMongoQuery_ReturnsBadRequest_WhenQueryIsEmpty()
        {
            var result =
                await _dbController.ExecuteMongoQuery("TestDb", "TestCollection", string.Empty) as ObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result?.StatusCode);
            Assert.AreEqual("Query is required.", result?.Value);
        }

        [Test]
        public async Task ExecuteMongoQuery_ReturnsOk_WhenQueryExecutesSuccessfully()
        {
            TestSetup.GenerateTestDb();
            var result =
                await _dbController.ExecuteMongoQuery("UnitTestDb", "collection", "{ \"age\": { \"$gt\": 25 } }") as
                    ObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status200OK, result?.StatusCode);
        }

        [Test]
        public async Task UpdateMongoDb_ReturnsBadRequest_WhenDbNameIsEmpty()
        {
            var result =
                await _dbController.UpdateMongoDb(string.Empty, "TestCollection", "1", CreateValidUpdateObject()) as
                    ObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result?.StatusCode);
            Assert.AreEqual("Database name is required.", result?.Value);
        }

        [Test]
        public async Task UpdateMongoDb_ReturnsBadRequest_WhenCollectionNameIsEmpty()
        {
            var result =
                await _dbController.UpdateMongoDb("TestDb", string.Empty, "1", CreateValidUpdateObject()) as
                    ObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result?.StatusCode);
            Assert.AreEqual("Collection name is required.", result?.Value);
        }

        [Test]
        public async Task UpdateMongoDb_ReturnsBadRequest_WhenIdIsEmpty()
        {
            var result =
                await _dbController.UpdateMongoDb("TestDb", "TestCollection", string.Empty, CreateValidUpdateObject())
                    as ObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result?.StatusCode);
            Assert.AreEqual("Document ID is required.", result?.Value);
        }

        [Test]
        public async Task UpdateMongoDb_ReturnsBadRequest_WhenUpdateObjectIsInvalid()
        {
            var invalidUpdateObject = new UpdateMongoDbObject();

            var result =
                await _dbController.UpdateMongoDb("TestDb", "TestCollection", "1", invalidUpdateObject) as ObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result?.StatusCode);
            Assert.AreEqual("Differences and rename map are required.", result?.Value);
        }

        [Test]
        public async Task UpdateMongoDb_ReturnsOk_WhenUpdateSucceeds()
        {
            TestSetup.GenerateTestDb();
            var documentId = await GetDocumentIdFromCollection();
            if (documentId != null)
            {
                var updateObject = CreateValidUpdateObject();
                var result = await _dbController.UpdateMongoDb("UnitTestDb", "collection", documentId, updateObject) as ObjectResult;

                Assert.IsNotNull(result);
                if (result != null)
                {
                    Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
                    Assert.IsTrue(result.Value != null && result.Value.ToString()!.Contains("updated successfully"));
                    
                    var updatedDocument = await GetDocumentById("UnitTestDb", "collection", documentId);
                    Assert.IsNotNull(updatedDocument);
                    Assert.AreEqual("John Doe", updatedDocument?["nameChanged"].AsString);
                }
            }
        }
        
        private async Task<bool> CheckAuthorization()
        {
            _dbController.ControllerContext = new ControllerContext()
            {
                HttpContext = _validHttpContext
            };
            var authorizationFilter = new mongodbweb.Server.Filters.Authorization();

            var actionContext = new ActionContext(_validHttpContext, new RouteData(), new ActionDescriptor());
            var actionExecutingContext = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(),
                new Dictionary<string, object>()!, _dbController);

            async Task<ActionExecutedContext> Next()
            {
                return await Task.FromResult(new ActionExecutedContext(actionContext, new List<IFilterMetadata>(),
                    _dbController));
            }

            await authorizationFilter.OnActionExecutionAsync(actionExecutingContext, Next);
            Assert.IsNull(actionExecutingContext.Result, "User is not authorized");
            return actionExecutingContext.Result == null;
        }
        
        private static UpdateMongoDbObject CreateValidUpdateObject()
        {
            return new UpdateMongoDbObject
            {
                Differences = new Dictionary<string, object> { { "John Doe", "Johnny Doe" } },
                RenameMap = new Dictionary<string, string> { { "name", "nameChanged" } }
            };
        }

        private async Task<string?> GetDocumentIdFromCollection()
        {
            var database = _dbController.mongoDbOperations.client.GetDatabase("UnitTestDb");
            var collection = database.GetCollection<BsonDocument>("collection");

            try
            {
                var document = await collection.Find(new BsonDocument()).FirstOrDefaultAsync();
                return document?["_id"]?.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching document ID: " + ex.Message);
                return null;
            }
        }
        
        private async Task<BsonDocument?> GetDocumentById(string dbName, string collectionName, string id)
        {
            var database = _dbController.mongoDbOperations.client.GetDatabase(dbName);
            var collection = database.GetCollection<BsonDocument>(collectionName);
            var filter = Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(id));

            try
            {
                return await collection.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching document by ID: " + ex.Message);
                return null;
            }
        }

    }
}