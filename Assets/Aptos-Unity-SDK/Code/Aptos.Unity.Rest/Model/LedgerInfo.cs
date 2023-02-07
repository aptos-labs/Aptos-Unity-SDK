using Newtonsoft.Json;

namespace Aptos.Unity.Rest.Model
{
    /// <summary>
    /// Represents a gas estimation response.
    /// https://fullnode.mainnet.aptoslabs.com/v1/spec#/operations/get_ledger_info
    /// </summary>
    [JsonObject]
    public class LedgerInfo
    {
        [JsonProperty("chain_id", Required = Required.Always)]
        public int ChainId { get; private set; }

        [JsonProperty("epoch", Required = Required.Always)]
        public string Epoch { get; private set; }

        [JsonProperty("ledger_version", Required = Required.Always)]
        public string LedgerVersion { get; private set; }

        [JsonProperty("oldest_ledger_version", Required = Required.Always)]
        public string OldestLedgerVersion { get; private set; }

        [JsonProperty("ledger_timestamp", Required = Required.Always)]
        public string LedgerTimestamp { get; private set; }

        [JsonProperty("node_role", Required = Required.Always)]
        public string NodeRole { get; private set; }

        [JsonProperty("oldest_block_height", Required = Required.Always)]
        public string OldestBlockHeight { get; private set; }

        [JsonProperty("block_height", Required = Required.Always)]
        public string BlockHeight { get; private set; }

        [JsonProperty("git_hash", Required = Required.Always)]
        public string GitHash { get; private set; }
    }
}