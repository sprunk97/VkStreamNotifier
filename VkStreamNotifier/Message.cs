namespace VkStreamNotifier
{
    /// <summary>
    /// A part of the main json file
    /// </summary>
    class Message
    {
        public string message { get; set; }
        public string[] attachment { get; set; }
        public string[] images { get; set; }
    }
}
