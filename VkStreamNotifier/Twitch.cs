using NLog;
using System;
using System.Collections.Generic;
using TwitchLib.Api;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using VkStreamNotifier.Schemes;

namespace VkStreamNotifier
{
    class Twitch
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        private readonly List<Streamer> streamers;
        private readonly Credentials credentials;
        private TwitchAPI api;

        public Twitch() { }
        public Twitch(Credentials credentials, List<Streamer> streamers)
        {
            this.credentials = credentials;
            this.streamers = streamers;
        }

        /// <summary>
        /// Creates twitch api and monitor instance
        /// </summary>
        public void CreateConnection()
        {
            TwitchClient client = new TwitchClient();
            ConnectionCredentials credentialConnection = new ConnectionCredentials(credentials.irc_username, credentials.irc_token);
            client.Initialize(credentialConnection, credentials.irc_username);
            client.OnConnected += Client_OnConnected;
            client.OnDisconnected += Client_OnDisconnected;// new EventHandler<TwitchLib.Client.Events.OnDisconnectedArgs> (Client_OnDisconnected);
            client.Connect();
        }

        private void Client_OnDisconnected(object sender, TwitchLib.Communication.Events.OnDisconnectedEventArgs e)
        {
            log.Warn("IRC client disconnected");
            Monitor.EndMonitor();
        }

        private void Client_OnConnected(object sender, TwitchLib.Client.Events.OnConnectedArgs e)
        {
            log.Info("IRC client connected");
            api = new TwitchAPI();
            api.Settings.ClientId = credentials.twitch_id;
            api.Settings.AccessToken = credentials.twitch_token;
            
            Monitor.GetInstance(credentials, streamers, api);
            Monitor.StartMonitor();
        }
    }
}
