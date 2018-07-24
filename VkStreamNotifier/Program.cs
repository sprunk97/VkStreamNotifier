using System;
using System.Net;
using System.Linq;
using CrashReporter;
using VkStreamNotifier.Schemes;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows.Input;

namespace VkStreamNotifier
{
    internal static class Program
    {
        #region variables
        private static Credentials credential;
        private static List<Streamer> streamers;
        #endregion

        [STAThread]
        static async Task Main(string[] args)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(GlobalExceptionHandler);

            if (args.Length != 0 && args.Contains("-lc"))
            {
                await Load();
                Connect();
            }

            Console.WriteLine("Commands: load, connect, update, help, exit");
            do
            {
                switch (Console.ReadLine())
                {
                    case "exit":
                        Environment.Exit(0);
                        break;
                    case "load":
                        await Load();
                        break;
                    case "connect":
                        Connect();
                        break;
                    case "update":
                        await UpdateEndings();
                        break;
                    case "help":
                        Console.WriteLine("Commands: load, connect, update, help, exit");
                        break;
                    case "":
                        Console.Clear();
                        Console.WriteLine("Commands: load, connect, update, help, exit");
                        break;
                    default:
                        Console.WriteLine("Unrecognized command. Use help");
                        break;
                }
            }
            while (true);
        }

        static void GlobalExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            NetworkCredential networkCredential = new NetworkCredential(credential.email, credential.email_password);
            var mail = new Sender(networkCredential, "sprunk97@gmail.com", "VkStreamNotifier Exception", null, null);
            try
            {
                mail.SendReport(e);
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }
        }

        static async Task Load()
        {
            var credentials = await SettingsWorker.GetCredentialsAsync();
            credential = credentials.Last();

            Console.WriteLine("\tCurrent credentials:");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            foreach (var property in typeof(Credentials).GetProperties())
                Console.WriteLine($"{property.Name} : {property.GetValue(credential, null)}");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine();

            streamers = await SettingsWorker.GetStreamersListAsync();
            foreach (var streamer in streamers)
            {
                foreach (var property in typeof(Streamer).GetProperties())
                    Console.WriteLine($"{property.Name} : {property.GetValue(streamer, null)}");
                Console.WriteLine();
            }
        }

        static void Connect()
        {
            var twitch = new Twitch(credential, streamers);
            twitch.CreateConnection();
        }

        static async Task UpdateEndings()
        {
            MongoDB.Driver.UpdateResult result = null;
            foreach (var streamer in streamers)
            {
                if (streamer.stream_ended == null)
                    result = await SettingsWorker.UpdateDowntimeAsync(DateTime.Now, streamer.twitch_username);
                if (result?.ModifiedCount > 0)
                    Console.WriteLine($"{streamer.twitch_username}: updated ending time");
            }
            Console.WriteLine("Updated");
        }
    }
}
