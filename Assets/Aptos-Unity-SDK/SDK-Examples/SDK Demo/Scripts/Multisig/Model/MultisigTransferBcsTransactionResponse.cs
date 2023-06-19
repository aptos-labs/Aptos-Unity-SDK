using Newtonsoft.Json;

namespace Aptos.Unity.Sample
{
    /// <summary>
    /// {
    ///     "hash":"0x397d8a96f34d6e08c1818e0425036e11557265e5f87205fa01a15040c65fbf4e",
    ///     "sender":"0x408f9998cbedd637aaa9acd8031cd5c73d79cd15d309e5ec36b997af82407383",
    ///     "sequence_number":"0",
    ///     "max_gas_amount":"100000",
    ///     "gas_unit_price":"100",
    ///     "expiration_timestamp_secs":"1687157711",
    ///     "payload":{
    ///         "function":"0x1::coin::transfer",
    ///         "type_arguments":[
    ///             "0x1::aptos_coin::AptosCoin"
    ///         ],
    ///         "arguments":[
    ///             "0xf3a6802af5e0c0fceb1980d4f945f2de49a3e4c52b2ef93e85ec9209a15bb7f3",
    ///             "100"
    ///         ],
    ///         "type":"entry_function_payload"
    ///     },
    ///     "signature":{
    ///         "public_keys":[
    ///             "0x0571cde5c8b5a7d4af586d313452a933abffba694a4d23c82c337c8ccb97d328",
    ///             "0xfb06f5dca7b4472196da74bb24d72f2eaee533d48ddd40e21d510b89c21384a9",
    ///             "0x885b838245fc87a9b5c8189807ada21af8a8a653167209abf4ee9edddb6b1bdc"
    ///         ],
    ///         "signatures":[
    ///             "0xcbdfa34a17ed6e298ce9f34e0f81d624f7a09f59dfe270ece8722825e8be2130c3000d15fd636f851ee199982e7de20a1170343c800e55ade2f1e2007294c209",
    ///             "0x28c145a3feee5c986aeb6ad638b2b89e4364a357b3f9cd5a906723f85937929b6a72b3353e5add3bb10662d04bb28fd93baa7975322673ef07016044d5268902"
    ///         ],
    ///         "threshold":2,
    ///         "bitmap":"0xc0000000",
    ///         "type":"multi_ed25519_signature"
    ///     }
    /// }
    /// </summary>
    [JsonObject]
    public class MultisigTransferBcsTransactionResponse
    {
        [JsonProperty("hash", Required = Required.AllowNull)]
        public string Hash { get; set; }

        [JsonProperty("sender", Required = Required.AllowNull)]
        public string Sender { get; set; }
    }
}

