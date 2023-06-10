using System;
using System.Collections.Generic;
using Aptos.Unity.Rest.Model.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class ResourceBaseListConverter<T> : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return (objectType == typeof(List<T>));
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        List<IResourceBase> resources = new List<IResourceBase>();

        JToken token = JToken.Load(reader);
        JArray resourceJArray = token as JArray;

        foreach (JObject item in resourceJArray)
        {
            string type = (string)item["type"];
            JObject data = (JObject)item["data"];
            string dataJson = data.ToString();

            switch (type)
            {
                case "0x4::collection::Collection":
                    CollectionResource collectionRes = new CollectionResource();
                    collectionRes.Type = type;
                    CollectionResourceData collectionData = JsonConvert.DeserializeObject<CollectionResourceData>(dataJson);
                    collectionRes.Data = collectionData;
                    resources.Add(collectionRes);
                    break;
                case "0x1::object::ObjectCore":
                    ObjectResource objectRes = new ObjectResource();
                    objectRes.Type = type;
                    ObjectResourceData objectData = JsonConvert.DeserializeObject<ObjectResourceData>(dataJson);
                    objectRes.Data = objectData;
                    resources.Add(objectRes);
                    break;
                case "0x4::property_map::PropertyMap":
                    PropertyMapResource propMapRes = new PropertyMapResource();
                    propMapRes.Type = type;
                    PropertyMapResourceData propMapData = JsonConvert.DeserializeObject<PropertyMapResourceData>(dataJson);
                    propMapRes.Data = propMapData;
                    resources.Add(propMapRes);
                    break;
                case "0x4::royalty::Royalty":
                    RoyaltyResource royaltyRes = new RoyaltyResource();
                    royaltyRes.Type = type;
                    RoyaltyResourceData royaltyData = JsonConvert.DeserializeObject<RoyaltyResourceData>(dataJson);
                    royaltyRes.Data = royaltyData;
                    resources.Add(royaltyRes);
                    break;
                case "0x4::token::Token":
                    TokenResource tokenRes = new TokenResource();
                    tokenRes.Type = type;
                    TokenResourceData tokenData = JsonConvert.DeserializeObject<TokenResourceData>(dataJson);
                    tokenRes.Data = tokenData;
                    resources.Add(tokenRes);
                    break;
            }
        }

        return resources;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}
