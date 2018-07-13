using System;
using System.Net;
using CrashReporter;

namespace VkStreamNotifier
{
    class Program
    {
        #region variables
        private static Settings settings;
        private static Twitch twitch;
        #endregion

        [STAThread]
        static void Main(string[] args)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(GlobalExceptionHandler);

            Console.WriteLine("Commands: load, connect, exit");
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
                        Console.WriteLine("Unrecognized command. List of commands: load, connect, exit");
                        break;
                }
            }
            while (true);
        }

        static void GlobalExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            NetworkCredential credential = new NetworkCredential(settings.email, settings.email_password);
            var mail = new Sender(credential, "sprunk97@gmail.com", "VkStreamNotifier Exception", null, null);
            try
            {
                mail.SendReport(e);
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }
        }

        static void Load()
        {
            settings = SettingsReader.LoadSettings();

            Console.WriteLine("\tCurrent settings:");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            foreach (var property in typeof(Settings).GetProperties())
                if (property.Name != "email" && property.Name != "email_password")
                    Console.WriteLine($"{property.Name} : {property.GetValue(settings, null)}");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine();
        }

        static void Connect()
        {
            twitch = new Twitch(settings);
            twitch.CreateConnection();
        }
    }
}
