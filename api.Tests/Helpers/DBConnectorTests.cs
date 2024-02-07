using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace mongodbweb.Server.Tests.Helpers
{
    internal class DbConnectorTests
    {
        private readonly DbConnector _dbConnector = new ();

        [Test]
        public void DbConnect_Is_MongoDb_Alive()
        {
            var client = DbConnector.DbConnect(TestSetup.GetConnectionString());
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
