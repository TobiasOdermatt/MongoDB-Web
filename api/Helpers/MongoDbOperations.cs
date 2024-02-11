using api.Hubs;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using System;
using static api.Helpers.LogManager;

namespace api.Helpers
{
    public class MongoDbOperations
    {
        private readonly LogManager _logger = new();
        public required MongoClient client;
        public string username = "";
        public string uuid = "";

        public readonly IHubContext<ProgressHub>? _hubContext;

        public MongoDbOperations(IHubContext<ProgressHub>? hubContext)
        {
            _hubContext = hubContext;
        }


        /// <summary>
        /// List every database from mongo
        /// </summary>
        /// <returns>Returns List of <BsonDocument/></returns>
        public List<BsonDocument>? ListAllDatabases()
        {
            List<BsonDocument>? dbList;
            try
            {
                dbList = client.ListDatabases().ToList();
            }
            catch (Exception e)
            {
                _logger.WriteLog(LogType.Error, "User: " + username + " has failed to load Dashboard ", e);
                return null;
            }

            return dbList;
        }

        /// <summary>
        /// List every Collection from specific database
        /// </summary>
        /// <param name="dbName">Database Name</param>
        /// <returns>Returns List <BsonDocument/> </returns>
        public List<BsonDocument>? ListAllCollectionsFromDb(string dbName)
        {
            List<BsonDocument>? result;
            try
            {
                result = client.GetDatabase(dbName).ListCollections().ToList();
            }
            catch (Exception e)
            {
                _logger.WriteLog(LogType.Error, "User: " + username + " has failed to load the all Collections from DB: " + dbName, e);
                result = null;
            }

            return result;
        }

        /// <summary>
        /// Get the number of Collections
        /// </summary>
        /// <param name="dbName">Database Name</param>
        /// <returns>If the result is -1, user is not authenticated</returns>
        public int GetNumberOfCollections(string dbName)
        {
            try
            {
                return client.GetDatabase(dbName).ListCollections().ToList().Count;
            }
            catch
            {
                _logger.WriteLog(LogType.Error, "User: " + username + " has failed to get the collection number of DB:" + dbName);
                return -1;
            }
        }

        /// <summary>
        /// Get a specific Collection from a Database
        /// </summary>
        /// <param name="dbName">Database Name</param>
        /// <param name="collectionName">Collection Name</param>
        /// <returns>Returns List<String />
        /// </returns>
        public List<string>? GetCollection(string dbName, string collectionName)
        {
            List<string> result = new();
            try
            {
                var collection = client.GetDatabase(dbName).GetCollection<BsonDocument>(collectionName);

                if (collection is null)
                    return null;

                var filter = new BsonDocument();
                var cursor = collection.Find(filter).ToCursor();
                while (cursor.MoveNext())
                {
                    result.AddRange(cursor.Current.Select(document => document.ToJson()));
                }
                return result;
            }
            catch (Exception e)
            {
                _logger.WriteLog(LogType.Error, "User: " + username + " has failed to load the Collection: " + collectionName + " from DB: " + dbName, e);
                return null;
            }
        }

        /// <summary>
        /// Get all available attributes from a collection in a database.
        /// </summary>
        /// <param name="dbName">Database Name</param>
        /// <param name="collectionName">Collection Name</param>
        /// <returns>List of attribute names</returns>
        public List<string> GetCollectionAttributes(string? dbName, string? collectionName)
        {
            List<string> keyList = new();

            try
            {
                var db = client.GetDatabase(dbName);
                var collection = db.GetCollection<BsonDocument>(collectionName);

                var filter = new BsonDocument();
                var cursor = collection.Find(filter).Limit(1);

                if (cursor.Any())
                {
                    var document = cursor.First();
                    keyList.AddRange(document.Elements.Select(element => element.Name));
                }
            }
            catch (Exception e)
            {
                _logger.WriteLog(LogType.Error, $"User: {username} has failed to load attributes from Collection: {collectionName} in DB: {dbName}", e);
                return new List<string>();
            }
            return keyList;
        }

