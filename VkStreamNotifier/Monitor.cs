using NLog;
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
        private static Logger logger = LogManager.GetCurrentClassLogger();

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
            {
                logger.Info("Creating monitor instance");
                instance = new Monitor(credentials, streamers, api);
            }
            else
            {
                logger.Info("Getting monitor instance");
            }
            return instance;
        }

        private void Inititalize(TwitchAPI api)
        {
            logger.Info("Inititalizing monitor");
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

        private async void OnStreamOffline(object sender, OnStreamOfflineArgs e)
        {
            logger.Info($"{DateTime.Now} {e.Channel} ended stream\r");
            var current = DateTime.Now;
            vkList.Find(x => x.streamer.twitch_username.Equals(e.Channel)).streamer.stream_ended = current;
            var result = await SettingsWorker.UpdateDowntimeAsync(current, e.Channel);
            Console.WriteLine($"Found: {result.MatchedCount}. Updated: {result.ModifiedCount}");
            await Program.CallMenu();
            Console.ReadKey();
        }

        private async void OnMonitorEnded(object sender, OnStreamMonitorEndedArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            logger.Warn($"Monitor ended");
            Console.ForegroundColor = ConsoleColor.Gray;
            await Program.CallMenu();
            Console.ReadKey();
        }

        private async void OnMonitorStarted(object sender, OnStreamMonitorStartedArgs e)
        {
            logger.Info($"Monitor started");
            Console.WriteLine($"Current offline streams amount: {monitor.CurrentOfflineStreams.Count}");
            Console.WriteLine($"Current live streams amount: {monitor.CurrentLiveStreams.Count}");
            foreach (var streamer in streamers)
            {
                vkList.Add(new VK(credentials, streamer));
                await vkList.Last().ConnectAsync();
            }
            await Program.CallMenu();
            Console.ReadKey();
        }

        private async void OnStreamOnline(object sender, OnStreamOnlineArgs e)
        {
            logger.Info($"{DateTime.Now} {e.Channel} started stream\r");

            if (vkList.Find(x => x.streamer.twitch_username.Equals(e.Channel)).streamer.stream_ended.ToLocalTime().AddMinutes(30) < DateTime.Now)
            {
                logger.Info($"No drops, sending notification");
                var vk = vkList.Find(x => x.streamer.twitch_username.Equals(e.Channel));
                vk.Notify();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                logger.Warn($"Seems like {e.Channel}'s stream dropped in last hour. Notification supressed");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            await Program.CallMenu();
            Console.ReadKey();
        }
    }
}
