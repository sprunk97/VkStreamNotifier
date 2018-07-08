using System;

namespace VkStreamNotifier
{
    class Program
    {
        private static Settings settings;

        static void Main(string[] args)
        {
            Console.WriteLine("Commands: load, connect");
            do
            {
                switch (Console.ReadLine())
                {
                    case "exit":
                        Environment.Exit(0);
                        break;
                    case "load":
                        Load();
                        break;
                    case "connect":
                        Connect();
                        break;
                    default:
                        Console.WriteLine("Unrecognized command. List of commands: load, connect");
                        break;
                }
            }
            while (true);
        }

        static void Load()
        {
            settings = SettingsReader.LoadSettings();

            Console.WriteLine("\tCurrent settings:");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            foreach (var property in typeof(Settings).GetProperties())
                Console.WriteLine($"{property.Name} : {property.GetValue(settings, null)}");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine();
        }

        static void Connect()
        {
            Twitch twitch = new Twitch(settings);
            twitch.CreateConnection();
        }
    }
}
