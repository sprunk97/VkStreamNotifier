using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace VkStreamNotifier.Schemes
{
    /// <summary>
    /// Contains streamer info
    /// </summary>
    class Streamer
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
    }
}
