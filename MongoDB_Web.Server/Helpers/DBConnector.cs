using MongoDB.Driver;
using static MongoDB_Web.Server.Helpers.LogManager;

namespace MongoDB_Web.Server.Helpers
{
    public class DBConnector
    {
        public MongoClient? Client;
        public static bool useAuthorization = true;
        static string? dbHost;
        static string? dbPort;
        static string? dbRules;
        static string? customString;
        static string? allowedIp;
        static int batchCount = 90;

        //Loads the connection values from config.json
        public DBConnector(IConfiguration config)
        {
            if (Boolean.TryParse(config["UseAuthorization"], out bool _useAuthorizationbool))
            {
                useAuthorization = _useAuthorizationbool;
            }
            if (int.TryParse(config["BatchCount"], out int _batchCount))
            {
                batchCount = _batchCount;
            }

            dbHost = config["DBHost"];
            dbPort = config["DBPort"];
            dbRules = config["DBRule"];
            customString = config["CustomConnectionString"];
            allowedIp = config["AllowedIP"];
        }

        public DBConnector(string username, string password, string uuid, string ipOfRequest)
        {
            Client = DbConnect(username, password, uuid, ipOfRequest);
        }

        public DBConnector() { }

        public MongoClient DbConnect(string connectionString)
        {
            return new MongoClient(connectionString);
        }

        public MongoClient? DbConnect(string username, string password, string uuid, string ipOfRequest)
        {
            if (allowedIp != "*" && allowedIp != ipOfRequest)
            {
                log($"User; {username} has failed to connect to the DB, IP: {ipOfRequest} is not allowed");
                return null;
            }

            string connectionString = getConnectionString(username, password);
            MongoClient mongoClient = new(connectionString);
            return mongoClient;
        }

        private void log(string message, Exception? e = null)
        {
            if (e == null)
                _ = new LogManager(LogType.Error, message);
            else
                _ = new LogManager(LogType.Error, message, e);
        }


        public string getConnectionString(string username, string password)
        {
            if (!string.IsNullOrEmpty(customString))
                return customString;

            return useAuthorization
                ? $"mongodb://{Sanitize(username)}:{Sanitize(password)}@{dbHost}:{dbPort}/{dbRules}"
                : $"mongodb://{dbHost}:{dbPort}";
        }

        private static string Sanitize(string input)
        {
            input = input.Trim();
            return Uri.EscapeDataString(input);
        }


        /// <summary>
        /// Test if the user can see any databases.
        /// </summary>
        /// <returns>True if databases exist, otherwise false.</returns>
        public bool ListAllDatabasesTest()
        {
            try
            {
                return Client?.ListDatabases().Any() == true;
            }
            catch
            {
                return false;
            }
        }
    }

}
