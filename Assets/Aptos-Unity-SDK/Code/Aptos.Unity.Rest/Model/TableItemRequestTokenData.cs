using Newtonsoft.Json;

namespace Aptos.Unity.Rest.Model
{
    /// <summary>
    /// Represents a table item request
    /// </summary>
    [JsonObject]
    public class TableItemRequestTokenData
    {
        [JsonProperty("key_type", Required = Required.Always)]
        public string KeyType { get; set; }

        [JsonProperty("value_type", Required = Required.Always)]
        public string ValueType { get; set; }

        [JsonProperty("key", Required = Required.Always)]
        public TokenDataId Key { get; set; }
    }
}