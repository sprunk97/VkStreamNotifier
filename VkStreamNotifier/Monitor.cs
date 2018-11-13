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
            foreach (var streamer in streamers)
                userIds.Add(api.Users.v5.GetUserByNameAsync(streamer.twitch_username).Result.Matches[0].Id);

            monitor = new LiveStreamMonitor(api, 60, true, false);
            monitor.OnStreamOnline += new EventHandler<OnStreamOnlineArgs>(OnStreamOnline);
            monitor.OnStreamOffline += new EventHandler<OnStreamOfflineArgs>(OnStreamOffline);
            monitor.OnStreamMonitorStarted += new EventHandler<OnStreamMonitorStartedArgs>(OnMonitorStarted);
            monitor.OnStreamMonitorEnded += new EventHandler<OnStreamMonitorEndedArgs>(OnMonitorEnded);

            monitor.SetStreamsByUserId(userIds);
        }

        /// <summary>
        /// 
        /// </summary>
        public static void StartMonitor()
        {
            instance.monitor.StartService();
        }

        /// <summary>
        /// Ends monitor and calls dispose method for VK
        /// </summary>
        public static void EndMonitor()
        {
            instance.monitor.StopService();
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

        private void OnMonitorEnded(object sender, OnStreamMonitorEndedArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            logger.Warn($"Monitor ended");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private void OnMonitorStarted(object sender, OnStreamMonitorStartedArgs e)
        {
            logger.Info($"Monitor started");
            Console.WriteLine($"Current offline streams amount: {monitor.CurrentOfflineStreams.Count}");
            Console.WriteLine($"Current live streams amount: {monitor.CurrentLiveStreams.Count}");
            foreach (var streamer in streamers)
            {
                vkList.Add(new VK(credentials, streamer));
                vkList.Last().Connect();
            }
        }

        private void OnStreamOnline(object sender, OnStreamOnlineArgs e)
        {
            logger.Info($"{DateTime.Now} {e.Channel} started stream\r");

            if (vkList.Find(x => x.streamer.twitch_username.Equals(e.Channel)).streamer.stream_ended.ToLocalTime().AddHours(1) < DateTime.Now &&
                vkList.Find(x => x.streamer.twitch_username.Equals(e.Channel)).streamer.notification_sent.ToLocalTime().AddHours(1) < DateTime.Now)
            {
                logger.Info($"No drops, sending notification");
                var vk = vkList.Find(x => x.streamer.twitch_username.Equals(e.Channel));
                vk.Notify();
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
