using Newtonsoft.Json;

namespace Aptos.Unity.Sample
{
    /// <summary>
    /// {
    ///     "hash":"0x1bd549db3aafe61dbec6046f8a36c9b29845a2769fe132cf2e13a2ddb90fff5f",
    ///     "sender":"0x5c6fbe666269d3e37921758434ff579edd87897734f20d53f19023e8d22bf7b4",
    ///     "sequence_number":"0",
    ///     "max_gas_amount":"100000",
    ///     "gas_unit_price":"100",
    ///     "expiration_timestamp_secs":"1687020648",
    ///     "payload":{
    ///         "function":"0x1::account::rotate_authentication_key",
    ///         "type_arguments":[
    ///         ],
    ///         "arguments":[
    ///             0,
    ///             "0x6c36a6f2faa8dbe031e3098d6dddf87d037ece488ef39acc48b770cd0961562b",
    ///             1,
    ///             "0x2ff49471fb0ebb5a142b3d8821c96dc19e4cc2211f504ea7570f7f5c2cf622cdcb1fb790668519a89e77125c52ef978d141f36fe4a23af07a694d464734f688f6e7075703fa4abba3ce7301e7653377b5edb3649e12b51ea323cde44356d8ce802",
    ///             "0xda3aaff5ec88d89eac530882c7a3c2cd465ff07cfc7f16c5e9e36e3ec68d707b94c8164d14493c61601c9d417c30335d7fbd771ca8362deb2cf6ff4f08653408",
    ///             "0xce4400f8c9bf83f3d36887e905dcc2734d79d6074a1cdb1c4684059469e9edcc41934e98452f43b744fe0f91bb3a1deda09322b914ca9b8540f2aa5179dd3c03200646c3d04863c10c99b6767c67074d09bb296a97da331222771667c574ce6c614be5cd0090038b15098fb00cc4aef4beaff5bd583b898563d1f0474e3b740560000000"
    ///         ],
    ///         "type":"entry_function_payload"
    ///     },
    ///     "signature":{
    ///         "public_key":"0x6c36a6f2faa8dbe031e3098d6dddf87d037ece488ef39acc48b770cd0961562b",
    ///         "signature":"0xbaed3b62d221921ccce3215235bd68d2a765cf0c3105e88fb2390f7c2264207816e29d028d438ca778056af788b43cfe7aabcaefeeb222a1ab9b9ee42e12980c",
    ///         "type":"ed25519_signature"
    ///     }
    /// }
    /// </summary>
    [JsonObject]
    public class RotateKeyBcsTransactionResponse
    {
        [JsonProperty("hash", Required = Required.AllowNull)]
        public string Hash { get; set; }

        [JsonProperty("sender", Required = Required.AllowNull)]
        public string Sender { get; set; }
    }
}