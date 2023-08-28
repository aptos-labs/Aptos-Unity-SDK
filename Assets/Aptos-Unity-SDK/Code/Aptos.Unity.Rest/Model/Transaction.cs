using Newtonsoft.Json;

namespace Aptos.Unity.Rest.Model
{
    /// <summary>
    /// Represent a Transaction response
    /// NOTE: A TransactionRequest is a subset of a Transaction
    /// https://fullnode.mainnet.aptoslabs.com/v1/spec#/schemas/Transaction
    /// </summary>
    [JsonObject]
    public class Transaction : TransactionRequest
    {
        public Transaction(TransactionRequest transactionRequest) : base(transactionRequest) { }

        public Transaction() { }

        [JsonProperty("type", Required = Required.AllowNull)]
        public string Type { get; set; }

        [JsonProperty("version", Required = Required.AllowNull)]
        public string Version { get; set; }

        [JsonProperty("hash", Required = Required.AllowNull)]
        public string Hash { get; set; }

        [JsonProperty("state_change_hash", Required = Required.AllowNull)]
        public string StateChangeHash { get; set; }

        [JsonProperty("event_root_hash", Required = Required.AllowNull)]
        public string EventRootHash { get; set; }

        [JsonProperty("state_checkpoint_hash", Required = Required.AllowNull)]
        public string StateCheckpointHash { get; set; }

        [JsonProperty("gas_used", Required = Required.AllowNull)]
        public string GasUsed { get; set; }

        [JsonProperty("success", Required = Required.AllowNull)]
        public bool Success { get; set; }

        [JsonProperty("vm_status", Required = Required.AllowNull)]
        public string VmStatus { get; set; }

        [JsonProperty("accumulator_root_hash", Required = Required.AllowNull)]
        public string AccumulatorRootHash { get; set; }

        [JsonProperty("timestamp", Required = Required.AllowNull)]
        public string Timestamp { get; set; }

        [JsonProperty("changes", Required = Required.AllowNull)]
        public Change[] Changes { get; set; }

        [JsonProperty("events", Required = Required.AllowNull)]
        public TransactionEvent[] Events { get; set; }
    }

    [JsonObject]
    public class TransactionEvent
    {
        [JsonProperty("guid", Required = Required.Always)]
        public GUIDAddress GUID { get; set; }

        [JsonProperty("sequence_number", Required = Required.Always)]
        public string SequenceNumber { get; set; }

        [JsonProperty("type", Required = Required.Always)]
        public string Type { get; set; }

        [JsonProperty("data", Required = Required.Always)]
        public DataObject Data { get; set; }

        [JsonObject]
        public class GUIDAddress
        {
            [JsonProperty("creation_number", Required = Required.Always)]
            public string CreationNumber { get; set; }

            [JsonProperty("account_address", Required = Required.Always)]
            public string AccountAddress { get; set; }
        }

        [JsonObject]
        public class DataObject
        {
            [JsonProperty("index")]
            public string Index { get; set; }

            [JsonProperty("token")]
            public string Token { get; set; }

            [JsonProperty("from")]
            public string From { get; set; }

            [JsonProperty("to")]
            public string To { get; set; }

            [JsonProperty("object")]
            public string Object { get; set; }
        }
    }

    [JsonObject]
    public class Change
    {
        [JsonProperty("type", Required = Required.Always)]
        public string Type { get; set; }

    }

    [JsonObject]
    public class ChangeWriteResource : Change
    {
        [JsonProperty("Address", Required = Required.Always)]
        public string Address { get; set; }

        [JsonProperty("state_key_hash", Required = Required.Always)]
        public string StateKeyHash { get; set; }
    }

    [JsonObject]
    public class ChangeWriteResourceAptosCoin : ChangeWriteResource { }

    [JsonObject]
    public class ChangeWriteResourceAccount : ChangeWriteResource
    {
        [JsonProperty("authentication_key", Required = Required.Always)]
        public string AuthenticationKey { get; set; }

        [JsonProperty("guid_creation_num", Required = Required.Always)]
        public string GuidCreationNum { get; set; }

        [JsonProperty("sequence_number", Required = Required.Always)]
        public string SequenceNumber { get; set; }
    }

    [JsonObject]
    public class ChangeWriteResourceWriteTableItem : Change
    {

        [JsonProperty("state_key_hash", Required = Required.Always)]
        public string StateKeyHash { get; set; }

        [JsonProperty("handle", Required = Required.Always)]
        public string Handle { get; set; }

        [JsonProperty("key", Required = Required.Always)]
        public string Key { get; set; }

        [JsonProperty("value", Required = Required.Always)]
        public string Value { get; set; }

        [JsonProperty("data", Required = Required.Always)]
        public string Data { get; set; }
    }
}