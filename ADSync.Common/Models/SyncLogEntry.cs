using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Common;
using ADSync.Common.Interfaces;

namespace ADSync.Common.Models
{
    public class SyncLogEntry : DocModelBase, IDocModelBase
    {
        [JsonProperty(PropertyName = "errorType")]
        public string ErrorType { get; set; }

        [JsonProperty(PropertyName = "detail")]
        public string Detail { get; set; }

        [JsonProperty(PropertyName = "stagedUserId")]
        public string StagedUserId { get; set; }

        [JsonProperty(PropertyName = "remoteSiteId")]
        public string RemoteSiteID { get; set; }

        [JsonProperty(PropertyName = "source")]
        public string Source { get; set; }

        [JsonProperty(PropertyName = "logDate")]
        public DateTime LogDate { get; set; }

        public SyncLogEntry()
        {
            LogDate = DateTime.UtcNow;
        }

        public SyncLogEntry(string errorType, string detail, string source)
        {
            ErrorType = errorType;
            Detail = detail;
            Source = source;
            LogDate = DateTime.UtcNow;
        }
    }
}