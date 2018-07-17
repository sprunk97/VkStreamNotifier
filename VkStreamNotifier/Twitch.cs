﻿using System.Collections.Generic;
using TwitchLib.Api;
using VkStreamNotifier.Schemes;

namespace VkStreamNotifier
{
    class Twitch
    {
        private readonly List<Streamer> streamers;
        private readonly Credentials credentials;

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
            TwitchAPI api = new TwitchAPI();
            api.Settings.ClientId = credentials.twitch_id;
            api.Settings.AccessToken = credentials.twitch_token;

            Monitor.GetInstance(credentials, streamers, api);
        }
    }
}
