using ADSync.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADSync.Common.Models
{
    public class RelayMessage
    {
        public SiteOperation Operation { get; set; }
        public string DestSiteId { get; set; }
        public string ApiKey { get; set; }
        public string OriginSiteId { get; set; }
        public string OriginConnectionId { get; set; }
        public string Identifier { get; set; }
        public string Data { get; set; }
    }

    public class RelayResponse
    {
        public SiteOperation Operation { get; set; }
        public string RespondingSiteId { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public string OriginSiteId { get; set; }
        public string OriginConnectionId { get; set; }
        public string Identifier { get; set; }
        public dynamic Data { get; set; }

        public RelayResponse()
        {
            Success = true;
        }
    }
}
