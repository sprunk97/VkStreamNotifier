using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using VkNet;
using VkNet.Model;

namespace VkStreamNotifier
{
    class VK
    {
        private VkApi api = new VkApi();
        private List<Schemes.Streamer> streamers;
        private Schemes.Credentials credentials;
        public bool IsAuthorized { get; private set; } = false;

        public VK() { }
        public VK(Schemes.Credentials credentials, List<Schemes.Streamer> streamers)
        {
            this.credentials = credentials;
            this.streamers = streamers;
        }

        /// <summary>
        /// Authorizing VK api
        /// </summary>
        public async void Connect()
        {
            await api.AuthorizeAsync(new ApiAuthParams
            {
                ApplicationId = ulong.Parse(credentials.vk_app_id),
                Settings = VkNet.Enums.Filters.Settings.All,
                AccessToken = credentials.vk_app_token
            });
            IsAuthorized = true;
        }

        /// <summary>
        /// Returns string in json
        /// </summary>
        /// <returns></returns>
        public string CreateJson(Schemes.Streamer streamer)
        {
            Schemes.NotifyMessage message = new Schemes.NotifyMessage()
            {
                message = new Schemes.Message() { message = streamer.message },
                list_ids = streamer.list_ids,
                run_now = "1",
                access_token = credentials.vk_app_token
            };
            return JsonConvert.SerializeObject(message);
        }

        /// <summary>
        /// Performs POST request to VK server
        /// </summary>
        public void SendNotify(string username)
        {
            var streamer = streamers.Find(x => x.twitch_username.Equals(username));
            WebRequest request = WebRequest.Create("https://broadcast.vkforms.ru/api/v2/broadcast?token=" + streamer.vk_api_token);
            request.Method = "POST";
            request.ContentType = "application/json";

            var json = CreateJson(streamer);

            byte[] byteArray = Encoding.UTF8.GetBytes(json);
            request.ContentLength = byteArray.Length;

            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            Console.WriteLine("\rSending request...");
            WebResponse response = request.GetResponse();
            if (((HttpWebResponse)response).StatusDescription == "OK")
                Console.ForegroundColor = ConsoleColor.Green;
            else Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine(((HttpWebResponse)response).StatusDescription);

            Console.ForegroundColor = ConsoleColor.Gray;
            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();

            Console.WriteLine(responseFromServer);
            
            reader.Close();
            dataStream.Close();
            response.Close();
        }
    }
}
