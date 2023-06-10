using Newtonsoft.Json;

namespace Aptos.Unity.Rest.Model.Resources
{
    /// <summary>
    /// {
    ///     "type":"0x4::collection::Collection",
    ///     "data":{
    ///         "creator":"0x5a5a71a09e33e6cefbc084c41a854ba440e5ecf304158482f606a00d716afed8",
    ///         "description":"Alice's simple collection",
    ///         "mutation_events":{
    ///             "counter":"0",
    ///             "guid":{
    ///                 "id":{
    ///                     "addr":"0xe0f9ff3281477d787365fec2531ba0ffc01b272ee692dfd2eb49839d893e9771",
    ///                     "creation_num":"1125899906842627"
    ///                 }
    ///             }
    ///         },
    ///         "name":"Alice's",
    ///         "uri":"https://aptos.dev"
    ///     }
    /// }
    /// </summary>
    [JsonObject]
    public class CollectionResource : IResourceBase
    {
        [JsonProperty("type", Required = Required.Always)]
        public string Type { get; set; }

        [JsonProperty("data", Required = Required.Always)]
        public CollectionResourceData Data { get; set; }
    }

    [JsonObject]
    public class CollectionResourceData : ResourceDataBase
    {
        [JsonProperty("creator", Required = Required.Always)]
        public string Creator { get; set; }

        [JsonProperty("description", Required = Required.Always)]
        public string Description { get; set; }

        [JsonProperty("mutation_events", Required = Required.Always)]
        public ResourceEvent MutationEvents { get; set; }

        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("uri", Required = Required.Always)]
        public string Uri { get; set; }
    }
}
