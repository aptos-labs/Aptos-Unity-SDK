using Newtonsoft.Json;

namespace Aptos.Unity.Rest.Model
{
    /// <summary>
    /// Represents Transaction Payload
    /// https://fullnode.mainnet.aptoslabs.com/v1/spec#/schemas/TransactionPayload
    /// NOTE: Arguments is serialized to a single object that contains arrays of different types
    /// </summary>
    [JsonObject]
    public class TransactionPayload
    {
        [JsonProperty("type", Required = Required.Always)]
        public string Type { get; set; }

        [JsonProperty("function", Required = Required.Always)]
        public string Function { get; set; }

        [JsonProperty("type_arguments", Required = Required.Always)]
        public string[] TypeArguments { get; set; }

        [JsonProperty("arguments", Required = Required.AllowNull)]
        public Arguments Arguments { get; set; }
    }

    [JsonObject]
    public class Arguments
    {
        [JsonProperty("arguments", Required = Required.AllowNull)]
        public string[] ArgumentStrings { get; set; }
        [JsonProperty("arguments", Required = Required.AllowNull)]
        public bool[] MutateSettings { get; set; }
        [JsonProperty("arguments", Required = Required.AllowNull)]
        public string[] PropertyKeys { get; set; }
        [JsonProperty("arguments", Required = Required.AllowNull)]
        public int[] PropertyValues { get; set; } // TODO: See Data Type for Property Values; array<string<hex>>
        [JsonProperty("arguments", Required = Required.AllowNull)]
        public string[] PropertyTypes { get; set; }
    }
}