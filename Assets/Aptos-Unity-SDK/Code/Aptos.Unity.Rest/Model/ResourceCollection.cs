using Newtonsoft.Json;

namespace Aptos.Unity.Rest.Model
{
    /// <summary>
    /// Represents account resource for a collection.
    /// https://fullnode.mainnet.aptoslabs.com/v1/spec#/schemas/MoveResource
    /// </summary>
    [JsonObject]
    public class ResourceCollection
    {
        [JsonConstructor]
        public ResourceCollection() { }

        [JsonProperty("type", Required = Required.Always)]
        public string Type { get; set; }

        [JsonProperty("data", Required = Required.Always)]
        public Data DataProp { get; set; }

        [JsonObject]
        public class Data
        {
            [JsonProperty("collection_data", Required = Required.Always)]
            public CollectionData CollectionData { get; set; }

            [JsonProperty("create_collection_events", Required = Required.Always)]
            public CollectionEvents CreateCollectionEvents { get; set; }

            [JsonProperty("create_token_data_events", Required = Required.Always)]
            public CollectionEvents CreateTokenDataEvents { get; set; }

            [JsonProperty("mint_token_events", Required = Required.Always)]
            public CollectionEvents MintTokenEvents { get; set; }

            [JsonProperty("token_data", Required = Required.Always)]
            public TokenData TokenData { get; set; }
        }

        [JsonObject]
        public class CollectionData
        {
            [JsonProperty("handle", Required = Required.Always)]
            public string Handle { get; set; }
        }

        [JsonObject]
        public class CollectionEvents
        {
            [JsonProperty("counter", Required = Required.Always)]
            public string Counter { get; private set; }

            [JsonProperty("guid", Required = Required.Always)]
            public Guid Guid { get; private set; }
        }

        [JsonObject]
        public class TokenData
        {
            [JsonProperty("handle", Required = Required.Always)]
            public string Handle { get; private set; }
        }

        [JsonObject]
        public class Guid
        {
            [JsonProperty("id", Required = Required.Always)]
            public Id Id { get; private set; }
        }

        [JsonObject]
        public class Id
        {
            [JsonProperty("addr", Required = Required.Always)]
            public string Addr { get; private set; }

            [JsonProperty("creation_num", Required = Required.Always)]
            public string CreationNum { get; private set; }
        }
    }
}