using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Aptos.Unity.Rest.Model.Resources
{
    /// <summary>
    /// "transfer_events":{
    ///     "counter":"0",
    ///     "guid":{
    ///         "id":{
    ///             "addr":"0xe0f9ff3281477d787365fec2531ba0ffc01b272ee692dfd2eb49839d893e9771",
    ///             "creation_num":"1125899906842624"
    ///         }
    ///     }
    /// }
    /// </summary>
    [JsonObject]
    public class ResourceEvent
    {
        [JsonProperty("counter", Required = Required.Always)]
        public string Counter { get; set; }

        [JsonProperty("guid", Required = Required.Always)]
        public GUid GUid { get; set; }
    }

    [JsonObject]
    public class GUid
    {
        [JsonProperty("id", Required = Required.Always)]
        public EventId Id { get; set; }
    }

    [JsonObject]
    public class EventId
    {
        [JsonProperty("addr", Required = Required.Always)]
        public string Address { get; set; }

        [JsonProperty("creation_num", Required = Required.Always)]
        public string CreationNumber { get; set; }
    }
}