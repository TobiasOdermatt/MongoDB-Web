using System.Collections;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using api.Tests;
using api.Models;
using api.Controllers;
using api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace api.Tests.Controllers
{
    [TestFixture]
    public class DbControllerTests
    {
        private DbController _dbController;
        private HttpContext _validHttpContext = new DefaultHttpContext();

        [SetUp]
        public async Task Setup()
        {
            var mockHubContext = new Moq.Mock<IHubContext<ProgressHub>>();
            _dbController = new DbController(mockHubContext.Object);
            TestSetup.ConfigureDbConnector();
            _validHttpContext = TestSetup.GetValidHttpContext();
            var isAuthorized = await CheckAuthorization();
            Assert.That(isAuthorized, Is.True);
            _dbController.mongoDbOperations.client = new MongoClient(TestSetup.GetConnectionString());
        }

        [Test]
        public void ListDb_ReturnsOk_WithDbList()
        {
            TestSetup.GenerateTestDb();
            var result = _dbController.ListDb() as ObjectResult;
            TestSetup.DropTestDatabase();
            Assert.That(result, Is.Not.Null);
            if (result == null) return;
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));

            var resultValue = result.Value;
            Assert.That(resultValue, Is.Not.Null, "Result value should not be null");

            var databasesProperty = resultValue?.GetType().GetProperty("databases");
            Assert.That(databasesProperty, Is.Not.Null, "The 'databases' property should exist in the result value.");

            var databases = databasesProperty?.GetValue(resultValue) as IEnumerable;
            Assert.That(databases, Is.Not.Null, "Databases should not be null.");
            if (databases == null) return;
            var enumerable = databases as object[] ?? databases.Cast<object>().ToArray();
            Assert.That(databases != null && enumerable.Any(), Is.True, "Databases should contain elements.");
            var containsUnitTestDb = enumerable.Cast<Dictionary<string, object>>()
                .Any(db => db.ContainsKey("name") && db["name"].ToString() == "UnitTestDb");
            Assert.That(containsUnitTestDb, Is.True, "Databases should contain a database named 'UnitTestDb'.");
        }


        [Test]
        public void ListCollections_ReturnsBadRequest_WhenDbNameIsEmpty()
        {
            var result = _dbController.ListCollections(string.Empty) as ObjectResult;

            Assert.That(result, Is.Not.Null);
            if (result == null) return;
            Assert.Multiple(() =>
            {
                Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest), "Status code should be 400 OK");
                Assert.That(result.Value, Is.EqualTo("Database name is required."));
            });
        }

        [Test]
        public void ListCollections_ReturnsOk_WithCollectionList()
        {
            TestSetup.GenerateTestDb();
            var result = _dbController.ListCollections("UnitTestDb") as ObjectResult;
            TestSetup.DropTestDatabase();
            Assert.That(result, Is.Not.Null);
            if (result == null) return;
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));

            var resultValue = result.Value;
            Assert.That(resultValue, Is.Not.Null, "Result value should not be null");

            var collectionsProperty = resultValue?.GetType().GetProperty("collections");
            Assert.That(collectionsProperty, Is.Not.Null, "The 'collections' property should exist in the result value.");

            var collections = collectionsProperty?.GetValue(resultValue) as IEnumerable;
            Assert.That(collections, Is.Not.Null, "Collections should not be null.");
            Assert.That(collections != null && collections.Cast<object>().Any(), Is.True,
                "Collections should contain elements.");
        }

        [Test]
        public void GetNumberOfCollections_ReturnsOk_WithCollectionCount()
        {
            TestSetup.GenerateTestDb();
            var result = _dbController.GetNumberOfCollections("UnitTestDb") as ObjectResult;
            TestSetup.DropTestDatabase();

            Assert.That(result, Is.Not.Null);
            if (result == null) return;
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            var resultValue = result.Value;
            Assert.That(resultValue, Is.Not.Null);
            var countProperty = resultValue?.GetType().GetProperty("count");
            Assert.That(countProperty, Is.Not.Null, "The 'count' property should exist in the result value.");

            var collectionCount = (int)(countProperty?.GetValue(resultValue) ?? 0);
            Assert.That(collectionCount >= 1, Is.True, "Collection count should be greater than 1.");
        }

        [Test]
        public void GetNumberOfDocuments_ReturnsOk_WithDocumentCount()
        {
            string dbName = "UnitTestDb";
            string collectionName = "TestCollection";
            TestSetup.GenerateTestDb();
            var result = _dbController.GetNumberOfDocuments(dbName, collectionName) as ObjectResult;

            Assert.That(result, Is.Not.Null);
            if (result == null) return; 
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));

            var resultValue = result.Value;
            Assert.That(resultValue, Is.Not.Null);
            var countProperty = resultValue?.GetType().GetProperty("count");
            Assert.That(countProperty, Is.Not.Null, "The 'count' property should exist in the result value.");

            var documentCount = (int)(countProperty?.GetValue(resultValue) ?? 0);
            Assert.That(documentCount, Is.EqualTo(5), "Document count should be equal to the number of inserted documents.");
        }


        [Test]
        public void GetNumberOfCollections_ReturnsBadRequest_WithoutDatabaseName()
        {
            var result = _dbController.GetNumberOfCollections("") as ObjectResult;
            Assert.That(result, Is.Not.Null);
            if (result == null) return;
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }


        [Test]
        public void GetCollection_ReturnsOk_WithCollection()
        {
            TestSetup.GenerateTestDb();
            var result = _dbController.GetCollection("UnitTestDb", "collection") as ObjectResult;
            TestSetup.DropTestDatabase();

            Assert.That(result, Is.Not.Null);
            if (result == null) return;
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            var resultValue = result.Value;
            Assert.That(resultValue, Is.Not.Null);
            var collectionProperty = resultValue?.GetType().GetProperty("collection");
            Assert.That(collectionProperty, Is.Not.Null, "The 'collection' property should exist in the result value.");

            var collection = collectionProperty?.GetValue(resultValue) as IEnumerable;
            Assert.That(collection, Is.Not.Null, "Collection should not be null.");
            Assert.That(collection != null && collection.Cast<object>().Any(), Is.True, "Collection should contain elements.");
        }

        [Test]
        public void GetCollection_ReturnsBadRequest_WhenDbNameIsEmpty()
        {
            var result = _dbController.GetCollection(string.Empty, "collection") as ObjectResult;

            Assert.That(result, Is.Not.Null);
            if (result == null) return;
            Assert.Multiple(() =>
            {
                Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest), "Status code should be 400 OK");
                Assert.That(result.Value, Is.EqualTo("Database name is required."));
            });
        }

        [Test]
        public void GetCollection_ReturnsBadRequest_WhenCollectionNameIsEmpty()
        {
            var result = _dbController.GetCollection("UnitTestDb", string.Empty) as ObjectResult;

            Assert.That(result, Is.Not.Null);
            if (result == null) return;
            Assert.Multiple(() =>
            {
                Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest), "Status code should be 400 OK");
                Assert.That(result.Value, Is.EqualTo("Collection name is required."));
            });
        }

        [Test]
        public void GetCollectionAttributes_ReturnsOk_WithCollectionAttributes()
        {
            TestSetup.GenerateTestDb();
            var result = _dbController.GetCollectionAttributes("UnitTestDb", "collection") as ObjectResult;
            TestSetup.DropTestDatabase();

            Assert.That(result, Is.Not.Null);
            if (result == null) return;
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            var resultValue = result.Value;
            Assert.That(resultValue, Is.Not.Null);
            var attributesProperty = resultValue?.GetType().GetProperty("attributes");
            Assert.That(attributesProperty, Is.Not.Null, "The 'collection' property should exist in the result value.");

            var attributes = attributesProperty?.GetValue(resultValue) as IEnumerable;
            Assert.That(attributes, Is.Not.Null, "Collection should not be null.");
            if (attributes == null) return;
            var enumerable = attributes.Cast<object>().ToList();
            Assert.That(attributes != null && enumerable.Any(), Is.True, "Collection should contain elements.");

            var attributeList = enumerable.Cast<string>().ToList();
            Assert.Multiple(() =>
            {
                Assert.That(attributeList.Contains("_id"), Is.True, "Attribute list should contain '_id'");
                Assert.That(attributeList.Contains("name"), Is.True, "Attribute list should contain 'name'");
                Assert.That(attributeList.Contains("email"), Is.True, "Attribute list should contain 'email'");
            });
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

            Assert.That(result, Is.Not.Null);
            if (result == null)
                return;

            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            var resultValue = result.Value as string;
            Assert.That(resultValue, Is.EqualTo("Attributes renamed successfully."));

            var updatedAttributes = _dbController.mongoDbOperations.GetCollectionAttributes("UnitTestDb", "collection");
            TestSetup.DropTestDatabase();
            Assert.Multiple(() =>
            {
                Assert.That(updatedAttributes.Contains("newName"), Is.True, "Attribute 'newName' should exist after renaming.");
                Assert.That(updatedAttributes.Contains("name"), Is.False, "Attribute 'name' should not exist after renaming.");
            });
        }

        [Test]
        public async Task RenameAttributesInCollectionAsync_ReturnsBadRequest_WhenDbNameIsEmpty()
        {
            var result =
                await _dbController.RenameAttributesInCollectionAsync("", "collection",
                    []) as ObjectResult;
            Assert.That(result, Is.Not.Null);
            if (result != null) Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }

        [Test]
        public async Task RenameAttributesInCollectionAsync_ReturnsBadRequest_WhenCollectionNameIsEmpty()
        {
            var result =
                await _dbController.RenameAttributesInCollectionAsync("UnitTestDb", "",
                    []) as ObjectResult;
            Assert.That(result, Is.Not.Null);
            if (result != null) Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }

        [Test]
        public async Task RenameAttributesInCollectionAsync_ReturnsBadRequest_WhenRenameMapIsEmpty()
        {
            var result =
                await _dbController.RenameAttributesInCollectionAsync("UnitTestDb", "collection",
                    []) as ObjectResult;
            Assert.That(result, Is.Not.Null);
            Assert.That(result?.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }

        [Test]
        public void GetCollectionAttributes_ReturnsBadRequest_WhenDbNameIsEmpty()
        {
            var result = _dbController.GetCollectionAttributes(string.Empty, "collection") as ObjectResult;

            Assert.That(result, Is.Not.Null);
            if (result == null) return;
            Assert.Multiple(() =>
            {
                Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest), "Status code should be 400 OK");
                Assert.That(result.Value, Is.EqualTo("Database name is required."));
            });
        }

        [Test]
        public void GetCollectionAttributes_ReturnsBadRequest_WhenCollectionNameIsEmpty()
        {
            var result = _dbController.GetCollectionAttributes("UnitTestDb", string.Empty) as ObjectResult;

            Assert.That(result, Is.Not.Null);
            if (result == null) return;
            Assert.Multiple(() =>
            {
                Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest), "Status code should be 400 OK");
                Assert.That(result.Value, Is.EqualTo("Collection name is required."));
            });
        }

        [Test]
        public void GetTotalCount_ReturnsOk_WithTotalCountWithEmptyKeyAndValue()
        {
            TestSetup.GenerateTestDb();
            var result = _dbController.GetTotalCount("UnitTestDb", "collection", "", "") as ObjectResult;
            TestSetup.DropTestDatabase();

            Assert.That(result, Is.Not.Null);
            if (result == null) return;
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            var resultValue = result.Value;
            Assert.That(resultValue, Is.Not.Null);
            var totalCountProperty = resultValue?.GetType().GetProperty("totalCount");
            Assert.That(totalCountProperty, Is.Not.Null, "The 'totalCount' property should exist in the result value.");

            var collectionCount = (long)(totalCountProperty?.GetValue(resultValue) ?? 0);
            Assert.That(collectionCount, Is.GreaterThanOrEqualTo(1), "totalCount should be greater than 1.");
        }

        [Test]
        public void GetTotalCount_ReturnsOk_WithTotalCountWithKeyAndValue()
        {
            TestSetup.GenerateTestDb();
            var result = _dbController.GetTotalCount("UnitTestDb", "collection", "email", "test") as ObjectResult;
            TestSetup.DropTestDatabase();

            Assert.That(result, Is.Not.Null);
            if (result == null) return;
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            var resultValue = result.Value;
            Assert.That(resultValue, Is.Not.Null);
            var totalCountProperty = resultValue?.GetType().GetProperty("totalCount");
            Assert.That(totalCountProperty, Is.Not.Null, "The 'totalCount' property should exist in the result value.");

            var collectionCount = (long)(totalCountProperty?.GetValue(resultValue) ?? 0);
            Assert.That(collectionCount, Is.GreaterThanOrEqualTo(1), "totalCount should be greater than 1.");
        }

        [Test]
        public void GetTotalCount_ReturnsOk_WithTotalCountWithoutKeyButValue()
        {
            TestSetup.GenerateTestDb();
            var result = _dbController.GetTotalCount("UnitTestDb", "collection", "", "test") as ObjectResult;
            TestSetup.DropTestDatabase();

            Assert.That(result, Is.Not.Null);
            if (result == null) return;
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            var resultValue = result.Value;
            Assert.That(resultValue, Is.Not.Null);
            var totalCountProperty = resultValue?.GetType().GetProperty("totalCount");
            Assert.That(totalCountProperty, Is.Not.Null, "The 'totalCount' property should exist in the result value.");

            var collectionCount = (long)(totalCountProperty?.GetValue(resultValue) ?? 0);
            Assert.That(collectionCount >= 1, Is.True, "totalCount should be greater than 1.");
        }

        [Test]
        public void GetTotalCount_ReturnsBadRequest_WhenDbNameIsEmpty()
        {
            var result = _dbController.GetTotalCount("", "collection", "key", "value") as ObjectResult;
            Assert.That(result, Is.Not.Null);
            if (result != null) Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }

        [Test]
        public void GetTotalCount_ReturnsBadRequest_WhenCollectionNameIsEmpty()
        {
            var result = _dbController.GetTotalCount("UnitTestDb", "", "key", "value") as ObjectResult;
            Assert.That(result, Is.Not.Null);
            if (result != null) Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }

        [Test]
        public void GetPaginatedCollection_ReturnsBadRequest_WhenDbNameIsEmpty()
        {
            var result =
                _dbController.GetPaginatedCollection("", "collectionName", 0, 10, "key", "value") as ObjectResult;
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result?.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
                Assert.That(result?.Value, Is.EqualTo("Database name is required."));
            });
        }

        [Test]
        public void GetPaginatedCollection_ReturnsBadRequest_WhenCollectionNameIsEmpty()
        {
            var result = _dbController.GetPaginatedCollection("UnitTestDb", "", 0, 10, "key", "value") as ObjectResult;
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result?.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
                Assert.That(result?.Value, Is.EqualTo("Collection name is required."));
            });
        }

        [Test]
        public void GetPaginatedCollection_ReturnsNotFound_WhenNoData()
        {
            var result =
                _dbController.GetPaginatedCollection("dbName", "collectionName", 0, 10, "key", "value") as ObjectResult;
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result?.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
                Assert.That(result?.Value, Is.EqualTo($"No data found in collection 'collectionName' in database 'dbName'."));
            });
        }

        [Test]
        public void GetPaginatedCollection_ReturnsOk_WithKeyAndValue()
        {
            TestSetup.GenerateTestDb();
            var result =
                _dbController.GetPaginatedCollection("UnitTestDb", "collection", 0, 10, "email",
                    "test") as ObjectResult;
            TestSetup.DropTestDatabase();
            Assert.That(result, Is.Not.Null);
            Assert.That(result?.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
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
            Assert.That(result, Is.Not.Null);
            Assert.That(result?.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            var resultValue = result?.Value as dynamic;
            Assert.IsNotNull(resultValue);
        }

        [Test]
        public void GetCollectionCount_ReturnsBadRequest_WhenDbNameIsEmpty()
        {
            var result = _dbController.GetCollectionCount("", "collectionName", "key", "value") as ObjectResult;
            Assert.That(result, Is.Not.Null);
            if (result == null) return;
            Assert.Multiple(() =>
            {
                Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
                Assert.That(result.Value, Is.EqualTo("Database name is required."));
            });
        }

        [Test]
        public void GetCollectionCount_ReturnsBadRequest_WhenCollectionNameIsEmpty()
        {
            var result = _dbController.GetCollectionCount("UnitTestDb", "", "key", "value") as ObjectResult;
            Assert.That(result, Is.Not.Null);
            if (result == null) return;
            Assert.Multiple(() =>
            {
                Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
                Assert.That(result.Value, Is.EqualTo("Collection name is required."));
            });
        }

        [Test]
        public void GetCollectionCount_ReturnsOk_WithCount()
        {
            TestSetup.GenerateTestDb();
            var result = _dbController.GetCollectionCount("UnitTestDb", "collection", "email", "test") as ObjectResult;
            TestSetup.DropTestDatabase();
            Assert.That(result, Is.Not.Null);
            Assert.That(result?.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            var resultValue = result?.Value as dynamic;
            Assert.IsNotNull(resultValue);
            var totalCountProperty = resultValue?.GetType().GetProperty("count");
            Assert.IsNotNull(totalCountProperty, "The 'count' property should exist in the result value.");

            var collectionCount = (long)(totalCountProperty?.GetValue(resultValue) ?? 0);
            Assert.That(collectionCount == 1, Is.True, "documents count should be 1.");
        }

        [Test]
        public async Task InsertDocumentAsync_InsertsDocumentSuccessfully()
        {
            dynamic document = new { Name = "Test", Value = "123" };
            var result =
                await _dbController.InsertDocumentAsync("UnitTestDb", "newCollection", document) as ObjectResult;
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result?.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
                Assert.That(result?.Value, Is.EqualTo("Document inserted successfully."));
            });
            var collection = _dbController.mongoDbOperations.client.GetDatabase("UnitTestDb")
                .GetCollection<BsonDocument>("newCollection");
            var insertedDocument = collection.Find(new BsonDocument("Name", "Test")).FirstOrDefault();
            TestSetup.DropTestDatabase();
            Assert.That(insertedDocument, Is.Not.Null);
            Assert.That(insertedDocument["Value"].AsString, Is.EqualTo("123"));
        }

        [Test]
        public async Task InsertDocumentAsync_ReturnsBadRequest_WhenDbNameIsEmpty()
        {
            dynamic document = new { Name = "Test", Value = "123" };

            var result = await _dbController.InsertDocumentAsync("", "test", document) as ObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result?.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
                Assert.That(result?.Value, Is.EqualTo("Database name is required."));
            });
        }

        [Test]
        public async Task InsertDocumentAsync_ReturnsBadRequest_WhenCollectionNameIsEmpty()
        {
            dynamic document = new { Name = "Test", Value = "123" };

            var result = await _dbController.InsertDocumentAsync("test", "", document) as ObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result?.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
                Assert.That(result?.Value, Is.EqualTo("Collection name is required."));
            });
        }

        [Test]
        public async Task InsertDocumentAsync_ReturnsBadRequest_WhenDocumentIsNull()
        {
            dynamic? document = null;
            var result = await _dbController.InsertDocumentAsync("test", "test", document) as ObjectResult;

            Assert.That(result, Is.Not.Null, "Result should not be null.");
            Assert.That(result, Is.InstanceOf<ObjectResult>(), "Result should be an instance of ObjectResult.");
            Assert.Multiple(() =>
            {
                Assert.That(result?.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest),
                            "Status code should be 400 BadRequest.");
                Assert.That(result?.Value, Is.EqualTo("Document is required."),
                    "The error message should indicate that the document is required.");
            });
        }

        [Test]
        public void DeleteDatabase_ReturnsBadRequest_WhenDbNameIsEmpty()
        {
            var result = _dbController.DeleteDatabase(string.Empty) as ObjectResult;

            Assert.That(result, Is.Not.Null);
            if (result == null) return;
            Assert.Multiple(() =>
            {
                Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
                Assert.That(result.Value, Is.EqualTo("Database name is required."));
            });
        }

        [Test]
        public void DeleteDatabase_ReturnsOk_WhenDeletionSucceeds()
        {
            TestSetup.GenerateTestDb();
            var result = _dbController.DeleteDatabase("UnitTestDb") as ObjectResult;
            TestSetup.DropTestDatabase();

            Assert.That(result, Is.Not.Null);
            if (result == null) return;
            Assert.Multiple(() =>
            {
                Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
                Assert.That(result.Value, Is.EqualTo($"Database 'UnitTestDb' deleted successfully."));
            });
        }

        [Test]
        public void CreateCollection_ReturnsBadRequest_WhenDbNameIsEmpty()
        {
            var result = _dbController.CreateCollection(string.Empty, "TestCollection") as ObjectResult;

            Assert.That(result, Is.Not.Null);
            if (result == null) return;
            Assert.Multiple(() =>
            {
                Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
                Assert.That(result.Value, Is.EqualTo("Database name is required."));
            });
        }

        [Test]
        public void CreateCollection_ReturnsBadRequest_WhenCollectionNameIsEmpty()
        {
            var result = _dbController.CreateCollection("TestDb", string.Empty) as ObjectResult;

            Assert.That(result, Is.Not.Null);
            if (result == null) return;
            Assert.Multiple(() =>
            {
                Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
                Assert.That(result.Value, Is.EqualTo("Collection name is required."));
            });
        }

        [Test]
        public void CreateCollection_ReturnsOk_WhenCreationSucceeds()
        {
            TestSetup.DropTestDatabase();
            var result = _dbController.CreateCollection("UnitTestDb", "TestCollection") as ObjectResult;
            TestSetup.DropTestDatabase();
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result?.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
                Assert.That(result?.Value, Is.EqualTo("Collection 'TestCollection' created successfully in database 'UnitTestDb'."));
            });
        }

        [Test]
        public void DeleteCollection_ReturnsBadRequest_WhenDbNameIsEmpty()
        {
            var result = _dbController.DeleteCollection(string.Empty, "TestCollection") as ObjectResult;

            Assert.That(result, Is.Not.Null);
            if (result == null) return;
            Assert.Multiple(() =>
            {
                Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
                Assert.That(result.Value, Is.EqualTo("Database name is required."));
            });
        }

        [Test]
        public void DeleteCollection_ReturnsBadRequest_WhenCollectionNameIsEmpty()
        {
            var result = _dbController.DeleteCollection("TestDb", string.Empty) as ObjectResult;

            Assert.That(result, Is.Not.Null);
            if (result == null) return;
            Assert.Multiple(() =>
            {
                Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
                Assert.That(result.Value, Is.EqualTo("Collection name is required."));
            });
        }

        [Test]
        public void DeleteCollection_ReturnsOk_WhenDeletionSucceeds()
        {
            var result = _dbController.DeleteCollection("TestDb", "TestCollection") as ObjectResult;

            Assert.That(result, Is.Not.Null);
            if (result == null) return;
            Assert.Multiple(() =>
            {
                Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
                Assert.That(result.Value, Is.EqualTo("Collection 'TestCollection' deleted successfully from database 'TestDb'."));
            });
        }

        [Test]
        public void GetDatabaseStatistics_ReturnsBadRequest_WhenDbNameIsEmpty()
        {
            var result = _dbController.GetDatabaseStatistics(string.Empty) as ObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result?.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
                Assert.That(result?.Value, Is.EqualTo("Database name is required."));
            });
        }

        [Test]
        public void GetDatabaseStatistics_ReturnsOk_WhenStatsFound()
        {
            var result = _dbController.GetDatabaseStatistics("TestDb") as ObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result?.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        }

        [Test]
        public void GetCollectionStatistics_ReturnsBadRequest_WhenDbNameIsEmpty()
        {
            var result = _dbController.GetCollectionStatistics(string.Empty, "TestCollection") as ObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result?.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
                Assert.That(result?.Value, Is.EqualTo("Database name is required."));
            });
        }

        [Test]
        public void GetCollectionStatistics_ReturnsBadRequest_WhenCollectionNameIsEmpty()
        {
            var result = _dbController.GetCollectionStatistics("TestDb", string.Empty) as ObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result?.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
                Assert.That(result?.Value, Is.EqualTo("Collection name is required."));
            });
        }

        [Test]
        public void GetCollectionStatistics_ReturnsOk_WhenStatsFound()
        {
            var result = _dbController.GetCollectionStatistics("TestDb", "TestCollection") as ObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result?.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        }

        [Test]
        public void GetGlobalStatistics_ReturnsOk_WhenStatsFound()
        {
            var result = _dbController.GetGlobalStatistics() as ObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result?.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        }

        [Test]
        public void CheckIfDatabaseExists_ReturnsBadRequest_WhenDbNameIsEmpty()
        {
            var result = _dbController.CheckIfDatabaseExists(string.Empty) as ObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result?.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
                Assert.That(result?.Value, Is.EqualTo("Database name is required."));
            });
        }

        [Test]
        public void CheckIfDatabaseExists_ReturnsFalse_WhenDbDoesNotExist()
        {
            var result = _dbController.CheckIfDatabaseExists("NonExistentDb") as ObjectResult;

            Assert.That(result, Is.Not.Null);
            if (result == null) return;
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));

            var resultValue = result.Value;
            Assert.That(resultValue, Is.Not.Null, "Result value should not be null");

            var existsProperty = resultValue?.GetType().GetProperty("exists");
            Assert.That(existsProperty, Is.Not.Null, "The 'exists' property should exist in the result value.");

            var exists = (bool)(existsProperty?.GetValue(resultValue) ?? false);
            Assert.That(exists, Is.False, "The database should not exist.");
        }

        [Test]
        public void CheckIfDatabaseExists_ReturnsTrue_WhenDbExists()
        {
            TestSetup.GenerateTestDb();
            var result = _dbController.CheckIfDatabaseExists("UnitTestDb") as ObjectResult;
            TestSetup.DropTestDatabase();
            Assert.That(result, Is.Not.Null);
            if (result == null) return;
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            if (result.Value == null) return;
            var resultValue = result.Value;
            Assert.That(resultValue, Is.Not.Null, "Result value should not be null");

            var existsProperty = resultValue?.GetType().GetProperty("exists");
            Assert.That(existsProperty, Is.Not.Null, "The 'exists' property should exist in the result value.");

            var exists = (bool)(existsProperty?.GetValue(resultValue) ?? false);
            Assert.That(exists, Is.True, "The database should  exist.");
        }

        [Test]
        public async Task UploadJsonAsync_ReturnsBadRequest_WhenDbNameIsEmpty()
        {
            var result =
                await _dbController.UploadJsonAsync(string.Empty, "TestCollection", new JObject(), false) as
                    ObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result?.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
                Assert.That(result?.Value, Is.EqualTo("Database name is required."));
            });
        }

        [Test]
        public async Task UploadJsonAsync_ReturnsBadRequest_WhenCollectionNameIsEmpty()
        {
            var result =
                await _dbController.UploadJsonAsync("TestDb", string.Empty, new JObject(), false) as ObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result?.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
                Assert.That(result?.Value, Is.EqualTo("Collection name is required."));
            });
        }

        [Test]
        public async Task UploadJsonAsync_ReturnsBadRequest_WhenJsonIsNull()
        {
            var result = await _dbController.UploadJsonAsync("TestDb", "TestCollection", null, false) as ObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result?.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
                Assert.That(result?.Value, Is.EqualTo("JSON content is required."));
            });
        }

        [Test]
        public async Task UploadJsonAsync_ReturnsOk_WhenUploadSucceeds()
        {
            var result =
                await _dbController.UploadJsonAsync("TestDb", "TestCollection", new JObject(), false) as ObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result?.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
                Assert.That(result?.Value, Is.EqualTo("JSON uploaded successfully."));
            });
        }

        [Test]
        public async Task ExecuteMongoQuery_ReturnsBadRequest_WhenDbNameIsEmpty()
        {
            var result = await _dbController.ExecuteMongoQuery(string.Empty, "TestCollection", "query") as ObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result?.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
                Assert.That(result?.Value, Is.EqualTo("Database name is required."));
            });
        }

        [Test]
        public async Task ExecuteMongoQuery_ReturnsBadRequest_WhenCollectionNameIsEmpty()
        {
            var result = await _dbController.ExecuteMongoQuery("TestDb", string.Empty, "query") as ObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result?.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
                Assert.That(result?.Value, Is.EqualTo("Collection name is required."));
            });
        }

        [Test]
        public async Task ExecuteMongoQuery_ReturnsBadRequest_WhenQueryIsEmpty()
        {
            var result =
                await _dbController.ExecuteMongoQuery("TestDb", "TestCollection", string.Empty) as ObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result?.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
                Assert.That(result?.Value, Is.EqualTo("Query is required."));
            });
        }

        [Test]
        public async Task ExecuteMongoQuery_ReturnsOk_WhenQueryExecutesSuccessfully()
        {
            TestSetup.GenerateTestDb();
            var result =
                await _dbController.ExecuteMongoQuery("UnitTestDb", "collection", "{ \"age\": { \"$gt\": 25 } }") as
                    ObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result?.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        }

        [Test]
        public async Task UpdateMongoDb_ReturnsBadRequest_WhenDbNameIsEmpty()
        {
            var result =
                await _dbController.UpdateMongoDb(string.Empty, "TestCollection", "1", CreateValidUpdateObject()) as
                    ObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result?.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
                Assert.That(result?.Value, Is.EqualTo("Database name is required."));
            });
        }

        [Test]
        public async Task UpdateMongoDb_ReturnsBadRequest_WhenCollectionNameIsEmpty()
        {
            var result =
                await _dbController.UpdateMongoDb("TestDb", string.Empty, "1", CreateValidUpdateObject()) as
                    ObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result?.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
                Assert.That(result?.Value, Is.EqualTo("Collection name is required."));
            });
        }

        [Test]
        public async Task UpdateMongoDb_ReturnsBadRequest_WhenIdIsEmpty()
        {
            var result =
                await _dbController.UpdateMongoDb("TestDb", "TestCollection", string.Empty, CreateValidUpdateObject())
                    as ObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result?.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
                Assert.That(result?.Value, Is.EqualTo("Document ID is required."));
            });
        }

        [Test]
        public async Task UpdateMongoDb_ReturnsBadRequest_WhenUpdateObjectIsInvalid()
        {
            var invalidUpdateObject = new UpdateMongoDbObject();

            var result =
                await _dbController.UpdateMongoDb("TestDb", "TestCollection", "1", invalidUpdateObject) as ObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result?.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
                Assert.That(result?.Value, Is.EqualTo("Differences and rename map are required."));
            });
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

                Assert.That(result, Is.Not.Null);
                if (result != null)
                {
                    Assert.Multiple(() =>
                    {
                        Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
                        Assert.That(result.Value != null && result.Value.ToString()!.Contains("updated successfully"), Is.True);
                    });
                    var updatedDocument = await GetDocumentById("UnitTestDb", "collection", documentId);
                    Assert.That(updatedDocument, Is.Not.Null);
                    Assert.That(updatedDocument?["nameChanged"].AsString, Is.EqualTo("John Doe"));
                }
            }
        }

        private async Task<bool> CheckAuthorization()
        {
            _dbController.ControllerContext = new ControllerContext()
            {
                HttpContext = _validHttpContext
            };
            var authorizationFilter = new api.Filters.Authorization();

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