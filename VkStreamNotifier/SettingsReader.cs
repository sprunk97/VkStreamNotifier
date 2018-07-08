using System.IO;

using Newtonsoft.Json;

namespace VkStreamNotifier
{
    class SettingsReader
    {
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
