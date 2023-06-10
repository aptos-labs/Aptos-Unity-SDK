using Newtonsoft.Json;

namespace Aptos.Unity.Sample
{
    /// <summary>
    /// {
    ///     "hash":"0xf09698659eccc921ff51218f0643656a00752718776262cb38da0cbe2674ee14",
    ///     "sender":"0x76c082bf7293aaec5cd1f074cf97d8a2c4fb9eceaef025314c9e134ca0c38088",
    ///     "sequence_number":"0",
    ///     "max_gas_amount":"100000",
    ///     "gas_unit_price":"100",
    ///     "expiration_timestamp_secs":"1686351491",
    ///     "payload":{
    ///         "function":"0x4::aptos_token::create_collection",
    ///         "type_arguments":[
    ///         ],
    ///         "arguments":[
    ///             "Alice's simple collection",
    ///             "1",
    ///             "Alice's",
    ///             "https://aptos.dev",
    ///             true,
    ///             true,
    ///             true,
    ///             true,
    ///             true,
    ///             true,
    ///             true,
    ///             true,
    ///             true,
    ///             "0",
    ///             "1"
    ///         ],
    ///         "type":"entry_function_payload"
    ///     },
    ///     "signature":{
    ///         "public_key":"0x48e78c4fdf91f4614ddb73074b25370ca83550362d5cfcfa821b3da56f597de0",
    ///         "signature":"0xdf066d12d5f7462a07c9c951e25fc5d240992c6db23baf73db991a93c52f4d05c8ba22cc4ff14efb24d83b910d8a78137ead09046287e6cb22e00989b39b560b",
    ///         "type":"ed25519_signature"
    ///     }
    /// }
    /// </summary>
    [JsonObject]
    public class CreateCollectionResponse
    {
        [JsonProperty("hash", Required = Required.AllowNull)]
        public string Hash { get; set; }

        [JsonProperty("sender", Required = Required.AllowNull)]
        public string Sender { get; set; }
    }
}