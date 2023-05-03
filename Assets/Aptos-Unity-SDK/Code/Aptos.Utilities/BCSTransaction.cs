﻿using Aptos.Accounts;
using System;
using System.Reflection;
using System.Text;

namespace Aptos.Utilities.BCS
{
    /// <summary>
    /// Representation of a raw transaction.
    /// </summary>
    public class RawTransaction : ISerializable
    {
        AccountAddress sender;
        int sequenceNumber;
        TransactionPayload payload;
        int maxGasAmount;
        int gasUnitPrice;
        ulong expirationTimestampsSecs;
        int chainId;

        public RawTransaction(AccountAddress sender, int sequenceNumber, TransactionPayload payload, int maxGasAmount, int gasUnitPrice, ulong expirationTimestampsSecs, int chainId)
        {
            this.sender = sender;
            this.sequenceNumber = sequenceNumber;
            this.payload = payload;
            this.maxGasAmount = maxGasAmount;
            this.gasUnitPrice = gasUnitPrice;
            this.expirationTimestampsSecs = expirationTimestampsSecs;
            this.chainId = chainId;
        }

        public byte[] Prehash()
        {
            var hashAlgorithm = new Org.BouncyCastle.Crypto.Digests.Sha3Digest(256);
            byte[] input = Encoding.UTF8.GetBytes("APTOS::RawTransaction");
            hashAlgorithm.BlockUpdate(input, 0, input.Length);
            byte[] result = new byte[32];
            hashAlgorithm.DoFinal(result, 0);
            return result;
        }

        public byte[] Keyed()
        {
            Serialization ser = new Serialization();
            this.Serialize(ser);

            byte[] prehash = this.Prehash();
            byte[] outputBytes = ser.GetBytes();

            byte[] res = new byte[prehash.Length + outputBytes.Length];

            prehash.CopyTo(res, 0);
            outputBytes.CopyTo(res, prehash.Length);

            return res;
        }

        public Signature Sign(PrivateKey key)
        {
            return key.Sign(this.Keyed());
        }

        public bool Verify(PublicKey key, Signature signature)
        {
            //throw new NotImplementedException();
            return key.Verify(this.Keyed(), signature);
        }

        public void Serialize(Serialization serializer)
        {
            this.sender.Serialize(serializer);
            serializer.SerializeU64((ulong)this.sequenceNumber);
            this.payload.Serialize(serializer);
            serializer.SerializeU64((ulong)this.maxGasAmount);
            serializer.SerializeU64((ulong)this.gasUnitPrice);
            serializer.SerializeU64((ulong)this.expirationTimestampsSecs);
            serializer.SerializeU8((byte)this.chainId);
        }

        public static RawTransaction Deserialize(Deserialization deserializer)
        {
            return new RawTransaction(
                AccountAddress.Deserialize(deserializer),
                (int)deserializer.DeserializeU64(),
                TransactionPayload.Deserialize(deserializer),
                (int)deserializer.DeserializeU64(),
                (int)deserializer.DeserializeU64(),
                deserializer.DeserializeU64(),
                deserializer.DeserializeU8()
            );
        }

        public object GetValue()
        {
            throw new NotSupportedException();
        }

        public override bool Equals(object other)
        {
            if (other is not RawTransaction)
                throw new NotImplementedException();

            RawTransaction otherRawTx = (RawTransaction)other;

            return (
                this.sender.Equals(otherRawTx.sender)
                && this.sequenceNumber == otherRawTx.sequenceNumber
                && this.payload.Equals(otherRawTx.payload)
                && this.maxGasAmount == otherRawTx.maxGasAmount
                && this.gasUnitPrice == otherRawTx.gasUnitPrice
                && this.expirationTimestampsSecs == otherRawTx.expirationTimestampsSecs
                && this.chainId == otherRawTx.chainId
            );
        }

        public override string ToString()
        {
            String s = String.Format("RawTransaction:\n" +
                "sender: {0}\n" +
                "sequence_number: {1}\n" +
                "payload: {2}\n" +
                "max_gas_amount: {3}\n" +
                "gas_unit_price: {4}\n" +
                "expiration_timestamps_secs: {5}\n" +
                "chain_id: {6}",
                this.sender.ToString(),
                this.sequenceNumber,
                this.payload.ToString(),
                this.maxGasAmount,
                this.gasUnitPrice,
                this.expirationTimestampsSecs,
                this.chainId);


            return base.ToString();
        }
    }

