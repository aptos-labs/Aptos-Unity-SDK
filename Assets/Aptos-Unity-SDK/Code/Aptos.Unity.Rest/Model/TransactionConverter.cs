using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Aptos.Unity.Rest.Model
{
    /// <summary>
    /// Converter to serialize and deserialize a Transaction
    /// </summary>
    public class TransactionConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Transaction);
        }

        ///<inheritdoc cref="JsonConverter.ReadJson(JsonReader, Type, object?, JsonSerializer)"/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartObject)
            {
                JObject item = JObject.Load(reader);

                // NOTE: failed transactions don't have a "type";
                // There's the submission response, and there's the VM response, e.g. user_transaction, transaction_pending
                if (item["type"] == null && item["hash"] != null)
                {
                    TransactionRequest transactionRequest = JsonConvert.DeserializeObject<TransactionRequest>(item.ToString(), new TransactionRequestConverter());
                    Transaction transaction = new Transaction(transactionRequest);
                    transaction.Hash = (string)item["hash"];
                    return transaction;
                }

                else if (item["type"] != null)
                {
                    string type = (string)item["type"];

                    if (type.Equals("pending_transaction"))
                    {
                        TransactionRequest transactionRequest = JsonConvert.DeserializeObject<TransactionRequest>(item.ToString(), new TransactionRequestConverter());
                        Transaction transaction = new Transaction(transactionRequest);
                        transaction.Type = type;

                        if (item["hash"] != null) transaction.Hash = (string)item["hash"];

                        return transaction;
                    }

                    else if (type.Equals("user_transaction"))
                    {
                        TransactionRequest transactionRequest = JsonConvert.DeserializeObject<TransactionRequest>(item.ToString(), new TransactionRequestConverter());
                        Transaction transaction = new Transaction(transactionRequest);
                        transaction.Type = type;

                        if (item["hash"] != null) transaction.Hash = (string)item["hash"];
                        if (item["state_change_hash"] != null) transaction.StateChangeHash = (string)item["state_change_hash"];
                        if (item["event_root_hash"] != null) transaction.EventRootHash = (string)item["event_root_hash"];
                        if (item["state_checkpoint_hash"] != null) transaction.StateCheckpointHash = (string)item["state_checkpoint_hash"];
                        if (item["gas_used"] != null) transaction.GasUsed = (string)item["gas_used"];
                        if (item["success"] != null) transaction.Success = (bool)item["success"];
                        if (item["vm_status"] != null) transaction.VmStatus = (string)item["vm_status"];
                        if (item["accumulator_root_hash"] != null) transaction.AccumulatorRootHash = (string)item["accumulator_root_hash"];

                        return transaction;
                    }
                }
            }
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // TODO: Implement TransactionConverter serializer (WriteJson)
            throw new NotImplementedException();
        }
    }
}