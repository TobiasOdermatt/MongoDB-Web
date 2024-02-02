﻿using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Clusters;
using static mongodbweb.Server.Helpers.LogManager;

namespace mongodbweb.Server.Helpers
{
    public class DbConnector
    {
        public readonly MongoClient? client;

        public DbConnector(string username, string password, string ipOfRequest)
        {
            client = DbConnect(username, password, ipOfRequest);
        }

        public DbConnector() { }

        public static MongoClient DbConnect(string connectionString)
        {
            return new MongoClient(connectionString);
        }

        private MongoClient? DbConnect(string username, string password, string ipOfRequest)
        {
            if (ConfigManager.allowedIp != "*" && ConfigManager.allowedIp != ipOfRequest)
            {
                _= new LogManager(LogType.Error, $"User; {username} has failed to connect to the DB, IP: {ipOfRequest} is not allowed");
                return null;
            }

            if ((string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) && ConfigManager.useAuthorization)
                return null;

            var connectionString = GetConnectionString(username, password);
            MongoClient mongoClient = new(connectionString);
            try
            {
                var connection = mongoClient.GetDatabase("pingTest").RunCommandAsync((Command<BsonDocument>)"{ping:1}")
                    .Wait(1000);
                return connection ? mongoClient : null;
            }
            catch (Exception e)
            {
                _= new LogManager(LogType.Error, $"User: {username} has failed to connect to the DB, IP: {ipOfRequest} with error: {e.Message}", e);
                return null;
            }
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
            return users != null && (from BsonDocument user in users select user["roles"].AsBsonArray).Any(roles => (from BsonDocument role in roles let roleDb = role["db"].AsString let roleName = role["role"].AsString where roleDb == "admin" && roleName is "root" or "userAdmin" or "userAdminAnyDatabase" select roleDb).Any());
        }
    }

}