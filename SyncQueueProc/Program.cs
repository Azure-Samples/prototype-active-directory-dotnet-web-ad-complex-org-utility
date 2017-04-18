using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using System.Configuration;
using Portal.Data;
using Common;

namespace SyncQueueProc
{
    // To learn more about Microsoft Azure WebJobs SDK, please see https://go.microsoft.com/fwlink/?LinkID=320976
    class Program
    {
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {
            DocDBRepo.Settings.DocDBUri = ConfigurationManager.AppSettings["DocDBUri"];
            DocDBRepo.Settings.DocDBAuthKey = ConfigurationManager.AppSettings["DocDBAuthKey"];
            DocDBRepo.Settings.DocDBName = ConfigurationManager.AppSettings["DocDBName"];
            DocDBRepo.Settings.DocDBCollection = ConfigurationManager.AppSettings["DocDBCollection"];

            var config = new JobHostConfiguration();
            config.StorageConnectionString = ConfigurationManager.AppSettings["StorageConnectionString"];
            config.DashboardConnectionString = null;

            //TODO: comment out for production
            config.Queues.BatchSize = 1;

            var client = DocDBRepo.Initialize().Result;

            if (config.IsDevelopment)
            {
                config.UseDevelopmentSettings();
            }

            var host = new JobHost();
            
            // The following code ensures that the WebJob will be running continuously
            host.RunAndBlock();
        }
    }
}
