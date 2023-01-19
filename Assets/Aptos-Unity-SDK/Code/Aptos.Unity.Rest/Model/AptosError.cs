using Newtonsoft.Json;

namespace Aptos.Unity.Rest.Model
{
    /// <summary>
    /// Represents an error return from REST service
    /// </summary>
    public class AptosError
    {
        [JsonProperty("message", Required = Required.Always)]
        public string Message { get; private set; }

        [JsonProperty("error_code", Required = Required.Always)]
        public string ErrorCode { get; private set; }

        [JsonProperty("vm_error_code", Required = Required.Always)]
        public string VMErrorCode { get; private set; }
    }
}