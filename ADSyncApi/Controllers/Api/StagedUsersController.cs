using ADSync.Data.Models;
using Common;
using Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Portal.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

namespace ADSyncApi.Controllers.Api
{
    [ApiAuth]
    public class StagedUsersController : ApiController
    {
        /// <summary>
        /// Remote sites will call this using their individually-issued RemoteSite API key
        /// </summary>
        /// <param name="userList"></param>
        [HttpPost]
        public HttpResponseMessage UpdateBatch(IEnumerable<StagedUser> userBatch)
        {
            var res = HttpStatusCode.NoContent; //204, response successfully processed with no content to return
            var errorMessage = "";

            if (userBatch == null || userBatch.Count() == 0)
            {
                errorMessage = "An empty or null collection was transmitted, no users were processed.";
            }
            else
            { 
                var siteDomains = SiteUtils.GetSiteDomainList(User.Identity);
                var siteId = User.Identity.GetClaim(CustomClaimTypes.SiteId);

                //sanity check on domains
                //note: this checks the indexed domain name, not the upn suffix. The upn suffix
                //will be checked before being dequeued - if an error occurs there, it will be logged.
                //var isInvalid = userBatch.Any(u => siteDomains.Any(d => d != u.DomainName));
                var isInvalid = userBatch.Select(u => u.DomainName).Except(siteDomains).Count() > 0;
                if (isInvalid)
                {
                    res = HttpStatusCode.InternalServerError;
                    var siteDomainList = string.Join(",", siteDomains);
                    var logMessage = string.Format("A user domain record was found that didn't match one of the caller domains \"{0}\"", siteDomainList);
                    Logging.WriteToSyncLog("StagedUsersController/AddPending", logMessage, System.Diagnostics.EventLogEntryType.Error);
                    errorMessage = "One or more domain names are invalid for your site. Please check your configuration.";
                }
                else
                {
                    try
                    {
                        StagedUser.AddBulkUsersToQueue(userBatch);
                    }
                    catch (Exception ex)
                    {
                        res = HttpStatusCode.InternalServerError;
                        Logging.WriteToSyncLog("StagedUsersController/UpdateBatch", ex.Message, System.Diagnostics.EventLogEntryType.Error, ex);
                        errorMessage = string.Format("An error occurred processing the request, check the event log for details ({0}).", ex.Message);
                    }
                }
            }

            HttpResponseMessage response = Request.CreateResponse(res);
            response.Headers.Add("ErrorMessage", errorMessage);
            return response;
        }

        public async Task<IEnumerable<StagedUser>> GetAllStaged()
        {
            var siteDomains = SiteUtils.GetSiteDomainList(User.Identity);
            var siteId = User.Identity.GetClaim(CustomClaimTypes.SiteId);
            return await StagedUser.GetAllByDomain(siteDomains);
        }

        public async Task<IEnumerable<StagedUser>> GetAllByStage(string stage)
        {
            LoadStage loadStage = (LoadStage)Enum.Parse(typeof(LoadStage), stage);

            var siteDomains = SiteUtils.GetSiteDomainList(User.Identity);
            var siteId = User.Identity.GetClaim(CustomClaimTypes.SiteId);
            return await StagedUser.GetAllByStageAndDomain(loadStage, siteDomains);
        }
        public async Task<RemoteSite> GetSiteConfig()
        {
            var siteId = User.Identity.GetClaim(CustomClaimTypes.SiteId);
            return await RemoteSite.GetSite(siteId);
        }
    }
}
