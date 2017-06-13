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
using System.Collections.Generic;
using System.IO;
using ADSync.Common.Events;
using Newtonsoft.Json;

namespace ComplexOrgSiteAgent
{
    static partial class Program
    {
        static SigRClient relay;
        static ScriptTimer timer;

        static string ConsoleLogSource;
        static string ServiceName;
        static string AssemblyName = "ComplexOrgSiteAgent.SiteListenerService";

        static PingEvent _pingTest;
        static bool _isDebug;
        static bool _isRunning;
        static int _timerIntervalMinutes;
        static string _scriptFolderPath;

        static void Main(string[] args)
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _scriptFolderPath = args.Where(a => a.StartsWith("-scriptPath")).FirstOrDefault();
            if (_scriptFolderPath != null)
            {
                _scriptFolderPath = _scriptFolderPath.Split(':')[1];
            }
            else
            {
                _scriptFolderPath = Path.GetFullPath(string.Format("{0}\\Scripts", dir));
            }

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
            _timerIntervalMinutes = Convert.ToInt16(ConfigurationManager.AppSettings["TimerIntervalMinutes"]);

            OrgApiCalls.ApiKey = ConfigurationManager.AppSettings["ApiKey"];
            OrgApiCalls.SiteUrl = (_isDebug) ? sDebugUrl : sUrl;

            ConsoleLogSource = Utils.SetupLog(ConsoleLogSource);

            try
            {
                RemoteSite siteConfig = null;

                try
                {
                    siteConfig = OrgApiCalls.GetSiteConfig();
                }
                catch (Exception)
                {
                    WriteConsoleStatus("Failed retrieving site status");
                }

                if (siteConfig == null)
                {
                    throw new Exception("Unable to retrieve site configuration, exiting");
                }

                var scriptConfig = GetJsonConfig(siteConfig, OrgApiCalls.SiteUrl);
                File.WriteAllText(Path.Combine(_scriptFolderPath, "SyncVars.json"), scriptConfig);

                ADTools.ADDomainName = siteConfig.OnPremDomainName;

                relay = new SigRClient(OrgApiCalls.SiteUrl, OrgApiCalls.ApiKey, "SiteHub", true);

                relay.StatusUpdate += Relay_StatusUpdate;
                relay.ErrorEvent += Relay_ErrorEvent;
                relay.PingEvent += Relay_PingEvent;

                InitTimers(siteConfig);

                relay.FireScriptEvent += Relay_FireScriptEvent;

                if (Environment.UserInteractive)
                {
                    RunConsole(siteConfig, args);
                }
                else
                {
                    ServiceBase[] ServicesToRun;
                    ServicesToRun = new ServiceBase[]
                    {
                        new SiteListenerService(relay, timer)
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

        private static void Relay_FireScriptEvent(object sender, FireScriptEvent e)
        {
            ScriptTimer.RunScript(e.Script);
        }

        private static void RunConsole(RemoteSite siteConfig, string[] args)
        {
            if (ParseCommandLine(args))
            {
                relay.StartAsync().Wait();
                timer.Start();
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
                            relay.Send(new RelayMessage
                            {
                                Operation = SiteOperation.Ping,
                                ApiKey = OrgApiCalls.ApiKey,
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

        private static void InitTimers(RemoteSite site)
        {
            //setup timers
            var scripts = new List<ScriptObject>();

            if (site.SiteType == SiteTypes.MasterHQ)
            {
                scripts.Add(new ScriptObject("sync-ad.ps1"));
                scripts.Add(new ScriptObject("sync-b2b.ps1"));
            }
            else
            {
                scripts.Add(new ScriptObject("sync.ps1"));
            }

            ScriptTimer.ScriptFolderPath = _scriptFolderPath;

            timer = new ScriptTimer(scripts.ToArray(), _timerIntervalMinutes);
            timer.ErrorEvent += Timer_ErrorEvent;
            timer.StatusUpdate += Timer_StatusUpdate;
        }

        #region Events
        private static void Timer_StatusUpdate(object sender, StatusEvent e)
        {
            WriteConsoleStatus(e.Message);
        }

        private static void Timer_ErrorEvent(object sender, ErrorEvent e)
        {
            Utils.AddLogEntry(ConsoleLogSource, e.Message, e.LogEntryType, e.EventId, e.Exception);
            WriteConsoleStatus("{0}: {1}", e.LogEntryType.ToString(), e.Message);
        }

        private static void Relay_PingEvent(object sender, PingEvent e)
        {
            var span = e.EndTime - _pingTest.StartTime;
            WriteConsoleStatus("{0} (elapsed time {1} milliseconds)", e.Message, span.Milliseconds);
        }

        private static void Relay_ErrorEvent(object sender, ErrorEvent e)
        {
            Utils.AddLogEntry(ConsoleLogSource, e.Message, e.LogEntryType, e.EventId, e.Exception);
            WriteConsoleStatus("{0}: {1}", e.LogEntryType.ToString(), e.Message);
        }

        private static void Relay_StatusUpdate(object sender, StatusEvent e)
        {
            WriteConsoleStatus(e.Message);
        }
        #endregion

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
            Console.WriteLine("{0}{2}{1}{2}", DateTime.Now, message, Environment.NewLine);
            Console.WriteLine("Press \"p\" to send a Ping test, Ctrl-C to end.");
        }

        private static void WriteConsoleStatus(string message, params object[] args)
        {
            message = string.Format(message, args);
            WriteConsoleStatus(message);
        }

        private static string GetJsonConfig(RemoteSite site, string apiUrl)
        {
            var scriptUpdates = new Dictionary<string, string>
            {
                { "ApiKey", site.ApiKey },
                { "ApiSite", apiUrl }
            };

            if (site.SiteType == SiteTypes.MasterHQ)
            {
                //to enable GraphAPI calls for B2B invitations
                scriptUpdates.Add("AADClientID", ConfigurationManager.AppSettings["AADClientID"]);
                scriptUpdates.Add("AADClientKey", ConfigurationManager.AppSettings["AADClientKey"]);
                scriptUpdates.Add("AADTenantID", ConfigurationManager.AppSettings["AADTenantID"]);
            }

            var config = JsonConvert.SerializeObject(scriptUpdates);
            return config;
        }
    }
}
