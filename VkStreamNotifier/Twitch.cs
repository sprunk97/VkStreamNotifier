using TwitchLib.Api;
using VkStreamNotifier.Schemes;

namespace VkStreamNotifier
{
    class Twitch
    {
        private Settings settings;

        public Twitch() { }
        public Twitch(Settings settings) => this.settings = settings;

        /// <summary>
        /// Creates twitch api and monitor instance
        /// </summary>
        public void CreateConnection()
        {
            TwitchAPI api = new TwitchAPI();
            api.Settings.ClientId = settings.twitch_id;
            api.Settings.AccessToken = settings.twitch_token;

            var monitor = Monitor.GetInstance(settings, api);
        }
    }
}
