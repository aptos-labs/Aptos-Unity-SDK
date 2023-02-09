using Newtonsoft.Json;

namespace Aptos.Unity.Rest.Model
{
    /// <summary>
    /// Represents a table item request
    /// https://fullnode.mainnet.aptoslabs.com/v1/spec#/schemas/TableItemRequest
    /// </summary>
    [JsonObject]
    public class TableItemRequest
    {
        [JsonProperty("key_type", Required = Required.Always)]
        public string KeyType { get; set; }

        [JsonProperty("value_type", Required = Required.Always)]
        public string ValueType { get; set; }

        [JsonProperty("key", Required = Required.Always)]
        public string Key { get; set; }
    }
}