    /// <summary>
    /// Representation of a multi agent raw transaction.
    /// </summary>
    public class MultiAgentRawTransaction : ISerializable
    {
        RawTransaction rawTransaction;
        Sequence secondarySigners;

        public MultiAgentRawTransaction(RawTransaction rawTransaction, Sequence secondarySigners)
        {
            this.rawTransaction = rawTransaction;
            this.secondarySigners = secondarySigners;
        }

        public RawTransaction Inner()
        {
            return this.rawTransaction;
        }

        public byte[] Prehash()
        {
            var hashAlgorithm = new Org.BouncyCastle.Crypto.Digests.Sha3Digest(256);
            byte[] input = Encoding.UTF8.GetBytes("APTOS::RawTransactionWithData");
            hashAlgorithm.BlockUpdate(input, 0, input.Length);
            byte[] result = new byte[32];
            hashAlgorithm.DoFinal(result, 0);
            return result;
        }

        public byte[] Keyed()
        {
            Serialization ser = new Serialization();

            // This is a type indicator for an enum
            ser.SerializeU8(0);
            ser.Serialize(this.rawTransaction);
            //secondarySigners.Serialize(ser); // Similar to Python: serializer.sequence_serializer
            ser.Serialize(secondarySigners); // Similar to Python: serializer.sequence(self.secondary_signers, Serializer.struct)

            byte[] prehash = this.Prehash();
            byte[] outputBytes = ser.GetBytes();
            byte[] res = new byte[prehash.Length + outputBytes.Length];

            prehash.CopyTo(res, 0);
            outputBytes.CopyTo(res, prehash.Length);

            return res;
        }

        public Signature Sign(PrivateKey key)
        {
            return key.Sign(this.Keyed());
        }

        public bool Verify(PublicKey key, Signature signature)
        {
            return key.Verify(this.Keyed(), signature);
        }

        public void Serialize(Serialization serializer)
        {
            throw new NotImplementedException();
        }

        public ISerializable Deserialize(Deserialization deserializer)
        {
            throw new NotImplementedException();
        }

        public object GetValue()
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Representation of a transaction's payload.
    /// </summary>
    public class TransactionPayload : ISerializable
    {
        public enum TypeTag
        {
            SCRIPT,
            MODULE_BUNDLE,
            SCRIPT_FUNCTION
        }

        readonly ISerializable value;
        readonly TypeTag variant;

        public TransactionPayload(ISerializable payload)
        {
            Type elementType = payload.GetType();

            if (elementType == typeof(Script))
                this.variant = TypeTag.SCRIPT;
            else if (elementType == typeof(ModuleBundle))
                this.variant = TypeTag.MODULE_BUNDLE;
            else if (elementType == typeof(EntryFunction))
                this.variant = TypeTag.SCRIPT_FUNCTION;
            else
                throw new Exception("Invalid type");

            this.value = payload;
        }

        public TypeTag Variant()
        {
            return this.variant;
        }

        public void Serialize(Serialization serializer)
        {
            serializer.SerializeU32AsUleb128((uint)this.Variant());
            this.value.Serialize(serializer);
        }

        public static TransactionPayload Deserialize(Deserialization deserializer)
        {
            TypeTag variant = (TypeTag) deserializer.DeserializeUleb128();
            ISerializable payload;

            if(variant == TypeTag.SCRIPT)
                payload = Script.Deserialize(deserializer);
            else if(variant == TypeTag.MODULE_BUNDLE)
                payload = ModuleBundle.Deserialize(deserializer);
            else if(variant == TypeTag.SCRIPT_FUNCTION)
                payload = EntryFunction.Deserialize(deserializer);
            else
                throw new Exception("Invalid type");

            return new TransactionPayload(payload);
        }

        public object GetValue()
        {
            throw new NotSupportedException();
        }

