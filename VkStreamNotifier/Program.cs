using System;

namespace VkStreamNotifier
{
    class Program
    {
        static void Main(string[] args)
        {
            Settings settings = SettingsReader.LoadSettings();

            Console.WriteLine("\tCurrent settings:");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            foreach (var property in typeof(Settings).GetProperties())
                Console.WriteLine($"{property.Name} : {property.GetValue(settings, null)}");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine();

            Twitch twitch = new Twitch(settings);
            twitch.CreateConnection();

            if (Console.ReadLine() != "exit")
                Console.ReadKey();
        }
    }
}
