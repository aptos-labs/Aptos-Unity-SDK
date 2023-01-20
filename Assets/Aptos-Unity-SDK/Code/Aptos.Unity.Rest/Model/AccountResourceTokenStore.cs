using Newtonsoft.Json;

namespace Aptos.Unity.Rest.Model
{
    /// <summary>
    /// Represents Account Resource for Aptos Coin
    /// https://fullnode.mainnet.aptoslabs.com/v1/spec#/schemas/MoveResource
    /// </summary>
    [JsonObject]
    public class AccountResourceTokenStore
    {
        [JsonConstructor]
        public AccountResourceTokenStore() { }

        [JsonProperty("type", Required = Required.Always)]
        public string Type { get; set; }

        [JsonProperty("data", Required = Required.Always)]
        public Data DataProp { get; set; }

        [JsonObject]
        public class Data
        {
            [JsonProperty("burn_events", Required = Required.Always)]
            public BurnEvents BurnEvent { get; set; }

            [JsonProperty("deposit_events", Required = Required.Always)]
            public DespositEvents DepositEvents { get; set; }

            [JsonProperty("direct_transfer", Required = Required.Always)]
            public string DirectTransfer { get; set; }

            [JsonProperty("mutate_token_property_events", Required = Required.Always)]
            public MutateTokenPropertyEvents MutateTokenPropertyEvents { get; set; }

            [JsonProperty("tokens", Required = Required.Always)]
            public Tokens Tokens { get; set; }

            [JsonProperty("withdraw_events", Required = Required.Always)]
            public WithdrawEvents WithdrawEvents { get; private set; }
        }

        [JsonObject]
        public class BurnEvents
        {
            [JsonProperty("counter", Required = Required.Always)]
            public string Counter { get; private set; }

            [JsonProperty("guid", Required = Required.Always)]
            public Guid Guid { get; private set; }
        }

        [JsonObject]
        public class DespositEvents
        {
            [JsonProperty("counter", Required = Required.Always)]
            public string Counter { get; private set; }

            [JsonProperty("guid", Required = Required.Always)]
            public Guid Guid { get; private set; }
        }

        [JsonObject]
        public class MutateTokenPropertyEvents
        {
            [JsonProperty("counter", Required = Required.Always)]
            public string Counter { get; private set; }

            [JsonProperty("guid", Required = Required.Always)]
            public Guid Guid { get; private set; }
        }

        [JsonObject]
        public class Tokens
        {
            [JsonProperty("handle", Required = Required.Always)]
            public string Handle { get; private set; }
        }

        [JsonObject]
        public class WithdrawEvents
        {
            [JsonProperty("counter", Required = Required.Always)]
            public string Counter { get; private set; }

            [JsonProperty("guid", Required = Required.Always)]
            public Guid Guid { get; private set; }
        }


        [JsonObject]
        public class Guid
        {
            [JsonProperty("id", Required = Required.Always)]
            public Id Id { get; private set; }
        }

        [JsonObject]
        public class Id
        {
            [JsonProperty("addr", Required = Required.Always)]
            public string Addr { get; private set; }

            [JsonProperty("creation_num", Required = Required.Always)]
            public string CreationNum { get; private set; }
        }
    }
}