using ADSync.Common.Enums;
using ADSync.Common.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ADSync.Common.Models
{
    public class RemoteSite: DocModelBase, IDocModelBase
    {
        [JsonProperty(PropertyName = "siteDomain")]
        [DisplayName("Site Domains")]
        public List<string> SiteDomains { get; set; }

        [JsonProperty(PropertyName = "siteType")]
        [DisplayName("Site Type")]
        public SiteTypes SiteType { get; set; }

        [JsonProperty(PropertyName = "apiKey")]
        [DisplayName("API Key")]
        public string ApiKey { get; set; }

        [JsonProperty(PropertyName = "onPremDomainName")]
        [DisplayName("On-premises Domain Name")]
        [Description("FQDN of on-prem domain, resolvable via DNS from agent server")]
        [Required(ErrorMessage = "On-prem domain name is required")]
        public string OnPremDomainName { get; set; }

        [JsonProperty(PropertyName = "b2bRedirectUrl")]
        [DisplayName("B2B Redirect Url")]
        public string B2BRedirectUrl { get; set; }

        public RemoteSite()
        {
            SiteDomains = new List<string>();
        }
    }
}