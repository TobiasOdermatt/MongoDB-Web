using System.Net;
using System.Text.RegularExpressions;
using Bogus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using api.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace api.Tests;

public static class TestSetup
{
    private const string ConnectionString = "mongodb://user:hallo1234@localhost:27017/?authSource=admin";
    private const string DbHost = "localhost";
    private const string DbPort = "27017";
    private const string DbRules = "?authSource=admin";
    private const string AllowedIp = "*";
    private const bool UseAuthorization = true;

    private static readonly AuthController AuthController = new();

    public static string GetConnectionString()
    {
        return Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING") ?? ConnectionString;
    }

    public static (string username, string password) ExtractUsernameAndPassword()
    {
        const string pattern = @"mongodb:\/\/([^:]+):([^@]+)@";
        var match = Regex.Match(GetConnectionString(), pattern);
        if (!match.Success) return ("", "");
        var username = match.Groups[1].Value;
        var password = match.Groups[2].Value;

        return (username, password);
    }

    public static void ConfigureDbConnector()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"UseAuthorization", UseAuthorization.ToString()},
            {"BatchCount", "100"},
            {"DBHost", DbHost},
            {"DBPort", DbPort},
            {"DBRule", DbRules},
            {"CustomConnectionString", ""},
            {"DeleteOtpInDays", "1" },
            {"AllowedIP", AllowedIp}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        ConfigManager.SetConfig(configuration);
    }

    public static HttpContext GetValidHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
        AuthController.ControllerContext = new ControllerContext()
        {
            HttpContext = context
        };

        var usernameAndPassword = ExtractUsernameAndPassword();

        var validCredentials = new ConnectRequestObject { Username = usernameAndPassword.username, Password = usernameAndPassword.password };
        var result = AuthController.CreateOtp(validCredentials) as JsonResult;

        if (result?.Value == null) return new DefaultHttpContext();
        var valueType = result.Value.GetType();
        var uuidProperty = valueType.GetProperty("uuid");
        var tokenProperty = valueType.GetProperty("token");

        if (uuidProperty == null || tokenProperty == null) return new DefaultHttpContext();
        var uuid = (string)uuidProperty.GetValue(result.Value)!;
        var token = (string)tokenProperty.GetValue(result.Value)!;

        context.Request.Headers.Append("Cookie", new StringValues($"UUID={uuid}; Token={token}"));
        return context;
    }

    public static void DropTestDatabase()
    {
        var client = new MongoClient(GetConnectionString());
        client.DropDatabase("UnitTestDb");
    }

    public static void GenerateTestDb()
    {
        var client = new MongoClient(GetConnectionString());
        var db = client.GetDatabase("UnitTestDb");
        var collection = db.GetCollection<BsonDocument>("collection");
        var batch = new List<BsonDocument>
        {
            new BsonDocument
            {
                { "name", "John Doe" },
                { "email", "test@test.de" }
            }
        };
        collection.InsertManyAsync(batch).Wait();
    }

    public static void GenerateRandomData(string dbName, int collectionsCount, int totalDocuments, Guid guid)
    {
        var client = new MongoClient(GetConnectionString());
        var faker = new Faker();
        var db = client.GetDatabase(dbName);
        var documentsPerCollection = totalDocuments / collectionsCount;

        for (var i = 1; i <= collectionsCount; i++)
        {
            var collectionName = $"collection-{i}";
            var collection = db.GetCollection<BsonDocument>(collectionName);
            var batch = new List<BsonDocument>();

            for (var j = 0; j < documentsPerCollection; j++)
            {
                var randomData = new BsonDocument
                {
                    { "name", faker.Name.FullName()},
                    { "email", faker.Internet.Email()},
                    { "address", faker.Address.StreetAddress()},
                    { "phone", faker.Phone.PhoneNumber()},
                    { "company", faker.Company.CompanyName()},
                    { "jobTitle", faker.Name.JobTitle()},
                };

                batch.Add(randomData);

                if (batch.Count < 100) continue;
                collection.InsertManyAsync(batch).Wait();
                batch.Clear();
            }

            if (batch.Count > 0)
            {
                collection.InsertManyAsync(batch).Wait();
            }
        }
    }
}