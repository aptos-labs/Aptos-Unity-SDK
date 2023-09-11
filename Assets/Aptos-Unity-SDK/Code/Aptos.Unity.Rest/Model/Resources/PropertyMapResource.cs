using System.Collections.Generic;
using Newtonsoft.Json;

namespace Aptos.Unity.Rest.Model.Resources
{
    [JsonObject]
    public class PropertyMapResource : IResourceBase
    {
        [JsonProperty("type", Required = Required.Always)]
        public string Type { get; set; }

        [JsonProperty("data", Required = Required.Always)]
        public PropertyMapResourceData Data { get; set; }
    }

    [JsonObject]
    public class PropertyMapResourceData: ResourceDataBase
    {
        [JsonProperty("inner", Required = Required.Always)]
        public Inner Inner { get; set; }
    }

    [JsonObject]
    public class Inner
    {
        [JsonProperty("data", Required = Required.Always)]
        public List<PropertyResource> Data { get; set; }
    }

    [JsonObject]
    public class PropertyResource
    {
        [JsonProperty("key", Required = Required.Always)]
        public string Key { get; set; }

        [JsonProperty("value", Required = Required.Always)]
        public PropertyValue Value { get; set; }
    }

    [JsonObject]
    public class PropertyValue
    {
        [JsonProperty("type", Required = Required.Always)]
        public string Type { get; set; }

        [JsonProperty("value", Required = Required.Always)]
        public string Value { get; set; }
    }
}