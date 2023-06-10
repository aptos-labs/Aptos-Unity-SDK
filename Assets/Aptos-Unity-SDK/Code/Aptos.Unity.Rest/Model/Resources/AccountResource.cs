using Newtonsoft.Json;

namespace Aptos.Unity.Rest.Model.Resources
{
    [JsonObject]
    public class AccountResource
    {
        [JsonProperty("data", Required = Required.Always)]
        public AccountResourceData Data { get; set; }
    }

    [JsonObject]
    public class AccountResourceData {
        [JsonProperty("authentication_key", Required = Required.Always)]
        public string AuthenticationKey { get; set; }

        [JsonProperty("coin_register_events", Required = Required.Always)]
        public ResourceEvent CoinRegisterEvents { get; set; }

        [JsonProperty("guid_creation_num", Required = Required.Always)]
        public string GuidCreationNum { get; set; }

        [JsonProperty("key_rotation_events", Required = Required.Always)]
        public ResourceEvent KeyRotationEvents { get; set; }

        [JsonProperty("rotation_capability_offer", Required = Required.Always)]
        public CapabilityOffer RotationCapabilityOffer { get; set; }

        [JsonProperty("sequence_number", Required = Required.Always)]
        public string SequenceNumber { get; set; }

        [JsonProperty("signer_capability_offer", Required = Required.Always)]
        public CapabilityOffer SignerCapabilityOffer { get; set; }
    }
}