using ADSync.Common.Enums;
using Newtonsoft.Json;

namespace ADSync.Common.Models
{
    public class DocModelBase
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "docType")]
        public DocTypes DocType { get; set; }
    }
}