        public async Task<bool> RenameAttributeInCollectionAsync(string dbName, string collectionName, Dictionary<string, string>? renameMap)
        {
            try
            {
                var db = client.GetDatabase(dbName);
                var collection = db.GetCollection<BsonDocument>(collectionName);

                if (renameMap == null) return true;
                var updateDefs = renameMap.Select(entry =>
                    Builders<BsonDocument>.Update.Rename(entry.Key, entry.Value)
                ).ToArray();

                var combined = Builders<BsonDocument>.Update.Combine(updateDefs);

                await collection.UpdateManyAsync(new BsonDocument(), combined);

                return true;
            }
            catch (Exception e)
            {
                _logger.WriteLog(LogType.Error, "User: " + username + " has failed to change the attributes in " + collectionName + " from DB: " + dbName, e);
                return false;
            }
        }

        public long GetTotalCount(string dbName, string collectionName, string selectedKey, string searchValue)
        {
            var filter = Builders<BsonDocument>.Filter.Empty;

            var database = client.GetDatabase(dbName);
            var collection = database.GetCollection<BsonDocument>(collectionName);

            if (string.IsNullOrWhiteSpace(searchValue)) return collection.CountDocuments(filter);
            if (!string.IsNullOrWhiteSpace(selectedKey))
            {
                filter = Builders<BsonDocument>.Filter.Regex(selectedKey, new BsonRegularExpression(searchValue, "i"));
            }
            else
            {
                var fieldNames = collection.Find(new BsonDocument()).Limit(1).FirstOrDefault()?.Names.ToList();
                var filters = new List<FilterDefinition<BsonDocument>>();

                if (fieldNames != null)
                {
                    filters.AddRange(fieldNames.Select(field => Builders<BsonDocument>.Filter.Regex(field, new BsonRegularExpression(searchValue, "i"))));
                }

                filter = Builders<BsonDocument>.Filter.Or(filters);
            }

            return collection.CountDocuments(filter);
        }

        public List<BsonDocument> GetCollection(string dbName, string collectionName, int skip, int limit, string selectedKey, string searchValue)
        {
            var filter = Builders<BsonDocument>.Filter.Empty;

            var database = client.GetDatabase(dbName);
            var collection = database.GetCollection<BsonDocument>(collectionName);

            if (string.IsNullOrWhiteSpace(searchValue))
                return collection.Find(filter).Skip(skip).Limit(limit).ToList().Select(doc => doc).ToList();
            if (!string.IsNullOrWhiteSpace(selectedKey))
            {
                filter = Builders<BsonDocument>.Filter.Regex(selectedKey, new BsonRegularExpression(searchValue, "i"));
            }
            else
            {
                var fieldNames = collection.Find(new BsonDocument()).Limit(1).FirstOrDefault()?.Names.ToList();
                var filters = new List<FilterDefinition<BsonDocument>>();

                if (fieldNames != null)
                {
                    filters.AddRange(fieldNames.Select(field => Builders<BsonDocument>.Filter.Regex(field, new BsonRegularExpression(searchValue, "i"))));
                }

                filter = Builders<BsonDocument>.Filter.Or(filters);
            }

            return collection.Find(filter).Skip(skip).Limit(limit).ToList().Select(doc => doc).ToList();
        }

        public int GetCollectionCount(string dbName, string collectionName, string selectedKey, string searchValue)
        {
            var filter = Builders<BsonDocument>.Filter.Empty;
            if (!string.IsNullOrWhiteSpace(selectedKey) && !string.IsNullOrWhiteSpace(searchValue))
            {
                filter = Builders<BsonDocument>.Filter.Regex(selectedKey, new BsonRegularExpression(searchValue, "i"));
            }

            var database = client.GetDatabase(dbName);
            var collection = database.GetCollection<BsonDocument>(collectionName);
            return (int)collection.CountDocuments(filter);
        }

        public async Task<bool> InsertDocumentAsync(string dbName, string collectionName, dynamic document)
        {
            try
            {
                var database = client.GetDatabase(dbName);
                var collection = database.GetCollection<BsonDocument>(collectionName);
                var doc = BsonDocument.Parse(JObject.FromObject(document).ToString());

                await collection.InsertOneAsync(doc);
                _logger.WriteLog(LogType.Info, "User: " + username + " has inserted a document into DB: " + dbName);

                return true;
            }
            catch (Exception e)
            {
                _logger.WriteLog(LogType.Error, "User: " + username + " failed to insert document into DB: " + dbName + " " + e);
                return false;
            }
        }

