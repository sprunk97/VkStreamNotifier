using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace VkStreamNotifier.Schemes
{
    /// <summary>
    /// Contains streamer info
    /// </summary>
    public class Streamer
    {
        [BsonId]
        public ObjectId _id { get; set; }

        [BsonElement("twitch_username")]
        public string twitch_username { get; set; }

        [BsonElement("message")]
        public string message { get; set; }

        [BsonElement("list_ids")]
        public string list_ids { get; set; }

        [BsonElement("vk_api_token")]
        public string vk_api_token { get; set; }

        [BsonElement("vk_app_token")]
        public string vk_app_token { get; set; }

        [BsonElement("stream_ended")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime stream_ended { get; set; }

        [BsonElement("notification_sent")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime notification_sent { get; set; }
    }
}
