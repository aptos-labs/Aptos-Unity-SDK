using Newtonsoft.Json;

namespace Aptos.Unity.Rest.Model.Resources {
    [JsonObject]
    public interface IResourceBase
    {
        [JsonProperty("type", Required = Required.Always)]
        public string Type { get; set; }
    }
}