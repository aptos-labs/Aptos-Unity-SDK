using Newtonsoft.Json;

namespace Aptos.Unity.Sample
{
    /// <summary>
    /// {
    ///     "hash":"0xcdeab82b0deae8d5bae279e7a67da6e3ee5888f36c15e06b75e6bf3e48c01582",
    ///     "sender":"0x688b65329660b8625096f0115b2129cb06c1fdd570cd89c58dd7d9fe470f21a5",
    ///     "sequence_number":"2",
    ///     "max_gas_amount":"100000",
    ///     "gas_unit_price":"100",
    ///     "expiration_timestamp_secs":"1686736761",
    ///     "payload":{
    ///         "function":"0x4::aptos_token::add_property",
    ///         "type_arguments":[
    ///             "0x4::token::Token"
    ///         ],
    ///         "arguments":[
    ///             {
    ///                 "inner":"0x885b89351cfb1bf747a12bc859f1d329ce50b7ecb48f5b8e7d28ab92b51b87a2"
    ///             },
    ///             "test",
    ///             "bool",
    ///             "0x00"
    ///         ],
    ///         "type":"entry_function_payload"
    ///     },
    ///     "signature":{
    ///         "public_key":"0xdcc00fced1d34a826154990a09ee713be71b4c3d6d595c9e19b9119356cd10d1",
    ///         "signature":"0x3bddc102209cba7b3127b075d3f20473d09f004f931290337179de112dea689897e1659baa08b5ce6230781ddc452e14c325e878f3e7befb817da0f1cfebf302",
    ///         "type":"ed25519_signature"
    ///     }
    /// }
    /// </summary>
    public class AddTokenPropertyResponse
    {
        [JsonProperty("hash", Required = Required.AllowNull)]
        public string Hash { get; set; }

        [JsonProperty("sender", Required = Required.AllowNull)]
        public string Sender { get; set; }
    }
}