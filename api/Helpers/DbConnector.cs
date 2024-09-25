using MongoDB.Bson;
using MongoDB.Driver;
using static api.Helpers.LogManager;

namespace api.Helpers
{
    public class DbConnector
    {
        public readonly MongoClient? client;

        public DbConnector(string username, string password, string ipOfRequest)
        {
            client = DbConnect(username, password, ipOfRequest);
        }

        public DbConnector() { }

        public static MongoClient? DbConnect(string connectionString)
        {
            MongoClient mongoClient = new(connectionString);
            try
            {
                var connection = mongoClient.GetDatabase("pingTest").RunCommandAsync((Command<BsonDocument>)"{ping:1}")
                    .Wait(1000);
                return connection ? mongoClient : null;
            }
            catch (Exception e)
            {
                _ = new LogManager(LogType.Error, $"Connectin failed", e);
                return null;
            }
        }

        private static MongoClient? DbConnect(string username, string password, string ipOfRequest)
        {
            if (ConfigManager.allowedIp != "*" && ConfigManager.allowedIp != ipOfRequest)
            {
                _ = new LogManager(LogType.Error, $"User; {username} has failed to connect to the DB, IP: {ipOfRequest} is not allowed");
                return null;
            }

            if ((string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) && ConfigManager.useAuthorization)
                return null;

            var connectionString = GetConnectionString(username, password);
            MongoClient? mongoClient = DbConnect(connectionString);
            if(mongoClient == null) { 
                _ = new LogManager(LogType.Error, $"User: {username} has failed to connect to the DB, IP: {ipOfRequest} ");
                return null;
            }
            return mongoClient;
        }

        public static string GetConnectionString(string username, string password)
        {
            if (!string.IsNullOrEmpty(ConfigManager.customString))
                return ConfigManager.customString;

            return ConfigManager.useAuthorization
                ? $"mongodb://{Sanitize(username)}:{Sanitize(password)}@{ConfigManager.dbHost}:{ConfigManager.dbPort}/{ConfigManager.dbRules}"
                : $"mongodb://{ConfigManager.dbHost}:{ConfigManager.dbPort}";
        }

        private static string Sanitize(string input)
        {
            input = input.Trim();
            return Uri.EscapeDataString(input);
        }

        public bool IsUserAdmin(string username)
        {
            var database = client?.GetDatabase("admin");

            var usersCommand = new BsonDocument { { "usersInfo", username } };
            var result = database?.RunCommand<BsonDocument>(usersCommand);
            var users = result?["users"].AsBsonArray;
            return users != null && users.Count > 0;
        }
    }

}
