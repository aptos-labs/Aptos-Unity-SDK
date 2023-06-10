using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Aptos.Unity.Sample
{
    /// <summary>
    /// {
    ///     "hash":"0x08034efcfe039f224912a8bafb24f4f5e7138cac51e53c07d783ecce92504fb3",
    ///     "sender":"0xbc6cbac893ce68e440241e824d48da2fdc486529707f60dcd42486dfc2de7030",
    ///     "sequence_number":"1",
    ///     "max_gas_amount":"100000",
    ///     "gas_unit_price":"100",
    ///     "expiration_timestamp_secs":"1686357501",
    ///     "payload":{
    ///         "function":"0x4::aptos_token::mint",
    ///         "type_arguments":[
    ///         ],
    ///         "arguments":[
    ///             "Alice's",
    ///             "Alice's simple token",
    ///             "Alice's first token",
    ///             "https://aptos.dev/img/nyan.jpeg",
    ///             [
    ///                 "string"
    ///             ],
    ///             [
    ///                 "0x1::string::String"
    ///             ],
    ///             [
    ///                 "0x0c737472696e672076616c7565"
    ///             ]
    ///         ],
    ///         "type":"entry_function_payload"
    ///     },
    ///     "signature":{
    ///         "public_key":"0x32312ab881e0d177c4ee4820a309da8e446be3f6763d615b511abf9493e4f40e",
    ///         "signature":"0xc8cd9bab2edde18b5f6002f9ea7c5968d512047e381e8e78f76a2acbc045d59fa11ecdb5a5ebde08d7beea05c373a148376de468a45263dcfde93b8e067b8104",
    ///         "type":"ed25519_signature"
    ///     }
    /// }
    /// </summary>
    public class MintTokenResponse : MonoBehaviour
    {
        [JsonProperty("hash", Required = Required.AllowNull)]
        public string Hash { get; set; }

        [JsonProperty("sender", Required = Required.AllowNull)]
        public string Sender { get; set; }
    }
}