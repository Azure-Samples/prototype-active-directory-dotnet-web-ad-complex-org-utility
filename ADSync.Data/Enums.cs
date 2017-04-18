using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Portal.Interfaces
{
    /// <summary>
    /// Filter for DocumentDB doc types - any records stored in DocumentDB need their type names added here
    /// </summary>
    public enum DocTypes
    {
        SyncLogEntry,
        StagedUser,
        RemoteSite
    }
    public enum SiteTypes
    {
        MasterHQ,
        AADB2B,
        LocalADOnly
    }
}