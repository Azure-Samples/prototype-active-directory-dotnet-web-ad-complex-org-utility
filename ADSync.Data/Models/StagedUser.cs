using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Portal.Interfaces;
using System.Threading.Tasks;
using Portal.Data;
using Common;

namespace ADSync.Data.Models
{
    public class StagedUser : DocModelBase, IDocModelBase
    {
        [JsonProperty(PropertyName = "upn")]
        public string Upn { get; set; }

        [JsonProperty(PropertyName = "domainName")]
        public string DomainName { get; set; }

        [JsonProperty(PropertyName = "masterGuid")]
        public string MasterGuid { get; set; }

        [JsonProperty(PropertyName = "localGuid")]
        public string LocalGuid { get; set; }

        [JsonProperty(PropertyName = "createDate")]
        public DateTime CreateDate { get; set; }

        [JsonProperty(PropertyName = "loadState")]
        public LoadStage LoadState { get; set; }

        [JsonProperty(PropertyName = "department")]
        public string Department { get; set; }

        [JsonProperty(PropertyName = "mobile")]
        public string Mobile { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "telephoneNumber")]
        public string TelephoneNumber { get; set; }

        [JsonProperty(PropertyName = "homePhone")]
        public string HomePhone { get; set; }

        [JsonProperty(PropertyName = "postalCode")]
        public string PostalCode { get; set; }

        [JsonProperty(PropertyName = "mail")]
        public string Mail { get; set; }

        [JsonProperty(PropertyName = "surname")]
        public string Surname {get; set; }

        [JsonProperty(PropertyName = "givenName")]
        public string GivenName { get; set; }

        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "streetAddress")]
        public string StreetAddress { get; set; }

        [JsonProperty(PropertyName = "city")]
        public string City { get; set; }

        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }

        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }

        public StagedUser()
        {
            CreateDate = DateTime.UtcNow;
        }

        public StagedUser(string upn, string domainName, string masterGuid, string localGuid, LoadStage loadState)
        {
            Upn = upn;
            DomainName = domainName;
            MasterGuid = masterGuid;
            LocalGuid = localGuid;
            CreateDate = DateTime.UtcNow;
            LoadState = loadState;
        }

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

        public static async Task<IEnumerable<StagedUser>> GetAllByStage(LoadStage state)
        {
            var res = await DocDBRepo.DB<StagedUser>.GetItemsAsync(u => u.LoadState == state);
            return res.ToList();
        }

        public static async Task<IEnumerable<StagedUser>> GetAllByStageAndDomain(LoadStage state, string domainName)
        {
            var res = await DocDBRepo.DB<StagedUser>.GetItemsAsync(u => u.LoadState == state && u.DomainName == domainName);
            return res.ToList();
        }
        public static async Task<IEnumerable<StagedUser>> GetAllByStageAndDomain(LoadStage state, IEnumerable<string> domainNameList)
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
                        case LoadStage.PendingHQAdd:
                            await DocDBRepo.DB<StagedUser>.CreateItemAsync(currStagedUser);
                            break;

                        case LoadStage.Deleted:
                            await DocDBRepo.DB<StagedUser>.DeleteItemAsync(currStagedUser);
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
                await SyncLogEntry.AddLog(new SyncLogEntry
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