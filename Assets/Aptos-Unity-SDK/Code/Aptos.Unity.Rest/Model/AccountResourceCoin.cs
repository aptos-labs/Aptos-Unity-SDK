using Newtonsoft.Json;

namespace Aptos.Unity.Rest.Model
{
    /// <summary>
    /// Represents Account Resource for Aptos Coin
    /// https://fullnode.mainnet.aptoslabs.com/v1/spec#/schemas/MoveResource
    /// </summary>
    [JsonObject]
    public class AccountResourceCoin
    {
        [JsonConstructor]
        public AccountResourceCoin() { }

        [JsonProperty("type", Required = Required.Always)]
        public string Type { get; set; }

        [JsonProperty("data", Required = Required.Always)]
        public Data DataProp { get; set; }

        [JsonObject]
        public class Data
        {
            [JsonProperty("coin", Required = Required.Always)]
            public Coin Coin { get; set; }

            [JsonProperty("deposit_events", Required = Required.Always)]
            public DespositEvents DepositEvents { get; set; }

            [JsonProperty("frozen", Required = Required.Always)]
            public string Frozen { get; private set; }

            [JsonProperty("withdraw_events", Required = Required.Always)]
            public DespositEvents WithdrawEvents { get; private set; }
        }

        [JsonObject]
        public class Coin
        {
            [JsonProperty("value", Required = Required.Always)]
            public string Value { get; set; }
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
        private class WithdrawEvents
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