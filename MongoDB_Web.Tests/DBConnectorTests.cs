using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;
using System;

namespace MongoDB_Web.Server.Tests
{
    internal class DBConnectorTests
    {
        private DBConnector _dbConnector;

        [SetUp]
        public void Setup()
        {
            _dbConnector = new DBConnector();
        }

        [Test]
        public void DbConnectIsMongoDbAlive()
        {
            string connectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING") ?? "";
            MongoClient client = _dbConnector.DbConnect(connectionString);
            Assert.That(client, Is.Not.Null);

            var database = client.GetDatabase("TestDatabase");
            var collectionName = "TestCollection";
            database.CreateCollection(collectionName);
            var filter = new BsonDocument("name", collectionName);
            var collections = database.ListCollections(new ListCollectionsOptions { Filter = filter });
            var exists = collections.Any();
            Assert.IsTrue(exists, "Test collection was not created.");

            client.DropDatabase("TestDatabase");
        }
    }
}
