using ADSync.Common.Enums;
using ADSync.Common.Models;
using ADSync.Data.Models;
using Common;
using Infrastructure;
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
    /// <summary>
    /// HQ calls will be made using the AAD App ID and Secret
    /// </summary>
    [ApiAuth("Admin")]
    public class StagedUsersAdmController : ApiController
    {
        /// <summary>
        /// Called to retrieve the current list of remote sites to process
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IEnumerable<RemoteSite>> GetRemoteSiteList()
        {
            return await RemoteSiteUtil.GetRemoteSites();
        }

        /// <summary>
        /// Get the list of users to process
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<StagedUser>> GetAllByStage(string stage)
        {
            LoadStageEnum loadStage = (LoadStageEnum)Enum.Parse(typeof(LoadStageEnum), stage);
            var res = await StagedUserUtil.GetAllByStage(loadStage);
            return res.ToList();
        }
        /// <summary>
        /// Get the list of users to process
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<StagedUser>> GetAllByStageAndSiteType(string stage, string type)
        {
            LoadStageEnum loadStage = (LoadStageEnum)Enum.Parse(typeof(LoadStageEnum), stage);
            SiteTypes siteType = (SiteTypes)Enum.Parse(typeof(SiteTypes), type);

            var res = await StagedUserUtil.GetAllByStageAndSiteType(loadStage, siteType);
            return res.ToList();
        }

        
        /// <summary>
        /// Called after users have been created locally, setting the HQ MasterGUID and updating the LoadState
        /// </summary>
        /// <param name="userList"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage UpdateBatchAdmin(IEnumerable<StagedUser> userBatch)
        {
            var res = HttpStatusCode.NoContent; //204, response successfully processed with no content to return
            var errorMessage = "";

            if (userBatch == null || userBatch.Count() == 0)
            {
                errorMessage = "An empty or null collection was transmitted, no users were processed.";
            }
            else
            {
                try
                {
                    StagedUserUtil.AddBulkUsersToQueue(userBatch);
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
    }
}
