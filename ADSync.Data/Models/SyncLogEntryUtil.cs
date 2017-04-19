using System;
using System.Collections.Generic;
using System.Linq;
using Portal.Data;
using System.Threading.Tasks;
using Common;
using ADSync.Common.Models;

namespace ADSync.Data.Models
{
    public static class SyncLogEntryUtil
    {
        public static async Task<IEnumerable<SyncLogEntry>> GetLogsBySite(string siteID, int? days = null)
        {
            IEnumerable<SyncLogEntry> res;

            if (days == null)
            {
                res = await DocDBRepo.DB<SyncLogEntry>.GetItemsAsync(l => l.RemoteSiteID == siteID);
            }
            else
            {
                var cutoffdate = DateTime.UtcNow.AddDays((int)days * -1);
                res = await DocDBRepo.DB<SyncLogEntry>.GetItemsAsync(l => l.LogDate > cutoffdate && l.RemoteSiteID == siteID);
            }
            return res;
        }

        public static async Task<IEnumerable<SyncLogEntry>> GetLogsByUser(string userID, int? days = null)
        {
            IEnumerable<SyncLogEntry> res;

            if (days == null)
            {
                res = await DocDBRepo.DB<SyncLogEntry>.GetItemsAsync(l => l.StagedUserId == userID);
            }
            else
            {
                var cutoffdate = DateTime.UtcNow.AddDays((int)days * -1);
                res = await DocDBRepo.DB<SyncLogEntry>.GetItemsAsync(l => l.LogDate > cutoffdate && l.StagedUserId == userID);
            }
            return res;
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