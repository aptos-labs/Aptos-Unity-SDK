using Newtonsoft.Json;

namespace Aptos.Unity.Rest.Model.Resources
{
    [JsonObject]
    public class RoyaltyResource : IResourceBase
    {
        [JsonProperty("type", Required = Required.Always)]
        public string Type { get; set; }

        [JsonProperty("data", Required = Required.Always)]
        public RoyaltyResourceData Data { get; set; }
    }

    [JsonObject]
    public class RoyaltyResourceData: ResourceDataBase
    {
        [JsonProperty("denominator", Required = Required.Always)]
        public string Denominator { get; set; }

        [JsonProperty("numerator", Required = Required.Always)]
        public string Numerator { get; set; }

        [JsonProperty("payee_address", Required = Required.Always)]
        public string PayeeAddress { get; set; }
    }
}