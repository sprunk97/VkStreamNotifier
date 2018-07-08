namespace VkStreamNotifier
{
    /// <summary>
    /// Main json file to send
    /// </summary>
    class NotifyMessage
    {
        public Message message { get; set; }
        public string user_ids { get; set; }
        public string list_ids { get; set; }
        public string run_now { get; set; }
        public string access_token { get; set; }
        public string run_at { get; set; }
    }
}
