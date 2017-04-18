using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Portal.Interfaces;
using Portal.Data;
using System.Threading.Tasks;
using Common;

namespace ADSync.Data.Models
{
    public class SyncLogEntry : DocModelBase, IDocModelBase
    {
        [JsonProperty(PropertyName = "errorType")]
        public string ErrorType { get; set; }

        [JsonProperty(PropertyName = "detail")]
        public string Detail { get; set; }

        [JsonProperty(PropertyName = "stagedUserId")]
        public string StagedUserId { get; set; }

        [JsonProperty(PropertyName = "remoteSiteId")]
        public string RemoteSiteID { get; set; }

        [JsonProperty(PropertyName = "source")]
        public string Source { get; set; }

        [JsonProperty(PropertyName = "logDate")]
        public DateTime LogDate { get; set; }

        public SyncLogEntry()
        {
            LogDate = DateTime.UtcNow;
        }

        public SyncLogEntry(string errorType, string detail, string source)
        {
            ErrorType = errorType;
            Detail = detail;
            Source = source;
            LogDate = DateTime.UtcNow;
        }

        public static async Task<IEnumerable<SyncLogEntry>> GetLogs(int? days=null)
        {
            IEnumerable<SyncLogEntry> res;

            if (days == null)
            {
                res = await DocDBRepo.DB<SyncLogEntry>.GetItemsAsync();
            }
            else
            {
                var cutoffdate = DateTime.UtcNow.AddDays((int)days * -1);
                res = await DocDBRepo.DB<SyncLogEntry>.GetItemsAsync(l => l.LogDate > cutoffdate);
            }
            return res;
        }

        public static async Task<bool> AddLog(SyncLogEntry log)
        {
            var res = await DocDBRepo.DB<SyncLogEntry>.CreateItemAsync(log);
            return (res != null);
        }

        public static void AddBulkLogs(IEnumerable<SyncLogEntry>logBatch, string siteId)
        {
            logBatch.ToList().ForEach(l => l.RemoteSiteID = siteId);
            //this will be queued
            StorageRepo.AddQueueItem(logBatch, "logbatch");
        }

        public static async Task<bool> ProcessCollection(IEnumerable<SyncLogEntry> logs)
        {
            var res = true;
            SyncLogEntry currLog = null;
            try
            {
                foreach (var log in logs)
                {
                    currLog = log;
                    await AddLog(log);
                }
            }
            catch (Exception ex)
            {
                Logging.WriteToAppLog("Error writing a sync log batch", System.Diagnostics.EventLogEntryType.Error, ex);
                res = false;
            }

            return res;
        }
    }
}