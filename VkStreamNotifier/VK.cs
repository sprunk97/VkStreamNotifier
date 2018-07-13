using System;
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
        private Settings settings;
        public bool IsAuthorized { get; private set; } = false;

        public VK() { }
        public VK(Settings settings) => this.settings = settings;

        /// <summary>
        /// Authorizing VK api and invokes sending messages
        /// </summary>
        public async void Connect()
        {
            await api.AuthorizeAsync(new ApiAuthParams
            {
                ApplicationId = ulong.Parse(settings.vk_app_id),
                Settings = VkNet.Enums.Filters.Settings.All,
                AccessToken = settings.vk_app_token
            });
            IsAuthorized = true;
        }

        /// <summary>
        /// Returns string in json
        /// </summary>
        /// <returns></returns>
        public string CreateJson()
        {
            NotifyMessage message = new NotifyMessage()
            {
                message = new Message() { message = settings.message },
                list_ids = settings.list_ids,
                run_now = "1",
                access_token = settings.vk_app_token
            };
            return JsonConvert.SerializeObject(message);
        }

        /// <summary>
        /// Performs POST request to VK server
        /// </summary>
        public void SendNotify()
        {
            WebRequest request = WebRequest.Create("https://broadcast.vkforms.ru/api/v2/broadcast?token=" + settings.vk_api_token);
            request.Method = "POST";
            request.ContentType = "application/json";

            var json = CreateJson();

            byte[] byteArray = Encoding.UTF8.GetBytes(json);
            request.ContentLength = byteArray.Length;

            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            Console.ForegroundColor = ConsoleColor.Blue;
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
