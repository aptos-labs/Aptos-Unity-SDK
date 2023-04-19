using Aptos.Accounts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aptos.Utilities.BCS
{
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
            //string hashString = BitConverter.ToString(result).Replace("-", "").ToLowerInvariant();
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
            //throw new NotImplementedException();
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
            //serializer.SerializeU8((byte)(this.chainId >> 8));
            serializer.SerializeU8((byte)this.chainId);
        }
    }

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
            //return ser.GetBytes();
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
    }

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
            {
                this.variant = TypeTag.SCRIPT;
            }

            else if (elementType == typeof(ModuleBundle))
            {
                this.variant = TypeTag.MODULE_BUNDLE;
            }

            else if (elementType == typeof(EntryFunction))
            {
                this.variant = TypeTag.SCRIPT_FUNCTION;
            }

            else
            {
                throw new Exception("Invalid type");
            }

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
    }

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
    }

    public class Script : ISerializable
    {
        public void Deserialize(Serialization serializer)
        {
            throw new NotImplementedException();
        }

        public void Serialize(Serialization serializer)
        {
            throw new NotImplementedException();
        }
    }

    public class ScriptArgument : ISerializable
    {
        public void Deserialize(Serialization serializer)
        {
            throw new NotImplementedException();
        }

        public void Serialize(Serialization serializer)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Representation of EntryFunction.
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
    }

    /// <summary>
    /// Representation of a Module ID.
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
    }

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
            //throw new NotImplementedException();
            byte[] keyed;

            Type elementType = this.authenticator.GetType();
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
    }
}
