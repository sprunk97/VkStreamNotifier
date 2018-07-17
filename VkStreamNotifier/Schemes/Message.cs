namespace VkStreamNotifier.Schemes
{
    /// <summary>
    /// A part of the main json file
    /// </summary>
    public class Message
    {
        public string message { get; set; }
        public string[] attachment { get; set; }
        public string[] images { get; set; }
    }
}
