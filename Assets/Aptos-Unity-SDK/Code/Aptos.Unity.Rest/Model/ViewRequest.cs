using Newtonsoft.Json;

namespace Aptos.Unity.Rest.Model
{
    /// <summary>
    /// Represents a view function request
    /// NOTE: Arguments is serialized to a single object that contains arrays of different types
    /// </summary>
    [JsonObject]
    public class ViewRequest
    {
        [JsonProperty("function", Required = Required.Always)]
        public string Function { get; set; }

        [JsonProperty("type_arguments", Required = Required.Always)]
        public string[] TypeArguments { get; set; }

        [JsonProperty("arguments", Required = Required.AllowNull)]
        public Arguments Arguments { get; set; }
    }
}
