using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using TwitchLib.Api;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;
using TwitchLib.Api.Services.Events;
using VkStreamNotifier.Schemes;

namespace VkStreamNotifier
{
    class Monitor
    {
        private LiveStreamMonitorService monitor;
        private readonly List<Streamer> streamers;
        private readonly Credentials credentials;
        private static Monitor instance;
        private readonly List<VK> vkList = new List<VK>();
        private static Logger logger = LogManager.GetCurrentClassLogger();

        protected Monitor() { }
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

            monitor = new LiveStreamMonitorService(api);
            monitor.OnStreamOnline += new EventHandler<OnStreamOnlineArgs>(OnStreamOnline);
            monitor.OnStreamOffline += new EventHandler<OnStreamOfflineArgs>(OnStreamOffline);
            monitor.OnServiceStarted += new EventHandler<OnServiceStartedArgs>(OnMonitorStartedAsync);
            monitor.OnServiceStopped += new EventHandler<OnServiceStoppedArgs>(OnMonitorEnded);

            monitor.SetChannelsByName(streamers.Select(x => x.twitch_username).ToList());
        }

        /// <summary>
        /// 
        /// </summary>
        public static void StartMonitor()
        {
            instance.monitor.Start();
        }

        /// <summary>
        /// Ends monitor and calls dispose method for VK
        /// </summary>
        public static void EndMonitor()
        {
            instance.monitor.Stop();
            foreach (var streamer in instance.streamers)
            {
                instance.vkList.Find(x => x.streamer.twitch_username == streamer.twitch_username).Drop();
            }
            StartMonitor();
        }

        private void OnStreamOffline(object sender, OnStreamOfflineArgs e)
        {
            var current = DateTime.Now;
            logger.Info($"{current} {e.Channel} ended stream\r");
            vkList.Find(x => x.streamer.twitch_username.Equals(e.Channel)).streamer.stream_ended = current;
            SettingsWorker.UpdateDowntime(current, e.Channel);
            Console.WriteLine($"Updated date when {e.Channel} went offline: {current.ToString()}");
        }

        private void OnMonitorEnded(object sender, OnServiceStoppedArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            logger.Warn($"Monitor ended");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private async void OnMonitorStartedAsync(object sender, OnServiceStartedArgs e)
        {
            logger.Info($"Monitor started");
            await monitor.UpdateLiveStreamersAsync(false);
            Console.WriteLine($"Current live streams amount: {monitor.LiveStreams.Count}");
            Console.WriteLine($"Total streams amount: {monitor.ChannelsToMonitor.Count}");
            foreach (var streamer in streamers)
            {
                vkList.Add(new VK(credentials, streamer));
                vkList.Last().Connect();
            }
        }

        private void OnStreamOnline(object sender, OnStreamOnlineArgs e)
        {
            Console.WriteLine($"Current live streams amount: {monitor.LiveStreams.Count}");
            Console.WriteLine($"Total streams amount: {monitor.ChannelsToMonitor.Count}");
            logger.Info($"{DateTime.Now} {e.Channel} started stream\r");

            if (vkList.Find(x => x.streamer.twitch_username.Equals(e.Channel)).streamer.stream_ended.ToLocalTime().AddHours(1) < DateTime.Now &&
                vkList.Find(x => x.streamer.twitch_username.Equals(e.Channel)).streamer.notification_sent.ToLocalTime().AddHours(1) < DateTime.Now)
            {
                logger.Info($"No drops, sending notification");
                var vk = vkList.Find(x => x.streamer.twitch_username.Equals(e.Channel));
                vk.Notify(e.Stream.Title);
                SettingsWorker.UpdateLastNotificationDate(DateTime.Now, e.Channel);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                logger.Warn($"Seems like {e.Channel}'s stream dropped in last hour. Notification supressed");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
    }
}
