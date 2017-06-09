using ADSync.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using ADSync.Common.Models;
using Common;
using ADSync.Common.Enums;

namespace ADSyncApi.Controllers.Api
{
    [Authorize]
    public class DashboardController : ApiController
    {
        [HttpGet]
        public async Task<Dashboard> Logs()
        {
            var data = new Dashboard();
            var siteActivity = await SyncLogEntryUtil.GetLogs(1);
            var sites = await RemoteSiteUtil.GetAllSites();

            var lastLog = siteActivity
                .OrderByDescending(s => s.LogDate)
                .FirstOrDefault();

            data.LastSiteActivity = lastLog?.LogDate;

            data.LatestLogs = siteActivity
                .Where(l => l.ErrorType == "Error")
                .OrderByDescending(l => l.LogDate)
                .Take(5)
                .Select(l => new Dashboard.DashLog(l, sites))
                .ToList();

            var lastUserActivity = siteActivity
                .Where(u => u.StagedUserId != null)
                .OrderByDescending(u => u.LogDate)
                .FirstOrDefault();

            data.LastUserSync = lastUserActivity?.LogDate;

            return data;
        }

        public async Task<Dashboard> Users()
        {
            var data = new Dashboard();
            var users = await StagedUserUtil.GetUsers();
            data.NumUsers = users.Count();
            data.NumUsersPendingSync = users
                .Where(u => u.LoadState.IsAnyOf(LoadStageEnum.PendingHQAdd, LoadStageEnum.PendingHQDelete, LoadStageEnum.PendingHQUpdate, LoadStageEnum.PendingRemoteUpdate))
                .Count();

            return data;
        }

        public async Task<Dashboard> Sites()
        {
            var data = new Dashboard();
            var sites = await RemoteSiteUtil.GetAllSites();
            data.NumSites = sites.Count();

            return data;
        }
    }
}
