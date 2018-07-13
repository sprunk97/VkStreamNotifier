using System;
using System.Collections.Generic;
using TwitchLib.Api;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;
using VkStreamNotifier.Schemes;

namespace VkStreamNotifier
{
    class Monitor
    {
        private LiveStreamMonitor monitor;
        private Settings settings;
        private static Monitor instance;
        private VK vk;

        public Monitor() { }
        protected Monitor(Settings settings, TwitchAPI api)
        {
            this.settings = settings;
            Inititalize(api);
        }

        public static Monitor GetInstance(Settings settings, TwitchAPI api)
        {
            if (instance == null)
                instance = new Monitor(settings, api);
            else Console.WriteLine("Already connected");
            return instance;
        }

        private void Inititalize(TwitchAPI api)
        {
            var userId = api.Users.v5.GetUserByNameAsync(settings.twitch_username).Result.Matches[0].Id;
            monitor = new LiveStreamMonitor(api);
            monitor.OnStreamOnline += new EventHandler<OnStreamOnlineArgs>(OnStreamOnline);
            monitor.OnStreamOffline += new EventHandler<OnStreamOfflineArgs>(OnStreamOffline);
            monitor.OnStreamMonitorStarted += new EventHandler<OnStreamMonitorStartedArgs>(OnMonitorStarted);
            monitor.OnStreamMonitorEnded += new EventHandler<OnStreamMonitorEndedArgs>(OnMonitorEnded);

            monitor.CheckIntervalSeconds = 60;
            monitor.SetStreamsByUserId(new List<string>() { userId });
            monitor.StartService();
        }

        private void OnStreamOffline(object sender, OnStreamOfflineArgs e)
        {
            Console.WriteLine($"{DateTime.Now} {e.Channel} ended stream");
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
            vk = new VK(settings);
            vk.Connect();
        }

        private void OnStreamOnline(object sender, OnStreamOnlineArgs e)
        {
            Console.WriteLine($"{DateTime.Now} {e.Channel} started stream");
            if (vk.IsAuthorized) vk.SendNotify();
            else vk.Connect();
        }
    }
}