        public override bool Equals(object other)
        {
            if (other is not TransactionPayload)
                throw new NotImplementedException();

            TransactionPayload otherTxnPayload = (TransactionPayload)other;

            return this.variant == otherTxnPayload.variant && this.value.Equals(otherTxnPayload.value);
        }

        public override string ToString()
        {
            return this.value.ToString();
        }
    }

    /// <summary>
    /// Representation of a module bundle.
    /// </summary>
    public class ModuleBundle : ISerializable
    {
        public void Deserialize(Serialization serializer)
        {
            throw new NotImplementedException();
        }

        public void Serialize(Serialization serializer)
        {
            throw new NotImplementedException();
        }

        public static ISerializable Deserialize(Deserialization deserializer)
        {
            throw new NotImplementedException();
        }

        public object GetValue()
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Representation of a script passed as bytes.
    /// </summary>
    public class Script : ISerializable
    {
        private readonly byte[] code;
        private readonly TagSequence typeArgs;
        private readonly Sequence scriptArgs;

        public Script(byte[] code, TagSequence typeArgs, Sequence scriptArgs)
        {
            this.code = code;
            this.typeArgs = typeArgs;
            this.scriptArgs = scriptArgs;
        }

        public void Serialize(Serialization serializer)
        {
            serializer.SerializeBytes(this.code);
            serializer.Serialize(this.typeArgs);
            serializer.Serialize(this.scriptArgs);
        }

        //TODO: Implement Script deserialization
        public static Script Deserialize(Deserialization deserializer)
        {
            byte[] code = deserializer.ToBytes();
            TagSequence typeArgs = deserializer.DeserializeTagSequence();
            Sequence scriptArgs = deserializer.DeserializeScriptArgSequence();

            return new Script(code, typeArgs, scriptArgs);
        }

        public object GetValue()
        {
            throw new NotSupportedException();
        }

        public override bool Equals(object other)
        {
            if (other is not Script)
                throw new NotImplementedException();

            Script otherScript = (Script)other;

            // TODO: Fix comparisons in Script Equals operator
            return (
                this.code == otherScript.code
                && this.typeArgs == otherScript.typeArgs
                && this.scriptArgs == otherScript.scriptArgs
            );
        }

        //  TODO: Implement Script ToString
        public override string ToString()
        {
            return base.ToString();
        }
    }

    /// <summary>
    /// Representation of a script argument.
    /// </summary>
    public class ScriptArgument : ISerializable
    {
        public enum TypeTag
        {
            U8,
            U64,
            U128,
            ADDRESS,
            U8_VECTOR,
            BOOL,
            U16,
            U32,
            U256
        }

        TypeTag variant;
        ISerializableTag value;

        public ScriptArgument(TypeTag variant, ISerializableTag value)
        {
            if (variant < 0 || variant > TypeTag.BOOL)
            {
                throw new ArgumentException("Invalid variant");
            }

            this.variant = variant;
            this.value = value;
        }

        public void Serialize(Serialization serializer)
        {
            serializer.SerializeU8((byte)this.variant);
            if(this.variant == TypeTag.U8)
            {
                serializer.SerializeU8((byte)this.value.GetValue());
            }
            else if(this.variant == TypeTag.U16)
            {
                serializer.SerializeU16((ushort)this.value.GetValue());
            }
            else if(this.variant == TypeTag.U32)
            {
                serializer.SerializeU32((uint)this.value.GetValue());
            }
            else if(this.variant == TypeTag.U64)
            {
                serializer.SerializeU64(Convert.ToUInt64(this.value.GetValue()));
            }
            else if(this.variant == TypeTag.U128)
            {
                serializer.SerializeU128((System.Numerics.BigInteger)this.value.GetValue());
            }
            // TODO: Inquire on C# U256 support
            //else if(this.variant == TypeTag.U256)
            //{
            //    serializer.Serial
            //}
            else if(this.variant == TypeTag.ADDRESS)
            {
                serializer.Serialize((AccountAddress) this.value);
            }
            else if(this.variant == TypeTag.U8_VECTOR)
            {
                serializer.SerializeBytes((byte[])this.value.GetValue());
            }
            else if(this.variant == TypeTag.BOOL)
            {
                serializer.SerializeBool((bool)this.value.GetValue());
            }
            else
            {
                throw new ArgumentException("Invalid ScriptArgument variant " + this.variant);
            }
        }

