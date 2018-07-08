using System.IO;

using Newtonsoft.Json;

namespace VkStreamNotifier
{
    class SettingsReader
    {
        /// <summary>
        /// Reads settings from json-file and return object of Settings class
        /// </summary>
        /// <returns></returns>
        static public Settings LoadSettings()
        {
            StreamReader streamReader = new StreamReader("credentials.json");
            JsonSerializer serializer = new JsonSerializer();

            JsonTextReader reader = new JsonTextReader(streamReader);
            var settings = serializer.Deserialize<Settings>(reader);

            return settings;
        }
    }
}
