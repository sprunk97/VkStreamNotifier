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
        private List<Streamer> streamers;
        private Credentials credentials;
        private static Monitor instance;
        private VK vk;

        public Monitor() { }
        protected Monitor(Credentials credentials, List<Streamer> streamers, TwitchAPI api)
        {
            this.streamers = streamers;
            this.credentials = credentials;
            Inititalize(api);
        }

        public static Monitor GetInstance(Credentials credentials, List<Streamer> streamers, TwitchAPI api)
        {
            if (instance == null)
                instance = new Monitor(credentials, streamers, api);
            else Console.WriteLine("Already connected");
            return instance;
        }

        private void Inititalize(TwitchAPI api)
        {
            var userIds = new List<string>();
            foreach (var streamer in streamers)
                userIds.Add(api.Users.v5.GetUserByNameAsync(streamer.twitch_username).Result.Matches[0].Id);

            monitor = new LiveStreamMonitor(api);
            monitor.OnStreamOnline += new EventHandler<OnStreamOnlineArgs>(OnStreamOnline);
            monitor.OnStreamOffline += new EventHandler<OnStreamOfflineArgs>(OnStreamOffline);
            monitor.OnStreamMonitorStarted += new EventHandler<OnStreamMonitorStartedArgs>(OnMonitorStarted);
            monitor.OnStreamMonitorEnded += new EventHandler<OnStreamMonitorEndedArgs>(OnMonitorEnded);

            monitor.CheckIntervalSeconds = 60;
            monitor.SetStreamsByUserId(userIds);
            monitor.StartService();
        }

        private void OnStreamOffline(object sender, OnStreamOfflineArgs e)
        {
            Console.WriteLine($"{DateTime.Now} {e.Channel} ended stream\r");
            streamers.Find(x => x.twitch_username.Equals(e.Channel)).stream_ended = DateTime.Now;
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
            Console.WriteLine($"Current offline streams amount: {monitor.CurrentOfflineStreams.Count}");
            Console.WriteLine($"Current live streams amount: {monitor.CurrentLiveStreams.Count}");

            vk = new VK(credentials, streamers);
            vk.Connect();
        }

        private void OnStreamOnline(object sender, OnStreamOnlineArgs e)
        {
            Console.WriteLine($"{DateTime.Now} {e.Channel} started stream\r");
            if (streamers.Find(x => x.twitch_username.Equals(e.Channel)).stream_ended?.AddHours(1) < DateTime.Now)
            {
                if (vk.IsAuthorized) vk.SendNotify(e.Channel);
                else
                {
                    vk.Connect();
                    vk.SendNotify(e.Channel);
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Seems like {e.Channel}'s stream dropped in last hour. Notification supressed");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
    }
}
