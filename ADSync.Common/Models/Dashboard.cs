using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADSync.Common.Models
{
    public class Dashboard
    {
        public int NumSites { get; set; }
        public DateTime? LastSiteActivity { get; set; }
        public int NumUsers { get; set; }
        public int NumUsersPendingSync { get; set; }
        public DateTime? LastUserSync { get; set; }
        public IEnumerable<DashLog> LatestLogs { get; set; }

        public class DashLog
        {
            public DateTime LogDate { get; set; }
            public string Message { get; set; }
            public string SiteName { get; set; }

            public DashLog(SyncLogEntry log, IEnumerable<RemoteSite> sites)
            {
                LogDate = log.LogDate;
                Message = (log.Detail.Length > 100) ? log.Detail.Substring(0, 100) + "..." : log.Detail;
                var site = sites.SingleOrDefault(s => s.Id == log.RemoteSiteID);
                SiteName = (site == null) ? "N/A" : site.OnPremDomainName;
            }
        }
    }
}
