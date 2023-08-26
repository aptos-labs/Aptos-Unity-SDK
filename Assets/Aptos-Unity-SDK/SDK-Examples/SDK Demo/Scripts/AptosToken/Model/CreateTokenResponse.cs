using Newtonsoft.Json;

namespace Aptos.Unity.Sample
{
    public class CreateTokenResponse
    {
        [JsonProperty("hash", Required = Required.AllowNull)]
        public string Hash { get; set; }

        [JsonProperty("sender", Required = Required.AllowNull)]
        public string Sender { get; set; }
    }
}