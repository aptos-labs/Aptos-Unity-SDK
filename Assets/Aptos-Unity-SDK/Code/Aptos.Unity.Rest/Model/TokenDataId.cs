using Newtonsoft.Json;

namespace Aptos.Unity.Rest.Model
{
    [JsonObject]
    public class TokenDataId
    {
        [JsonProperty("collection", Required = Required.AllowNull)]
        public string Collection { get; set; }

        [JsonProperty("creator", Required = Required.AllowNull)]
        public string Creator { get; set; }

        [JsonProperty("name", Required = Required.AllowNull)]
        public string Name { get; set; }
    }
}