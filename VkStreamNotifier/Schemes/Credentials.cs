using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace VkStreamNotifier.Schemes
{
    /// <summary>
    /// API's credentials
    /// </summary>
    class Credentials
    {
        [BsonId]
        public ObjectId _id { get; set; }

        [BsonElement("twitch_id")]
        public string twitch_id { get; set; }

        [BsonElement("twitch_token")]
        public string twitch_token { get; set; }

        [BsonElement("vk_app_id")]
        public string vk_app_id { get; set; }

        [BsonElement("vk_app_token")]
        public string vk_app_token { get; set; }

        [BsonElement("email")]
        public string email { get; set; }

        [BsonElement("email_password")]
        public string email_password { get; set; }
    }
}
