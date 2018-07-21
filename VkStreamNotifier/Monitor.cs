using System;
using System.Collections.Generic;
using System.Linq;
using TwitchLib.Api;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;
using VkStreamNotifier.Schemes;

namespace VkStreamNotifier
{
    class Monitor
    {
        private LiveStreamMonitor monitor;
        private readonly List<Streamer> streamers;
        private readonly Credentials credentials;
        private static Monitor instance;
        private readonly List<VK> vkList = new List<VK>();

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

            monitor = new LiveStreamMonitor(api, 60, true, false);
            monitor.OnStreamOnline += new EventHandler<OnStreamOnlineArgs>(OnStreamOnline);
            monitor.OnStreamOffline += new EventHandler<OnStreamOfflineArgs>(OnStreamOffline);
            monitor.OnStreamMonitorStarted += new EventHandler<OnStreamMonitorStartedArgs>(OnMonitorStarted);
            monitor.OnStreamMonitorEnded += new EventHandler<OnStreamMonitorEndedArgs>(OnMonitorEnded);

            monitor.SetStreamsByUserId(userIds);
            monitor.StartService();
        }

        private void OnStreamOffline(object sender, OnStreamOfflineArgs e)
        {
            Console.WriteLine($"{DateTime.Now} {e.Channel} ended stream\r");
            vkList.Find(x => x.streamer.twitch_username.Equals(e.Channel)).streamer.stream_ended = DateTime.Now;
        }

        private void OnMonitorEnded(object sender, OnStreamMonitorEndedArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{DateTime.Now} Monitor ended");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private async void OnMonitorStarted(object sender, OnStreamMonitorStartedArgs e)
        {
            Console.WriteLine($"{DateTime.Now} Monitor started");
            Console.WriteLine($"Current offline streams amount: {monitor.CurrentOfflineStreams.Count}");
            Console.WriteLine($"Current live streams amount: {monitor.CurrentLiveStreams.Count}");

            foreach (var streamer in streamers)
            {
                vkList.Add(new VK(credentials, streamer));
                await vkList.Last().ConnectAsync();
            }
        }

        private async void OnStreamOnline(object sender, OnStreamOnlineArgs e)
        {
            Console.WriteLine($"{DateTime.Now} {e.Channel} started stream\r");
            if (vkList.Find(x => x.streamer.twitch_username.Equals(e.Channel)).streamer.stream_ended == null)
            {
                var vk = vkList.Find(x => x.streamer.twitch_username.Equals(e.Channel));
                if (vk.IsAuthorized) vk.SendNotify();
                else
                {
                    await vk.ConnectAsync();
                    vk.SendNotify();
                }
            }
            if (vkList.Find(x => x.streamer.twitch_username.Equals(e.Channel)).streamer.stream_ended != null && vkList.Find(x => x.streamer.twitch_username.Equals(e.Channel)).streamer.stream_ended?.AddHours(1) < DateTime.Now)
            {
                var vk = vkList.Find(x => x.streamer.twitch_username.Equals(e.Channel));
                if (vk.IsAuthorized) vk.SendNotify();
                else
                {
                    await vk.ConnectAsync();
                    vk.SendNotify();
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
