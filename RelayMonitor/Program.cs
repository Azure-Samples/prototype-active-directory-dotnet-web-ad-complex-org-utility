using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Portal.Data;
using System.Configuration;
using ADSync.Common.Models;
using Newtonsoft.Json;
using Common;
using System.Diagnostics;

namespace RelayMonitor
{
    // To learn more about Microsoft Azure WebJobs SDK, please see https://go.microsoft.com/fwlink/?LinkID=320976
    class Program
    {
        private static OrgRelayServer _relayServer;
        private static ServerOp _ops;

        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {
            _ops = new ServerOp();

            DocDBRepo.Settings.DocDBUri = ConfigurationManager.AppSettings["DocDBUri"];
            DocDBRepo.Settings.DocDBAuthKey = ConfigurationManager.AppSettings["DocDBAuthKey"];
            DocDBRepo.Settings.DocDBName = ConfigurationManager.AppSettings["DocDBName"];
            string sbRelayConnectionString = ConfigurationManager.AppSettings["SBRelayConnectionString"];

            var config = new JobHostConfiguration();
            config.StorageConnectionString = ConfigurationManager.AppSettings["StorageConnectionString"];
            config.DashboardConnectionString = ConfigurationManager.AppSettings["StorageConnectionString"];

            if (config.IsDevelopment)
            {
                config.UseDevelopmentSettings();
            }

            var client = DocDBRepo.Initialize().Result;
            _relayServer = new OrgRelayServer(sbRelayConnectionString);
            _relayServer.IncomingMessage += RelayServer_IncomingMessage;

            _relayServer.Start();

            var host = new JobHost(config);

            // The following code ensures that the WebJob will be running continuously
            host.RunAndBlock();
        }

        private static void RelayServer_IncomingMessage(object sender, ADSync.Common.Events.MessageEventArgs e)
        {
            try
            {
                var message = JsonConvert.DeserializeObject<RelayMessage>(e.Message);
                _ops.ProcessMessage(message, _relayServer);
            }
            catch (Exception ex)
            {
                var msg = Utils.GetFormattedException(ex);
                Console.Write(msg);
                EventLog.WriteEntry("Application", msg, EventLogEntryType.Error, 0);
            }
        }
    }
}
