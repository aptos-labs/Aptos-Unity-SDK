using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Aptos.Unity.Rest.Model
{
    /// <summary>
    /// Converter to serialize and deserialize a Transaction Request
    /// </summary>
    public class TransactionRequestConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(TransactionRequest));
        }

        /// <summary>
        /// Parses JSON string to a TransactionRequest object
        /// <inheritdoc cref="JsonConverter.ReadJson(JsonReader, Type, object?, JsonSerializer)"/>
        /// </summary>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            TransactionRequest transactionRequest = new TransactionRequest();
            TransactionPayload transactionPayload = new TransactionPayload();

            if (reader.TokenType == JsonToken.StartObject)
            {
                JObject item = JObject.Load(reader);

                // STEP 1: Parse through default propertes
                if (item["sender"] != null) transactionRequest.Sender = (string)item["sender"];
                if (item["sequence_number"] != null) transactionRequest.SequenceNumber = (string)item["sequence_number"];
                if (item["max_gas_amount"] != null) transactionRequest.MaxGasAmount = (string)item["max_gas_amount"];
                if (item["gas_unit_price"] != null) transactionRequest.GasUnitPrice = (string)item["gas_unit_price"];
                if (item["expiration_timestamp_secs"] != null) transactionRequest.ExpirationTimestampSecs = (string)item["expiration_timestamp_secs"];

                // STEP 2: Parse through default Payload arguments
                if (item["payload"]["function"] != null) transactionPayload.Function = (string)item["payload"]["function"];
                if (item["payload"]["type"] != null) transactionPayload.Type = (string)item["payload"]["type"];
                if (item["payload"]["type_arguments"] != null)
                {
                    JArray typeArgs = (JArray)item["payload"]["type_arguments"];
                    transactionPayload.TypeArguments = typeArgs.ToObject<string[]>();
                }

                // STEP 3: Parse Payload object and identify if there is a mix of string arguments, and an array of booleans
                if (item["payload"] != null & item["payload"]["arguments"] != null)
                {
                    JArray arguments = (JArray)item["payload"]["arguments"];
                    List<string> argumentStrings = new List<string>();
                    List<bool> argumentBooleans = new List<bool>();

                    foreach (var arg in arguments)
                    {
                        if (arg.Type == JTokenType.String)
                        {
                            string argStr = arg.Value<string>();
                            argumentStrings.Add(argStr);
                        }

                        if (arg.Type == JTokenType.Array)
                        {
                            JArray boolArr = arg as JArray;

                            foreach (var boolArg in boolArr)
                            {
                                bool b = boolArg.Value<bool>();
                                argumentBooleans.Add(b);
                            }
                        }
                        transactionPayload.Arguments = new Arguments()
                        {
                            ArgumentStrings = argumentStrings.ToArray(),
                            MutateSettings = argumentBooleans.ToArray()
                        };
                    }
                }

                // STEP 4: Parse Signature
                if (item["signature"] != null)
                {
                    SignatureData signature = new SignatureData
                    {
                        Type = (string)item["signature"]["type"],
                        PublicKey = (string)item["signature"]["public_key"],
                        Signature = (string)item["signature"]["signature"]
                    };
                    transactionRequest.Signature = signature;
                }
            }
            return transactionRequest;
        }

        /// <summary>
        /// <inheritdoc cref="JsonConverter.WriteJson(JsonWriter, object?, JsonSerializer)"/>
        /// </summary>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            TransactionRequest transactionRequest = (TransactionRequest)value;
            JObject oTransactionRequest = new JObject();
            oTransactionRequest.Add("sender", transactionRequest.Sender);
            oTransactionRequest.Add("sequence_number", transactionRequest.SequenceNumber);
            oTransactionRequest.Add("max_gas_amount", transactionRequest.MaxGasAmount);
            oTransactionRequest.Add("gas_unit_price", transactionRequest.GasUnitPrice);
            oTransactionRequest.Add("expiration_timestamp_secs", transactionRequest.ExpirationTimestampSecs);

            // PAYLOAD
            TransactionPayload payload = transactionRequest.Payload;

            JObject oPayload = new JObject();
            oPayload.Add("function", payload.Function);
            oPayload.Add("type_arguments", new JArray(payload.TypeArguments));

            string[] argsString = transactionRequest.Payload.Arguments.ArgumentStrings;
            JArray jArguments = new JArray(argsString);

            if (transactionRequest.Payload.Arguments.MutateSettings != null)
            {
                bool[] argsBool = transactionRequest.Payload.Arguments.MutateSettings;
                jArguments.Add(new JArray(argsBool));
            }

            if (transactionRequest.Payload.Arguments.PropertyKeys != null)
            {
                string[] propertyKeys = transactionRequest.Payload.Arguments.PropertyKeys;
                jArguments.Add(new JArray(propertyKeys));
            }

            if (transactionRequest.Payload.Arguments.PropertyValues != null)
            {
                int[] propertyValues = transactionRequest.Payload.Arguments.PropertyValues;
                jArguments.Add(new JArray(propertyValues));
            }

            if (transactionRequest.Payload.Arguments.PropertyTypes != null)
            {
                string[] propertyTypes = transactionRequest.Payload.Arguments.PropertyTypes;
                jArguments.Add(new JArray(propertyTypes));
            }

            oPayload.Add("arguments", jArguments);
            oPayload.Add("type", payload.Type);

            oTransactionRequest.Add("payload", oPayload);

            // SIGNATURE
            if (transactionRequest.Signature != null)
            {
                JToken signature = JToken.FromObject(transactionRequest.Signature);
                oTransactionRequest.Add("signature", signature);
            }

            // Write JObject to Write
            oTransactionRequest.WriteTo(writer);
        }
    }
}