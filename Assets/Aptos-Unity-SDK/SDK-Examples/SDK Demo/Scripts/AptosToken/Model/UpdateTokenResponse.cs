using Newtonsoft.Json;

namespace Aptos.Unity.Sample
{
    /// <summary>
    /// {
    ///     "hash":"0x7cf5591680d23d3b4efb78cc27babc4ba8185460aeeac074bfc3bc6ced0aa765",
    ///     "sender":"0xa09c1722e1308b169f3b497f5202c971c285bd30fcd8c9c1cd27378dd6091ea5",
    ///     "sequence_number":"4",
    ///     "max_gas_amount":"100000",
    ///     "gas_unit_price":"100",
    ///     "expiration_timestamp_secs":"1686771862",
    ///     "payload":{
    ///         "function":"0x4::aptos_token::update_property",
    ///         "type_arguments":[
    ///             "0x4::token::Token"
    ///         ],
    ///         "arguments":[
    ///             {
    ///                 "inner":"0x20f8fae146f660d2244fe0c1780dff715e15fbcfbb090fa97cfdb2ad514764ae"
    ///             },
    ///             "test",
    ///             "bool",
    ///             "0x01"
    ///         ],
    ///         "type":"entry_function_payload"
    ///     },
    ///     "signature":{
    ///         "public_key":"0x447df93a610a554121868277ef0217d5b1c46dfae7949729bf492da0aaebac80",
    ///         "signature":"0x492c53745852c0a4583ec5f1ef9d7c3d1cea63fe078d64c9ed8ccdecc79a76dd39bc8c26012e0f34f736522055a60e3c2605880711ece21cf66315abab87ef0e",
    ///         "type":"ed25519_signature"
    ///     }
    /// }
    /// </summary>
    public class UpdateTokenResponse
    {
        [JsonProperty("hash", Required = Required.AllowNull)]
        public string Hash { get; set; }

        [JsonProperty("sender", Required = Required.AllowNull)]
        public string Sender { get; set; }
    }

}