using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VkNet;
using VkNet.Model;

namespace VkStreamNotifier
{
    class VK
    {
        private const string path = "https://broadcast.vkforms.ru/api/v2/broadcast?token=";
        private readonly VkApi api = new VkApi();
        private readonly Schemes.Credentials credentials;
        public Schemes.Streamer streamer;

        public VK() { }
        public VK(Schemes.Credentials credentials, Schemes.Streamer streamer)
        {
            this.credentials = credentials;
            this.streamer = streamer;
        }

        /// <summary>
        /// Authorizing VK api
        /// </summary>
        public async Task ConnectAsync()
        {
            await api.AuthorizeAsync(new ApiAuthParams
            {
                ApplicationId = ulong.Parse(credentials.vk_app_id),
                Settings = VkNet.Enums.Filters.Settings.All,
                AccessToken = streamer.vk_app_token
            });
        }

        /// <summary>
        /// Returns string in json
        /// </summary>
        /// <returns></returns>
        public string CreateJson()
        {
            Schemes.NotifyMessage message = new Schemes.NotifyMessage()
            {
                message = new Schemes.Message() { message = streamer.message },
                list_ids = streamer.list_ids,
                run_now = "1",
                access_token = streamer.vk_app_token
            };
            return JsonConvert.SerializeObject(message);
        }

        /// <summary>
        /// Performs POST request to VK server
        /// </summary>
        public void SendNotify()
        {
            WebRequest request = WebRequest.Create(path + streamer.vk_api_token);
            request.Method = "POST";
            request.ContentType = "application/json";

            var json = CreateJson();

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
