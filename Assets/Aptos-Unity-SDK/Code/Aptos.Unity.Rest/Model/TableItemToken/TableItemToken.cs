
using Newtonsoft.Json;

namespace Aptos.Unity.Rest.Model
{
    /// <summary>
    /// https://fullnode.mainnet.aptoslabs.com/v1/spec#/operations/get_table_item
    /// </summary>
    [JsonObject]
    public class TableItemToken
    {
        [JsonProperty("amount", Required = Required.Always)]
        public string Amount { get; set; }

        [JsonProperty("id", Required = Required.Always)]
        public Id Id { get; set; }

        [JsonProperty("token_properties", Required = Required.AllowNull)]
        public TokenProperties TokenProperties { get; set; }
    }

    [JsonObject]
    public class Id

    {
        [JsonProperty("token_data_id", Required = Required.AllowNull)]
        public TokenDataId TokenDataId { get; set; }

        [JsonProperty("property_version", Required = Required.AllowNull)]
        public string PropertyVersion { get; set; }
    }

    [JsonObject]
    public class TokenProperties
    {
        [JsonProperty("map", Required = Required.AllowNull)]
        public Map Map { get; set; }
    }

    [JsonObject]
    public class Map
    {
        [JsonProperty("data", Required = Required.AllowNull)]
        public string [] Data { get; set; }
    }
}
