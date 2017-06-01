using System;
using System.ServiceProcess;
using OrgRelay;
using ADSync.Common.Models;
using Common;
using System.Diagnostics;
using ADSync.Common.Enums;
using System.Linq;
using System.Reflection;
using System.Configuration;
using System.Threading;

namespace ComplexOrgSiteAgent
{
    static partial class Program
    {
        static SigRClient relay;

        public static string ConsoleLogSource;
        public static string ServiceName;
        public static string AssemblyName = "ComplexOrgSiteAgent.SiteListenerService";

        private static PingEvent _pingTest;
        private static bool _isDebug;
        private static bool _isRunning;
        private static int _pollIntervalMinutes;

        static void Main(string[] args)
        {
            ServiceName = Assembly.GetCallingAssembly().GetCustomAttribute<AssemblyTitleAttribute>().Title;
            ConsoleLogSource = String.Format("{0}-console", ServiceName);
            ServiceUtil.AssemblyName = AssemblyName;
            ServiceUtil.ServiceName = ServiceName;

            PrintLogo();

            if (args.Any(a => a == "-debug"))
            {
                _isDebug = true;
            }

            var sUrl = ConfigurationManager.AppSettings["SiteUrl"];
            var sDebugUrl = ConfigurationManager.AppSettings["DebugSiteUrl"];
            _pollIntervalMinutes = Convert.ToInt16(ConfigurationManager.AppSettings["TimerIntervalMinutes"]);

            OrgApiCalls.ApiKey = ConfigurationManager.AppSettings["ApiKey"];
            OrgApiCalls.SiteUrl = (_isDebug) ? sDebugUrl : sUrl;

            ConsoleLogSource = Utils.SetupLog(ConsoleLogSource);

            try
            {
                RemoteSite siteConfig = null;

                //while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape))
                //{
                    try
                    {
                        siteConfig = OrgApiCalls.GetSiteConfig();
                        //break;
                    }
                    catch (Exception)
                    {
                        //var delay = new Random().Next(2000, 4000);
                        WriteConsoleStatus("Failed retrieving site status");
                        //Console.WriteLine("Press ESC to cancel");
                        //Thread.Sleep(delay);
                    }
                //}

                if (siteConfig == null)
                {
                    throw new Exception("Unable to retrieve site configuration, exiting");
                }

                ADTools.ADDomainName = siteConfig.OnPremDomainName;

                relay = new SigRClient(OrgApiCalls.SiteUrl, OrgApiCalls.ApiKey, "SiteHub");

                relay.StatusUpdate += Relay_StatusUpdate;
                relay.ErrorEvent += Relay_ErrorEvent;
                relay.PingEvent += Relay_PingEvent;

                if (Environment.UserInteractive)
                {
                    if (ParseCommandLine(args))
                    {
                        relay.StartAsync().Wait();
                        _isRunning = true;
                        
                        while (_isRunning)
                        {
                            Console.TreatControlCAsInput = true;
                            Console.WriteLine("Press \"p\" to send a Ping test, Ctrl-C to end.");
                            var key = Console.ReadKey(true);
                            switch (key.Key)
                            {
                                case ConsoleKey.P:
                                    WriteConsoleStatus("Sending ping...");
                                    _pingTest = new PingEvent(DateTime.Now);
                                    relay.Send(new RelayMessage {
                                        Operation = SiteOperation.Ping,
                                        ApiKey=OrgApiCalls.ApiKey,
                                        DestSiteId = siteConfig.Id
                                    });
                                    break;
                                case ConsoleKey.Escape:
                                case ConsoleKey.C:
                                    if (key.Key == ConsoleKey.C && key.Modifiers != ConsoleModifiers.Control)
                                        break;

                                    Console.WriteLine("Stop the relay agent? (Y or N)");
                                    if (Console.ReadKey().Key == ConsoleKey.Y)
                                    {
                                        WriteConsoleStatus("Received shutdown command.");
                                        _isRunning = false;
                                    }
                                    break;
                            }
                        }
                        relay.Stop();
                    }
                }
                else
                {

                    ServiceBase[] ServicesToRun;
                    ServicesToRun = new ServiceBase[]
                    {
                        new SiteListenerService(relay)
                    };
                    ServiceBase.Run(ServicesToRun);
                }
            }
            catch (Exception ex)
            {
                var msg = Utils.GetFormattedException(ex);
                Utils.AddLogEntry(ConsoleLogSource, msg, EventLogEntryType.Error);
            }
        }

        private static void Relay_PingEvent(object sender, PingEvent e)
        {
            var span = e.EndTime - _pingTest.StartTime;
            Console.WriteLine("{0} (elapsed time {1} milliseconds)", e.Message, span.Milliseconds);
        }

        private static void Relay_ErrorEvent(object sender, ErrorEvent e)
        {
            Utils.AddLogEntry(ConsoleLogSource, e.Message, e.LogEntryType, e.EventId, e.Exception);
            Console.WriteLine("{0}: {1}", e.LogEntryType.ToString(),  e.Message);
        }

        private static void Relay_StatusUpdate(object sender, StatusEvent e)
        {
            Console.Write(e.Message);
        }

        static void PrintLogo()
        {
            Console.WriteLine("Complex Org Site Agent\n(c) Microsoft Corporation\n\n");
        }
        static void PrintInstructions()
        {
            Console.WriteLine("Instructions:");
            Console.WriteLine("   <program> -install -username:<username> -password:<password>");
            Console.WriteLine("        Installs the service on this server.");
            Console.WriteLine("");
            Console.WriteLine("   <program> -uninstall");
            Console.WriteLine("        Removes the service from this server.");
            Console.WriteLine("");
            Console.WriteLine("   <program> -console");
            Console.WriteLine("        Runs the utility as a command-line program.");
            Console.WriteLine("");
        }

        static bool ParseCommandLine(string[] args)
        {
            if (args.Length == 0)
            {
                PrintInstructions();
                return false;
            }

            switch (args[0])
            {
                case "-install":
                    var username = args.SingleOrDefault(a => a.StartsWith("-username"));
                    var password = args.Single(a => a.StartsWith("-password"));
                    if (username==null || password == null)
                    {
                        Console.WriteLine("Install requires username and password");
                        PrintInstructions();
                        return false;
                    }
                    username = username.Split(':')[1];
                    password = password.Split(':')[1];

                    ServiceUtil.InstallService(username, password);
                    ServiceUtil.StartService();
                    return false;

                case "-uninstall":
                    ServiceUtil.StopService();
                    ServiceUtil.UninstallService();

                    return false;

                case "-console":
                    return true;
            }
            return false;
        }
        private static void WriteConsoleStatus(string message)
        {
            Console.WriteLine("{0}: {1}{2}", DateTime.Now, message, Environment.NewLine);
        }

        private static void WriteConsoleStatus(string message, params object[] args)
        {
            message = string.Format(message, args);
            WriteConsoleStatus(message);
        }

    }
}