        public bool DeleteAllDatabases()
        {
            var overallResult = true;
            try
            {
                var databaseNames = client.ListDatabaseNames().ToList();
                foreach (var unused in databaseNames.Where(dbName => dbName != "admin" && dbName != "config" && dbName != "local").Where(dbName => !DeleteDb(dbName)))
                {
                    overallResult = false;
                }
                _logger.WriteLog(LogType.Info, "User: " + username + " has deleted all databases.");
            }
            catch (Exception e)
            {
                _logger.WriteLog(LogType.Error, "User: " + username + " has failed to delete all databases. " + e);
                overallResult = false;
            }

            return overallResult;
        }

        /// <summary>
        /// Delete a specific Collection from a Database
        /// </summary>
        /// <param name="dbName">Database Name</param>
        /// <returns>bool will be returned, if success</returns>
        public bool DeleteDb(string dbName)
        {
            bool result;
            try
            {
                client.DropDatabase(dbName);
                _logger.WriteLog(LogType.Info, "User: " + username + " has deleted the DB: " + dbName);
                result = true;
            }
            catch (Exception e)
            {
                _logger.WriteLog(LogType.Error, "User: " + username + " has failed to delete the DB: " + dbName + " " + e);
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Create a new Collection
        /// </summary>
        /// <param name="dbName">Database name</param>
        /// <param name="collectionName">Collection name</param>
        /// <returns>bool will be returned, if success</returns>
        public bool CreateCollection(string dbName, string collectionName)
        {
            bool result;
            try
            {
                client.GetDatabase(dbName).CreateCollection(collectionName);
                _logger.WriteLog(LogType.Info, "User: " + username + " has created the Collection: " + collectionName + " in DB: " + dbName);
                result = true;
            }
            catch (Exception e)
            {
                _logger.WriteLog(LogType.Error, "User: " + username + " has failed by creating Collection " + collectionName + " in DB: " + dbName, e);
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Delete a specific Collection
        /// </summary>
        /// <param name="dbName">Database Name</param>
        /// <param name="collectionName">Collection Name</param>
        /// <returns>bool will be returned, if success</returns>
        public bool DeleteCollection(string dbName, string collectionName)
        {
            bool result;
            try
            {
                var db = client.GetDatabase(dbName);
                db.DropCollection(collectionName);
                _logger.WriteLog(LogType.Info, "User: " + username + " has deleted the Collection: " + collectionName + " in DB: " + dbName);
                result = true;
            }
            catch (Exception e)
            {
                _logger.WriteLog(LogType.Error, "User: " + username + " has failed by deleting Collection" + collectionName + " in DB: " + dbName, e);
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Get all statistics of a specific MongoDB database
        /// </summary>
        /// <param name="dbName">Database Name</param>
        /// <returns>A BsonDocument containing all the database statistics or null if failed</returns>
        public BsonDocument? GetDatabaseStatistics(string dbName)
        {
            try
            {
                var db = client.GetDatabase(dbName);
                var command = new BsonDocument { { "dbStats", 1 }, { "scale", 1 } };
                var stats = db.RunCommand<BsonDocument>(command);
                return stats;
            }
            catch (Exception e)
            {
                _logger.WriteLog(LogType.Error, "User: " + username + " failed to fetch statistics for DB: " + dbName, e);
                return null;
            }
        }

        /// <summary>
        /// Get all statistics of a specific MongoDB collection
        /// </summary>
        /// <param name="dbName">Database Name</param>
        /// <param name="collectionName">Collection Name</param>
        /// <returns>A BsonDocument containing all the collection statistics or null if failed</returns>
        public BsonDocument? GetCollectionStatistics(string dbName, string collectionName)
        {
            try
            {
                var db = client.GetDatabase(dbName);
                db.GetCollection<BsonDocument>(collectionName);
                var command = new BsonDocument { { "collStats", collectionName } };
                var stats = db.RunCommand<BsonDocument>(command);
                return stats;
            }
            catch (Exception e)
            {
                _logger.WriteLog(LogType.Error, "User: " + username + " failed to fetch statistics for collection: " + collectionName, e);
                return null;
            }
        }

        /// <summary>
        /// Get global statistics of the MongoDB instance
        /// </summary>
        /// <returns>A BsonDocument containing all the server statistics or null if failed</returns>
        public BsonDocument? GetGlobalStatistics()
        {
            try
            {
                var db = client.GetDatabase("admin");
                var command = new BsonDocument { { "serverStatus", 1 } };
                var stats = db.RunCommand<BsonDocument>(command);
                return stats;
            }
            catch (Exception e)
            {
                _logger.WriteLog(LogType.Error, "User: " + username + " failed to fetch global statistics.", e);
                return null;
            }
        }

        ///<summary>
        ///Check if database already exist
        /// </summary>
        /// <param name="dbName">Database name</param>
        /// <returns>bool will be returned</returns>
        public bool CheckIfDbExist(string dbName)
        {
            bool result;
            try
            {
                var filter = new BsonDocument("name", dbName);
                var options = new ListDatabasesOptions { Filter = filter };
                var list = client.ListDatabases(options).ToList();
                result = list.Count != 0;
            }
            catch (Exception e)
            {
                _logger.WriteLog(LogType.Error, "User: " + username + " has failed to check if DB: " + dbName + " exist", e);
                result = false;
            }

            return result;
        }

        public async Task<bool> UploadJsonAsync(string dbName, string collectionName, JToken? json, bool adaptOid)
        {
            try
            {
                var db = client.GetDatabase(dbName);
                var collection = db.GetCollection<BsonDocument>(collectionName);

                if (json is JArray jsonArray)
                {
                    await ProcessJsonArray(collection, jsonArray, adaptOid);
                }
                else
                {
                    var document = ParseJsonToken(json, adaptOid);
                    await collection.InsertOneAsync(document);
                }
                return true;
            }
            catch (Exception e)
            {
                _logger.WriteLog(LogType.Error, $"User: {username} has failed to upload Json {dbName}, Collection: {collectionName} {e}");
                return false;
            }
        }

        private async Task ProcessJsonArray(IMongoCollection<BsonDocument> collection, JArray jsonArray, bool adaptOid)
        {
            var batch = new List<BsonDocument>();
            foreach (var jToken in jsonArray)
            {
                var document = ParseJsonToken(jToken, adaptOid);
                batch.Add(document);

                if (batch.Count < ConfigManager.batchCount) continue;
                await collection.InsertManyAsync(batch);
                batch.Clear();
            }

            if (batch.Count > 0)
            {
                await collection.InsertManyAsync(batch);
            }
        }

        private BsonDocument ParseJsonToken(JToken? token, bool adaptOid)
        {
            var jsonObject = JObject.Parse(token?.ToString() ?? string.Empty);
            if (!adaptOid && jsonObject.ContainsKey("_id"))
            {
                jsonObject.Remove("_id");
            }
            return BsonDocument.Parse(jsonObject.ToString());
        }


        /// <summary>
        /// Execute a MongoDB query
        /// </summary>
        /// <param name="dbName">Database name</param>
        /// <param name="collectionName">Collection name</param>
        /// <param name="query">MongoDB query string</param>
        /// <returns>String containing the result of the query</returns>
        public async Task<string> ExecuteMongoQuery(string dbName, string collectionName, string query)
        {
            string result;
            try
            {
                var db = client.GetDatabase(dbName);
                var collection = db.GetCollection<BsonDocument>(collectionName);

                var filter = BsonDocument.Parse(query);
                var findResult = await collection.Find(filter).ToListAsync();

                result = Newtonsoft.Json.JsonConvert.SerializeObject(findResult);

                _logger.WriteLog(LogType.Info, $"User: {username} has executed the query: {query} on DB: {dbName}, Collection: {collectionName}");
            }
            catch (Exception e)
            {
                _logger.WriteLog(LogType.Error, $"User: {username} has failed to execute the query: {query} on DB: {dbName}, Collection: {collectionName} {e}");
                result = "Error: " + e.Message;
            }

            return result;
        }

        /// <summary>
        /// Get all collections from a Database, the return has every data from every collection in the DB
        /// </summary>
        /// <param name="dbName">Database Name</param>
        /// <returns>Returns JObject?</returns>
        public async Task StreamAllCollectionExport(StreamWriter writer, string dbName, Guid guid)
        {
            try
            {
                var progress = 0;
                var db = client.GetDatabase(dbName);
                var collections = db.ListCollections().ToList();

                int totalCollections = collections.Count;
                int processedCollections = 0;

                await writer.WriteAsync("{\"" + dbName + "\":{");

                var jsonWriterSettings = new JsonWriterSettings { OutputMode = JsonOutputMode.CanonicalExtendedJson };
                bool isFirstCollection = true;

                foreach (var collection in collections)
                {
                    progress = (int)((double)processedCollections / totalCollections * 100);
                    await _hubContext!.Clients.All.SendAsync("ReceiveProgressDatabase", totalCollections, processedCollections, progress, guid.ToString(), "download");
                    if (!isFirstCollection)
                    {
                        await writer.WriteAsync(",");
                    }

                    var collectionName = collection["name"].AsString;
                    await writer.WriteAsync($"\"{collectionName}\": [");

                    var collectionData = db.GetCollection<BsonDocument>(collectionName);
                    var filter = new BsonDocument();

                    using (var cursor = collectionData.Find(filter).ToCursor())
                    {
                        bool isFirstDocument = true;
                        while (await cursor.MoveNextAsync())
                        {
                            foreach (var document in cursor.Current)
                            {
                                if (!isFirstDocument)
                                {
                                    await writer.WriteAsync(",");
                                }

                                var documentJson = document.ToJson(jsonWriterSettings);
                                await writer.WriteAsync(documentJson);

                                isFirstDocument = false;
                            }
                        }
                    }

                    await writer.WriteAsync("]");
                    isFirstCollection = false;
                    processedCollections++;
                }
                progress = (int)((double)processedCollections / totalCollections * 100);
                await _hubContext!.Clients.All.SendAsync("ReceiveProgressDatabase", totalCollections, processedCollections, progress, guid.ToString(), "download");
                await writer.WriteAsync("}}");
            }
            catch (Exception e)
            {
                _logger.WriteLog(LogType.Error, "User: " + username + " has failed to load the All Collections from DB: " + dbName, e);
            }
        }

        public async Task<bool> UpdateMongoDb(string dbName, string collectionName, Dictionary<string, object>? differences, Dictionary<string, string>? renameMap, string id)
        {
            try
            {
                var database = client.GetDatabase(dbName);
                var collection = database.GetCollection<BsonDocument>(collectionName);

                var filter = Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(id));
                var updateDef = Builders<BsonDocument>.Update;

                if (differences != null && differences.ContainsKey("") && differences[""] is JObject replacementDoc)
                {
                    var replacementBson = BsonDocument.Parse(replacementDoc.ToString());
                    await collection.ReplaceOneAsync(filter, replacementBson);
                    return true;
                }

                if (renameMap != null)
                    foreach (var renameDefinition in renameMap.Select(rename =>
                                 updateDef.Rename(rename.Key, rename.Value)))
                    {
                        await collection.UpdateOneAsync(filter, renameDefinition);
                    }

                if (differences == null) return true;
                foreach (var (key, value) in differences)
                {
                    var bsonValue = value switch
                    {
                        JObject jObject => BsonDocument.Parse(jObject.ToString()),
                        null => BsonNull.Value,
                        _ => ConvertToBsonValue(value)
                    };

                    var updateDefinition = updateDef.Set(key, bsonValue);
                    await collection.UpdateOneAsync(filter, updateDefinition);
                }

                return true;
            }
            catch (Exception e)
            {
                _logger.WriteLog(LogType.Error, $"User: {username} has failed to update DB: {dbName}, Collection: {collectionName} {e}");
                return false;
            }
        }

        private BsonValue ConvertToBsonValue(object value)
        {
            switch (value)
            {
                case JObject jObject:
                    return BsonDocument.Parse(jObject.ToString());

                case JArray jArray:
                    return ConvertJArrayToBsonArray(jArray);

                case JValue jValue:
                    return ConvertJValueToBsonValue(jValue);

                case null:
                    return BsonNull.Value;

                default:
                    var bsonValue = BsonValue.Create(value);
                    return bsonValue;
            }
        }

        private BsonArray ConvertJArrayToBsonArray(JArray jArray)
        {
            var bsonArray = new BsonArray();
            foreach (var item in jArray)
            {
                bsonArray.Add(ConvertToBsonValue(item));
            }
            return bsonArray;
        }

        private static BsonValue ConvertJValueToBsonValue(JValue jValue)
        {
            return BsonValue.Create(jValue.Value);
        }
    }
}
