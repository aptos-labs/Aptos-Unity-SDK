using Newtonsoft.Json;
using System;

namespace Aptos.Unity.Rest.Model
{
    // TODO: Implement AccountResource Converter
    /// <summary>
    /// Converter to serialize and deserialize an Account Resource
    /// https://fullnode.mainnet.aptoslabs.com/v1/spec#/schemas/MoveResource
    /// TODO: Implement Account Resource Converter
    /// </summary>
    public class AccountResourceConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