        public static ScriptArgument Deserialize(Deserialization deserializer)
        {
            TypeTag variant = (TypeTag) deserializer.DeserializeU8();
            ISerializableTag value;

            if(variant == (int) TypeTag.U8)
            {
                value = new U8(deserializer.DeserializeU8());
            }
            // TODO: implement U16
            //else if(variant == (int) TypeTag.U16)
            //{
            //    value = deserializer.DeserializeU16();
            //}
            else if(variant == TypeTag.U32)
            {
                value = new U32(deserializer.DeserializeU32());
            }
            else if(variant == TypeTag.U64)
            {
                value = new U64(deserializer.DeserializeU64());
            }
            else if(variant == TypeTag.U128)
            {
                value = new U128(deserializer.DeserializeU128());
            }
            // TODO: implement U256
            //else if(variant == (int) TypeTag.U256)
            //{
            //    value = deserializer.DeserializeU256();
            //}
            else if (variant == TypeTag.ADDRESS)
            {
                value = AccountAddress.Deserialize(deserializer);
            }
            // TODO: Implement U8 Vector deserialization
            //else if(variant == (int) TypeTag.U8_VECTOR)
            //{

            //}
            else if(variant == TypeTag.BOOL)
            {
                value = new Bool(deserializer.DeserializeBool());
            }
            else
            {
                throw new Exception("Invalid variant");
            }

            return new ScriptArgument(variant, value);
        }

        public object GetValue()
        {
            throw new NotSupportedException();
        }

        public override bool Equals(object other)
        {
            if (other is not ScriptArgument)
                throw new NotImplementedException();

            ScriptArgument otherScriptArg = (ScriptArgument)other;

            // TODO: Check comparator and Equals operator for ISerializableTag(s)
            return (
                this.variant == otherScriptArg.variant
                && this.value == otherScriptArg.value
            );
        }

        public override string ToString()
        {
            return String.Format("[{0}] {1}", this.variant, this.value);
        }
    }

    /// <summary>
    /// Representation of an entry function.
    /// </summary>
    public class EntryFunction : ISerializable
    {
        readonly ModuleId module;
        readonly string function;
        readonly TagSequence typeArgs;
        readonly Sequence args;

        public EntryFunction(ModuleId module, string function, TagSequence typeArgs, Sequence args)
        {
            this.module = module;
            this.function = function;
            this.typeArgs = typeArgs;
            this.args = args;
        }

        public static EntryFunction Natural(ModuleId module, string function, TagSequence typeArgs, Sequence args)
        {
            return new EntryFunction(module, function, typeArgs, args);
        }

        public void Serialize(Serialization serializer)
        {
            serializer.Serialize(this.module);
            serializer.SerializeString(this.function);
            serializer.Serialize(this.typeArgs);
            args.Serialize(serializer);
        }

        // TODO: implement Entry Function deserialization
        public static EntryFunction Deserialize(Deserialization deserializer)
        {
            ModuleId module = ModuleId.Deserialize(deserializer);
            string function = deserializer.DeserializeString();
            TagSequence typeArgs = deserializer.DeserializeTagSequence();
            Sequence args = Sequence.Deserialize(deserializer);

            return new EntryFunction(module, function, typeArgs, args);
        }

        public object GetValue()
        {
            throw new NotSupportedException();
        }

        public override bool Equals(object other)
        {
            if (other is not EntryFunction)
                throw new NotImplementedException();

            EntryFunction otherEntryFunc = (EntryFunction)other;

            // TODO: Add TagSequence comparator
            // TODO: Add Sequence comparator
            return (
                this.module.Equals(otherEntryFunc.module)
                && this.function.Equals(otherEntryFunc.function)
                && this.typeArgs.Equals(otherEntryFunc.typeArgs)
                && this.args.Equals(otherEntryFunc.args)
            );
        }

        // TODO: Add TagSequence ToString
        // TODO: Add Sequence ToString
        public override string ToString()
        {
            return String.Format("{0}::{1}::<{2}>({3})", this.module, this.function, this.typeArgs.ToString(), this.args.ToString());
        }
    }

