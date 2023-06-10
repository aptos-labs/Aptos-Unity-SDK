using Newtonsoft.Json;

namespace Aptos.Unity.Rest.Model.Resources
{
    [JsonObject]
    public class TokenResource : IResourceBase
    {
        [JsonProperty("type", Required = Required.Always)]
        public string Type { get; set; }

        [JsonProperty("data", Required = Required.Always)]
        public TokenResourceData Data { get; set; }
    }

    [JsonObject]
    public class TokenResourceData: ResourceDataBase
    {
        [JsonProperty("collection", Required = Required.Always)]
        public Collection Collection { get; set; }

        [JsonProperty("description", Required = Required.Always)]
        public string Description { get; set; }

        [JsonProperty("index", Required = Required.Always)]
        public string Index { get; set; }

        [JsonProperty("mutation_events", Required = Required.Always)]
        public ResourceEvent MutationEvents { get; set; }

        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("uri", Required = Required.Always)]
        public string Uri { get; set; }
    }

    [JsonObject]
    public class Collection
    {
        [JsonProperty("inner", Required = Required.Always)]
        public string Inner { get; set; }
    }
}