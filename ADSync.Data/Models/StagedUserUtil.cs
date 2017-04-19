using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Portal.Data;
using Common;
using ADSync.Common.Models;
using ADSync.Common.Enums;

namespace ADSync.Data.Models
{
    public static class StagedUserUtil
    {
        public static void AddUserToQueue(StagedUser user)
        {
            //this will be queued
            StorageRepo.AddQueueItem(user, "stageduser");
        }

        public static void AddBulkUsersToQueue(IEnumerable<StagedUser> userBatch)
        {
            //this will be queued
            StorageRepo.AddQueueItem(userBatch, "stageduser");
        }

        public static async Task<IEnumerable<StagedUser>> GetUsers()
        {
            var res = await DocDBRepo.DB<StagedUser>.GetItemsAsync();
            
            return res;
        }

        public static async Task<IEnumerable<StagedUser>> GetAllByStage(LoadStageEnum state)
        {
            var res = await DocDBRepo.DB<StagedUser>.GetItemsAsync(u => u.LoadState == state);
            return res.ToList();
        }

        public static async Task<IEnumerable<StagedUser>> GetAllByStageAndSiteType(LoadStageEnum state, SiteTypes type)
        {
            var res = await DocDBRepo.DB<StagedUser>.GetItemsAsync(u => u.LoadState == state && u.SiteType == type);
            return res.ToList();
        }

        public static async Task<IEnumerable<StagedUser>> GetAllByStageAndDomain(LoadStageEnum state, string domainName)
        {
            var res = await DocDBRepo.DB<StagedUser>.GetItemsAsync(u => u.LoadState == state && u.DomainName == domainName);
            return res.ToList();
        }
        public static async Task<IEnumerable<StagedUser>> GetAllByStageAndDomain(LoadStageEnum state, IEnumerable<string> domainNameList)
        {
            //"Any" is not supported by DocumentDB via IEnumerable:
            //http://stackoverflow.com/questions/33839854/c-sharp-linq-any-not-working-on-documentdb-createdocumentquery

            //So, we'll loop the domain name list and grab the results of each, stuffing them into the return list
            var res = new List<StagedUser>();
            foreach (var domainName in domainNameList)
            {
                var res1 = await DocDBRepo.DB<StagedUser>.GetItemsAsync(u => u.DomainName == domainName && u.LoadState == state);
                res.AddRange(res1);
            }
            return res.ToList();
        }

        public static async Task<IEnumerable<StagedUser>> GetAllByDomain(string domainName)
        {
            var res = await DocDBRepo.DB<StagedUser>.GetItemsAsync(u => u.DomainName == domainName);
            return res.ToList();
        }

        public static async Task<IEnumerable<StagedUser>> GetAllBySiteId(string siteId)
        {
            var res = await DocDBRepo.DB<StagedUser>.GetItemsAsync(u => u.SiteId == siteId);
            return res.ToList();
        }

        public static async Task<IEnumerable<StagedUser>> GetAllByDomain(IEnumerable<string> domainNameList)
        {
            //"Any" is not supported by DocumentDB via IEnumerable
            var res = new List<StagedUser>();
            foreach(var domainName in domainNameList)
            {
                var res1 = await DocDBRepo.DB<StagedUser>.GetItemsAsync(u => u.DomainName == domainName);
                res.AddRange(res1);
           }
            return res.ToList();
        }

        public static async Task<bool> Update(StagedUser user)
        {
            try
            {
                var item = await DocDBRepo.DB<StagedUser>.UpdateItemAsync(user);
                return (item != null);
            }
            catch (Exception ex)
            {
                Logging.WriteToSyncLog("StagedUser/Update", ex.Message, System.Diagnostics.EventLogEntryType.Error, ex);
                return false;
            }
        }

        public static async Task<bool> ProcessCollection(IEnumerable<StagedUser> users)
        {
            var res = true;
            StagedUser currStagedUser = null;
            try
            {
                foreach(var user in users)
                {
                    currStagedUser = user;
                    switch (currStagedUser.LoadState)
                    {
                        case LoadStageEnum.PendingHQAdd:
                            await DocDBRepo.DB<StagedUser>.CreateItemAsync(currStagedUser);
                            break;

                        case LoadStageEnum.NewNothingPending:
                            currStagedUser.LoadState = LoadStageEnum.NothingPending;
                            await DocDBRepo.DB<StagedUser>.CreateItemAsync(currStagedUser);
                            break;

                        case LoadStageEnum.Deleted:
                            //We will leave this "deleted" record in the database for posterity
                            //await DocDBRepo.DB<StagedUser>.DeleteItemAsync(currStagedUser);
                            break;

                        default:
                            await DocDBRepo.DB<StagedUser>.UpdateItemAsync(currStagedUser);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                res = false;
                await SyncLogEntryUtil.AddLog(new SyncLogEntry
                {

                    Source = "StagedUser.ProcessCollection",
                    StagedUserId = (currStagedUser!=null) ? currStagedUser.Id : null,
                    Detail = string.Format("User: {0}\n\rError: {1}", ((currStagedUser != null) ? currStagedUser.Upn : "N/A"), ex.Message),
                    ErrorType = System.Diagnostics.EventLogEntryType.Error.ToString(),
                    LogDate=DateTime.UtcNow
                });
            }

            return res;
        }
    }
}