    /// <summary>
    /// Representation of a module ID.
    /// </summary>
    public class ModuleId : ISerializable
    {
        public AccountAddress address;
        public string name;

        public ModuleId(AccountAddress address, string name)
        {
            // TODO: assert on length of an address- and make it it's own type
            this.address = address;
            this.name = name;
        }

        public void Serialize(Serialization serializer)
        {
            this.address.Serialize(serializer);
            serializer.SerializeString(this.name);
        }

        public static ModuleId Deserialize(Deserialization deserializer)
        {
            AccountAddress addr = AccountAddress.Deserialize(deserializer);
            string name = deserializer.DeserializeString();

            return new ModuleId(addr, name);
        }

        public object GetValue()
        {
            throw new NotSupportedException();
        }

        public override bool Equals(object other)
        {
            if (other is not ModuleId)
                throw new NotImplementedException();

            ModuleId otherModuleId = (ModuleId)other;

            return this.address.Equals(otherModuleId.address) && this.name.Equals(otherModuleId.name);
        }

        public override string ToString()
        {
            return String.Format("{0}::{1}", this.address, this.name);
        }
    }

    public class TransactionArgument
    {
        ISerializable value;
        Type encoderType;

        public TransactionArgument(ISerializable value, Type encoderType)
        {
            this.value = value;
            this.encoderType = encoderType;
        }

        public byte[] Encode()
        {
            Serialization ser = new Serialization();
            ser.Serialize(value);
            //MethodInfo method = encoderType.GetMethod("Deserialize", new Type[] { typeof(Deserialization) });
            //ISerializableTag val = (ISerializableTag)method.Invoke(null, new[] { ser, this.value });
            return ser.GetBytes();
        }
    }

    /// <summary>
    /// Signed transaction implementation.
    /// NOTE: TransactionArgument is not implemented in this SDK, instead a Sequence object is used
    /// TODO: Add comoments regarding TransactionArgument not being implemented
    /// </summary>
    public class SignedTransaction : ISerializable
    {
        RawTransaction transaction;
        Authenticator.Authenticator authenticator;

        public SignedTransaction(RawTransaction transaction, Authenticator.Authenticator authenticator)
        {
            this.transaction = transaction;
            this.authenticator = authenticator;
        }

        public byte[] Bytes()
        {
            Serialization ser = new Serialization();
            ser.Serialize(this);
            return ser.GetBytes();
        }

        public bool Verify()
        {
            byte[] keyed;

            Type elementType = this.authenticator.GetAuthenticator().GetType();
            if (elementType == typeof(Authenticator.MultiAgentAuthenticator))
            {
                Authenticator.MultiAgentAuthenticator authenticator = (Authenticator.MultiAgentAuthenticator)this.authenticator.GetAuthenticator();
                
                MultiAgentRawTransaction transaction = new MultiAgentRawTransaction(
                    this.transaction, authenticator.SecondaryAddresses()
                );

                keyed = transaction.Keyed();
            }
            else
            {
                keyed = this.transaction.Keyed();
            }

            return this.authenticator.Verify(keyed);
        }

        public void Serialize(Serialization serializer)
        {
            this.transaction.Serialize(serializer);
            this.authenticator.Serialize(serializer);
        }

        public static SignedTransaction Deserialize(Deserialization deserializer)
        {
            RawTransaction transaction = RawTransaction.Deserialize(deserializer);
            Authenticator.Authenticator authenticator = (Authenticator.Authenticator)Authenticator.Authenticator.Deserialize(deserializer);

            return new SignedTransaction(transaction, authenticator);
        }

        public object GetValue()
        {
            throw new NotSupportedException();
        }

        public override bool Equals(object other)
        {
            if (other is not SignedTransaction)
                throw new NotImplementedException();

            SignedTransaction otherSignedTxn = (SignedTransaction)other;

            return (
                this.transaction.Equals(otherSignedTxn.transaction)
                &&this.authenticator.Equals(otherSignedTxn.authenticator)
            );
        }

        public override string ToString()
        {
            return String.Format("Transaction: {0} Authenticator: {1}", this.transaction.ToString(), this.authenticator.ToString());
        }
    }
}
