using System;
using System.Net;
using System.Linq;
using CrashReporter;
using VkStreamNotifier.Schemes;
using System.Collections.Generic;
using NLog;

namespace VkStreamNotifier
{
    internal static class Program
    {
        #region variables
        private static Credentials credential;
        private static List<Streamer> streamers;
        #endregion

        [STAThread]
        static void Main(string[] args)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(GlobalExceptionHandler);

            var config = new NLog.Config.LoggingConfiguration();
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "NLog.txt" };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);
            LogManager.Configuration = config;
            var log = LogManager.GetCurrentClassLogger();
            log.Info("\r\n\tSTARTED");

            if (args.Length != 0 && args.Contains("-lc"))
            {
                Load();
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
                        Load();
                        break;
                    case "connect":
                        Connect();
                        break;
                    case "update":
                        UpdateEndings();
                        break;
                    case "help":
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
            var logger = LogManager.GetCurrentClassLogger();

            Exception e = (Exception)args.ExceptionObject;
            NetworkCredential networkCredential = new NetworkCredential(credential.email, credential.email_password);
            var mail = new Sender(networkCredential, "sprunk97@gmail.com", "VkStreamNotifier Exception", null, null);
            try
            {
                mail.SendReport(e);
            }
            catch (Exception exc)
            {
                logger.Error("Error sending crash report");
                logger.Trace(exc);
            }

            logger.Info("Sent crash trace over email");
        }

        static void Load()
        {
            var credentials = SettingsWorker.GetCredentials();
            credential = credentials.Last();
            Console.WriteLine("\tCredentials loaded");

            streamers = SettingsWorker.GetStreamersList();
            foreach (var streamer in streamers)
                Console.WriteLine($"Streamer : {streamer.twitch_username} : loaded");
        }

        static void Connect()
        {
            var twitch = new Twitch(credential, streamers);
            twitch.CreateConnection();
        }

        static void UpdateEndings()
        {
            foreach (var streamer in streamers)
            {
                if (streamer.stream_ended == null)
                {
                    SettingsWorker.UpdateDowntime(DateTime.Now, streamer.twitch_username);
                    Console.WriteLine($"{streamer.twitch_username}: updated ending time");
                }
            }
        }
    }
}
