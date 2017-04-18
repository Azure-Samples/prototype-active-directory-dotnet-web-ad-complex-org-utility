using ADSync.Data.Models;
using Common;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace ADSyncApi.Controllers.Api
{
    [ApiAuth]
    public class SyncLogUpdateController : ApiController
    {
        [HttpPost]
        public async Task<bool> AddLogEntry(SyncLogEntry log)
        {
            var siteId = User.Identity.GetClaim(CustomClaimTypes.SiteId);
            log.RemoteSiteID = siteId;
            return await SyncLogEntry.AddLog(log);
        }

        /// <summary>
        /// Called after users have been created locally, setting the HQ MasterGUID and updating the LoadState
        /// </summary>
        /// <param name="userList"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage AddBatchLogs(IEnumerable<SyncLogEntry> logBatch)
        {
            var siteId = User.Identity.GetClaim(CustomClaimTypes.SiteId);

            var res = HttpStatusCode.NoContent; //204, response successfully processed with no content to return
            var errorMessage = "";

            if (logBatch == null || logBatch.Count() == 0)
            {
                errorMessage = "An empty or null collection was transmitted, no logs were added.";
            }
            else
            {
                try
                {
                    SyncLogEntry.AddBulkLogs(logBatch, siteId);
                }
                catch (Exception ex)
                {
                    res = HttpStatusCode.InternalServerError;
                    Logging.WriteToAppLog("Error processing SyncLogUpdateController/AddBatchLogs", System.Diagnostics.EventLogEntryType.Error, ex);
                    errorMessage = string.Format("An error occurred processing the request, check the system event log for details ({0}).", ex.Message);
                }
            }

            HttpResponseMessage response = Request.CreateResponse(res);
            response.Headers.Add("ErrorMessage", errorMessage);
            return response;
        }
    }
}
