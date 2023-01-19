using Newtonsoft.Json;

namespace Aptos.Unity.Rest.Model
{
    /// <summary>
    /// Represents a Transaction Request, a subset of Transaction
    /// https://fullnode.mainnet.aptoslabs.com/v1/spec#/schemas/Transaction
    /// </summary>
    [JsonObject]
    public class TransactionRequest
    {
        public TransactionRequest() { }
        public TransactionRequest(TransactionRequest transactionRequest)
        {
            Sender = transactionRequest.Sender;
            SequenceNumber = transactionRequest.SequenceNumber;
            MaxGasAmount = transactionRequest.MaxGasAmount;
            GasUnitPrice = transactionRequest.GasUnitPrice;
            ExpirationTimestampSecs = transactionRequest.ExpirationTimestampSecs;
            Payload = transactionRequest.Payload;
            Signature = transactionRequest.Signature;
        }

        [JsonProperty("sender", Required = Required.Always)]
        public string Sender { get; set; }

        [JsonProperty("sequence_number", Required = Required.Always)]
        public string SequenceNumber { get; set; }

        [JsonProperty("max_gas_amount", Required = Required.Always)]
        public string MaxGasAmount { get; set; }

        [JsonProperty("gas_unit_price", Required = Required.Always)]
        public string GasUnitPrice { get; set; }

        [JsonProperty("expiration_timestamp_secs", Required = Required.Always)]
        public string ExpirationTimestampSecs { get; set; }

        [JsonProperty("payload", Required = Required.Always)]
        public TransactionPayload Payload { get; set; }

        [JsonProperty("signature", Required = Required.AllowNull, NullValueHandling = NullValueHandling.Ignore)]
        public SignatureData Signature { get; set; }
    }

    [JsonObject]
    public class SignatureData
    {
        [JsonProperty("type", Required = Required.Always)]
        public string Type { get; set; }

        [JsonProperty("public_key", Required = Required.Always)]
        public string PublicKey { get; set; }

        [JsonProperty("signature", Required = Required.Always)]
        public string Signature { get; set; }
    }
}