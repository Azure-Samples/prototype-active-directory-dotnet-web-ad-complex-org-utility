using System;
using Newtonsoft.Json;
using ADSync.Common.Interfaces;
using ADSync.Common.Enums;

namespace ADSync.Common.Models
{
    public class StagedUser : DocModelBase, IDocModelBase
    {
        [JsonProperty(PropertyName = "upn")]
        public string Upn { get; set; }

        [JsonProperty(PropertyName = "domainName")]
        public string DomainName { get; set; }

        [JsonProperty(PropertyName = "siteId")]
        public string SiteId { get; set; }

        [JsonProperty(PropertyName = "siteType")]
        public SiteTypes SiteType { get; set; }

        [JsonProperty(PropertyName = "masterGuid")]
        public string MasterGuid { get; set; }

        [JsonProperty(PropertyName = "localGuid")]
        public string LocalGuid { get; set; }

        [JsonProperty(PropertyName = "createDate")]
        public DateTime CreateDate { get; set; }

        [JsonProperty(PropertyName = "loadState")]
        public LoadStageEnum LoadState { get; set; }

        [JsonProperty(PropertyName = "department")]
        public string Department { get; set; }

        [JsonProperty(PropertyName = "mobile")]
        public string Mobile { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "telephoneNumber")]
        public string TelephoneNumber { get; set; }

        [JsonProperty(PropertyName = "homePhone")]
        public string HomePhone { get; set; }

        [JsonProperty(PropertyName = "postalCode")]
        public string PostalCode { get; set; }

        [JsonProperty(PropertyName = "mail")]
        public string Mail { get; set; }

        [JsonProperty(PropertyName = "surname")]
        public string Surname {get; set; }

        [JsonProperty(PropertyName = "givenName")]
        public string GivenName { get; set; }

        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "streetAddress")]
        public string StreetAddress { get; set; }

        [JsonProperty(PropertyName = "city")]
        public string City { get; set; }

        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }

        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }

        public StagedUser()
        {
            CreateDate = DateTime.UtcNow;
        }
    }
}