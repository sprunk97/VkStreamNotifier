namespace VkStreamNotifier
{
    /// <summary>
    /// Contains properties of settings and credentials
    /// </summary>
    class Settings
    {
        public string twitch_id { get; set; }
        public string twitch_token { get; set; }
        public string twitch_username { get; set; }
        public string vk_app_id { get; set; }
        public string vk_app_token { get; set; }
        public string message { get; set; }
        public string list_ids { get; set; }
        public string vk_api_token { get; set; }
    }
}
