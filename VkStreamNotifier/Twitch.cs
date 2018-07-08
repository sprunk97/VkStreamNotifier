using System;
using System.Collections.Generic;

using TwitchLib.Api;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;

namespace VkStreamNotifier
{
    class Twitch
    {
        private TwitchAPI api = new TwitchAPI();
        private LiveStreamMonitor monitor;
        private Settings settings;

        public Twitch() { }
        public Twitch(Settings settings) => this.settings = settings;

        public void CreateConnection()
        {
            api.Settings.ClientId = settings.twitch_id;
            api.Settings.AccessToken = settings.twitch_token;
            var userId = api.Users.v5.GetUserByNameAsync(settings.twitch_username).Result.Matches[0].Id;

            monitor = new LiveStreamMonitor(api);
            monitor.OnStreamOnline += new EventHandler<OnStreamOnlineArgs>(OnStreamOnline);
            monitor.OnStreamMonitorStarted += new EventHandler<OnStreamMonitorStartedArgs>(OnMonitorStarted);
            monitor.OnStreamMonitorEnded += new EventHandler<OnStreamMonitorEndedArgs>(OnMonitorEnded);

            monitor.CheckIntervalSeconds = 60;
            monitor.SetStreamsByUserId(new List<string>() { userId });
            monitor.StartService();
        }

        private void OnMonitorEnded(object sender, OnStreamMonitorEndedArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{DateTime.Now} Monitor ended");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private void OnMonitorStarted(object sender, OnStreamMonitorStartedArgs e)
        {
            Console.WriteLine($"{DateTime.Now} Monitor started");
        }

        private void OnStreamOnline(object sender, OnStreamOnlineArgs e)
        {
            Console.WriteLine($"{DateTime.Now} Stream started");
            VK vk = new VK(settings);
            vk.Connect();
        }
    }
}
