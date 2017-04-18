using Newtonsoft.Json;

namespace Portal.Interfaces
{
    public class DocModelBase
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "docType")]
        public DocTypes DocType { get; set; }
    }
}