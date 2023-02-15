using Newtonsoft.Json;

namespace Aptos.Unity.Rest.Model
{
    [JsonObject]
    public class TableItemTokenMetadata
    {
        [JsonProperty("default_properties", Required = Required.Always)]
        public TokenProperties TokenProperties { get; set; }

        [JsonProperty("description", Required = Required.Always)]
        public string Description { get; set; }

        [JsonProperty("largest_property_version", Required = Required.Always)]
        public string LargestPropertyVersion { get; set; }

        [JsonProperty("maximum", Required = Required.Always)]
        public string Maximum { get; set; }

        [JsonProperty("mutability_config", Required = Required.Always)]
        public MutabilityConfig MutabilityConfig { get; set; }

        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("royalty", Required = Required.Always)]
        public Royalty Royalty { get; set; }

        [JsonProperty("supply", Required = Required.Always)]
        public string Supply { get; set; }

        [JsonProperty("uri", Required = Required.Always)]
        public string Uri { get; set; }
    }

    [JsonObject]
    public class MutabilityConfig
    {
        [JsonProperty("description", Required = Required.Always)]
        public bool Description { get; set; }

        [JsonProperty("maximum", Required = Required.Always)]
        public bool Maximum { get; set; }

        [JsonProperty("properties", Required = Required.Always)]
        public bool Properties { get; set; }

        [JsonProperty("royalty", Required = Required.Always)]
        public bool Royalty { get; set; }

        [JsonProperty("uri", Required = Required.Always)]
        public bool Uri { get; set; }
    }

    [JsonObject]
    public class Royalty
    {
        [JsonProperty("payee_address", Required = Required.Always)]
        public string PayeeAddress { get; set; }

        [JsonProperty("royalty_points_denominator", Required = Required.Always)]
        public string RoyaltyPointsDenominator { get; set; }

        [JsonProperty("royalty_points_numerator", Required = Required.Always)]
        public string RoyaltyPointsNumerator { get; set; }
    }
}