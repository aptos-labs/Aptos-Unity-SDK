using Newtonsoft.Json;

namespace Aptos.Unity.Rest.Model
{
    /// <summary>
    /// A representation a token_id object
    /// {
    ///     "token_data_id":{
    ///         "creator":"0xcd7820caacab04fbf1d7e563f4d329f06d2ce3140d654729d99258b5b68a27bf",
    ///         "collection":"Alice's",
    ///         "name":"Alice's first token"
    ///     },
    ///     "property_version":"0"
    /// }
    /// </summary>
    [JsonObject]
    public class TokenIdRequest
    {
        [JsonProperty("token_data_id", Required = Required.Always)]
        public TokenDataId TokenDataId { get; set; }

        [JsonProperty("property_version", Required = Required.Always)]
        public string PropertyVersion { get; set; }
    }
}