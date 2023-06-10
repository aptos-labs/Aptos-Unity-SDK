using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Aptos.Unity.Rest.Model.Resources
{
    /// <summary>
    ///
    /// {
    ///     "type":"0x1::object::ObjectCore",
    ///     "data":{
    ///         "allow_ungated_transfer":false,
    ///         "guid_creation_num":"1125899906842628",
    ///         "owner":"0x5a5a71a09e33e6cefbc084c41a854ba440e5ecf304158482f606a00d716afed8",
    ///         "transfer_events":{
    ///             "counter":"0",
    ///             "guid":{
    ///                 "id":{
    ///                     "addr":"0xe0f9ff3281477d787365fec2531ba0ffc01b272ee692dfd2eb49839d893e9771",
    ///                     "creation_num":"1125899906842624"
    ///                 }
    ///             }
    ///         }
    ///     }
    /// }
    /// </summary>
    [JsonObject]
    public class ObjectResource : IResourceBase
    {
        [JsonProperty("type", Required = Required.Always)]
        public string Type { get; set; }

        [JsonProperty("data", Required = Required.Always)]
        public ObjectResourceData Data { get; set; }
    }

    [JsonObject]
    public class ObjectResourceData : ResourceDataBase
    {
        [JsonProperty("allow_ungated_transfer", Required = Required.Always)]
        public bool AllowUngatedTransfer { get; set; }

        [JsonProperty("guid_creation_num", Required = Required.Always)]
        public string GuidCreationNum { get; set; }

        [JsonProperty("owner", Required = Required.Always)]
        public string Owner { get; set; }

        [JsonProperty("transfer_events", Required = Required.Always)]
        public ResourceEvent TransferEvets { get; set; }
    }
}