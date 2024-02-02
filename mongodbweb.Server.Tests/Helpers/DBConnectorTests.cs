using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace mongodbweb.Server.Tests.Helpers
{
    internal class DbConnectorTests
    {
        private readonly DbConnector _dbConnector = new ();

        [SetUp]
        public void Setup()
        {
            Environment.SetEnvironmentVariable("MONGODB_CONNECTION_STRING", "mongodb://user:hallo1234@localhost:27017");
        }

        [Test]
        public void DbConnect_Is_MongoDb_Alive()
        {
            var connectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING") ?? "";
            var client = DbConnector.DbConnect(connectionString);
            Assert.That(client, Is.Not.Null);

            var database = client.GetDatabase("UnitTestDb");
            const string collectionName = "TestCollection";
            
            if (database.ListCollectionNames().ToList().Contains(collectionName))
                database.DropCollection(collectionName);

            database.CreateCollection(collectionName);
            var filter = new BsonDocument("name", collectionName);
            var collections = database.ListCollections(new ListCollectionsOptions { Filter = filter });
            var exists = collections.Any();
            Assert.IsTrue(exists, "Test collection was not created.");

            client.DropDatabase("UnitTestDb");
        }
    }
}
