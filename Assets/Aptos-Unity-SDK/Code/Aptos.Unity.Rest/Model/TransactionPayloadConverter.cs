using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Aptos.Unity.Rest.Model
{
    public class TransactionPayloadConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(TransactionPayload));
        }

        // TODO: Implement ReadJson TransactionPayloadConverter
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            TransactionPayload transactionPayload = (TransactionPayload)value;
            JObject oTransactionPayload = new JObject();

            JArray jTypeArguments = new JArray();
            jTypeArguments.Add(transactionPayload.TypeArguments);

            JArray jArguments = JArray.FromObject(transactionPayload.Arguments.ArgumentStrings);

            if (transactionPayload.Arguments.MutateSettings != null)
            {
                JArray jBoolArray = JArray.FromObject(transactionPayload.Arguments.MutateSettings);
                jArguments.Add(jBoolArray);
            }

            oTransactionPayload.Add("type", transactionPayload.Type);
            oTransactionPayload.Add("function", transactionPayload.Function);
            oTransactionPayload.Add("type_arguments", jTypeArguments);
            oTransactionPayload.Add("arguments", jArguments);

            oTransactionPayload.WriteTo(writer);
        }
    }
}