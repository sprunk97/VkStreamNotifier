using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using VkStreamNotifier.Schemes;

namespace VkStreamNotifier
{
    public static class SettingsReader
    {
        /// <summary>
        /// Returns list of streamers info
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Streamer>> GetStreamersListAsync()
        {
            MongoClient client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("notifier");
            var collection = database.GetCollection<BsonDocument>("streamers");

            List<Streamer> streamers = new List<Streamer>();
            var rawStreamersInfo = await collection.Find(new BsonDocument()).ToListAsync();
            foreach (var doc in rawStreamersInfo)
                streamers.Add(BsonSerializer.Deserialize<Streamer>(doc));

            return streamers;
        }

        /// <summary>
        /// Returns list of application credentials
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Credentials>> GetCredentialsAsync()
        {
            MongoClient client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("notifier");
            var collection = database.GetCollection<BsonDocument>("credentials");

            List<Credentials> credentials = new List<Credentials>();
            var rawCredentials = await collection.Find(new BsonDocument()).ToListAsync();
            foreach (var doc in rawCredentials)
                credentials.Add(BsonSerializer.Deserialize<Credentials>(doc));

            return credentials;
        }
    }
}
