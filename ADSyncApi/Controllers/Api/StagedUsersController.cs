using ADSync.Common.Enums;
using ADSync.Common.Models;
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

                try
                {
                    StagedUserUtil.AddBulkUsersToQueue(userBatch, siteDomains);
                }
                catch (Exception ex)
                {
                    res = HttpStatusCode.InternalServerError;
                    Logging.WriteToSyncLog("StagedUsersController/UpdateBatch", ex.Message, System.Diagnostics.EventLogEntryType.Error, ex);
                    errorMessage = string.Format("An error occurred processing the request, check the event log for details ({0}).", ex.Message);
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
            return await StagedUserUtil.GetAllBySiteId(siteId);
        }

        public async Task<IEnumerable<StagedUser>> GetAllByStage(string stage)
        {
            LoadStageEnum loadStage = (LoadStageEnum)Enum.Parse(typeof(LoadStageEnum), stage);

            var siteDomains = SiteUtils.GetSiteDomainList(User.Identity);
            var siteId = User.Identity.GetClaim(CustomClaimTypes.SiteId);
            return await StagedUserUtil.GetAllByStageAndDomain(loadStage, siteDomains);
        }
        public async Task<RemoteSite> GetSiteConfig()
        {
            var siteId = User.Identity.GetClaim(CustomClaimTypes.SiteId);
            return await RemoteSiteUtil.GetSite(siteId);
        }
    }
}
