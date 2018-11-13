using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using NLog;
using VkStreamNotifier.Schemes;

namespace VkStreamNotifier
{
    public static class SettingsWorker
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Returns list of streamers info
        /// </summary>
        /// <returns></returns>
        public static List<Streamer> GetStreamersList()
        {
            log.Info("Getting streamers");
            MongoClient client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("notifier");
            var collection = database.GetCollection<BsonDocument>("streamers");

            log.Info("Serializing streamer class");
            List<Streamer> streamers = new List<Streamer>();
            var rawStreamersInfo = collection.Find(new BsonDocument()).ToList();
            foreach (var doc in rawStreamersInfo)
                streamers.Add(BsonSerializer.Deserialize<Streamer>(doc));
            return streamers;
        }

        /// <summary>
        /// Returns list of application credentials
        /// </summary>
        /// <returns></returns>
        public static List<Credentials> GetCredentials()
        {
            log.Info("Getting credentials");
            MongoClient client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("notifier");
            var collection = database.GetCollection<BsonDocument>("credentials");

            log.Info("Serializing credentials class");
            List<Credentials> credentials = new List<Credentials>();
            var rawCredentials = collection.Find(new BsonDocument()).ToList();
            foreach (var doc in rawCredentials)
                credentials.Add(BsonSerializer.Deserialize<Credentials>(doc));
            return credentials;
        }

        /// <summary>
        /// Updates time when streamer went offline
        /// </summary>
        /// <param name="stream_ended"></param>
        /// <param name="twitch_username"></param>
        /// <returns></returns>
        public static void UpdateDowntime(DateTime stream_ended, string twitch_username)
        {
            log.Info("Updating downtime");
            MongoClient client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("notifier");
            var collection = database.GetCollection<Streamer>("streamers");

            var filter = Builders<Streamer>.Filter.Eq("twitch_username", twitch_username);
            var update = Builders<Streamer>.Update.Set(x => x.stream_ended, stream_ended);
            collection.UpdateOne(filter, update);
        }

        /// <summary>
        /// Updates date when last notification was sent
        /// </summary>
        /// <param name="stream_ended"></param>
        /// <param name="twitch_username"></param>
        public static void UpdateLastNotificationDate(DateTime notification_sent, string twitch_username)
        {
            log.Info("Updating last notification date");
            MongoClient client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("notifier");
            var collection = database.GetCollection<Streamer>("streamers");

            var filter = Builders<Streamer>.Filter.Eq("twitch_username", twitch_username);
            var update = Builders<Streamer>.Update.Set(x => x.stream_ended, notification_sent);
            collection.UpdateOne(filter, update);
        }
    }
}
