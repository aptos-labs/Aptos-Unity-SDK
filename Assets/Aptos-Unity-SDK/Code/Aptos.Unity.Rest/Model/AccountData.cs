using Newtonsoft.Json;

namespace Aptos.Unity.Rest.Model
{
    /// <summary>
    /// Represents Account Data object
    /// https://fullnode.mainnet.aptoslabs.com/v1/spec#/schemas/AccountData
    /// </summary>
    [JsonObject]
    public class AccountData
    {
        [JsonConstructor]
        public AccountData() { }

        [JsonProperty("sequence_number", Required = Required.Always)]
        public string SequenceNumber { get; private set; }

        [JsonProperty("authentication_key", Required = Required.Always)]
        public string AuthenticationKey { get; private set; }
    }
}