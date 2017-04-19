using ADSync.Common.Models;
using ADSync.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace ADSyncApi.Controllers.Api
{
    public class LogApiController : ApiController
    {
        public async Task<IEnumerable<SyncLogEntry>> GetLogsBySite(string siteId, int? days = null)
        {
            var res = await SyncLogEntryUtil.GetLogsBySite(siteId, days);
            return res.OrderByDescending(l => l.LogDate).ToList();
        }

        public async Task<IEnumerable<SyncLogEntry>> GetLogsByUser(string userId, int? days = null)
        {
            var res = await SyncLogEntryUtil.GetLogsByUser(userId, days);
            return res.OrderByDescending(l => l.LogDate).ToList();
        }
    }
}
