using Newtonsoft.Json;

namespace Aptos.Unity.Rest.Model
{
    public class TokenDataIdRequest
    {
        [JsonProperty("creator", Required = Required.Always)]
        public string Creator { get; set; }

        [JsonProperty("collection", Required = Required.Always)]
        public string Collection { get; set; }

        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }
    }
}
