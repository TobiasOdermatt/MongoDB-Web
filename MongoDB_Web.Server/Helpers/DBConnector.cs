using MongoDB.Driver;

namespace MongoDB_Web.Server.Helpers
{
    public class DBConnector
    {
        public MongoClient DbConnect(string connectionString)
        {
            return new MongoClient(connectionString);
        }
    }
}
