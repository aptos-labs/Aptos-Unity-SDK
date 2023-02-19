using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Aptos.Utilities.BCS
{
    public enum TypeTag
    {
        BOOL, // int = 0
        U8, // int = 1
        U64, // int = 2
        U32, // TODO: INSPECT WHERE TypeTag enum is leveraged
        U128, // int = 3
        ACCOUNT_ADDRESS, // int = 4
        SIGNER, // int = 5
        VECTOR, // int = 6
        STRUCT, // int = 7
    }

    public interface ISerializable
    {
        public void Serialize(Serialization serializer);
    }

    public interface ISerializableTag : ISerializable
    {
        public TypeTag Variant();

        public void SerializeTag(Serialization serializer)
        {
            serializer.SerializeU32AsUleb128((uint)this.Variant());
            this.Serialize(serializer);
        }
    }

    /// <summary>
    /// Representation of a tag sequence.
    /// </summary>
    public class TagSequence : ISerializable
    {
        ISerializableTag[] serializableTags;

        public TagSequence(ISerializableTag[] serializableTags)
        {
            this.serializableTags = serializableTags;
        }

        public void Serialize(Serialization serializer)
        {
            serializer.SerializeU32AsUleb128((uint)this.serializableTags.Length);
            foreach (ISerializableTag element in this.serializableTags)
            {
                element.SerializeTag(serializer);
            }
        }
    }

    /// <summary>
    /// Representation of a sequence
    /// </summary>
    public class Sequence : ISerializable
    {
        ISerializable[] values;

        public int Length
        {
            get
            {
                return values.Length;
            }
        }

        public ISerializable[] GetValues()
        {
            return values;
        }

        public Sequence(ISerializable[] serializable)
        {
            this.values = serializable;
        }

        public void Serialize(Serialization serializer)
        {
            serializer.SerializeU32AsUleb128((uint)this.values.Length);

            foreach (ISerializable element in this.values)
            {
                Type elementType = element.GetType();
                if (elementType == typeof(Sequence))
                {
                    Serialization seqSerializer = new Serialization();
                    Sequence seq = (Sequence)element;
                    seqSerializer.Serialize(seq);

                    byte[] elementsBytes = seqSerializer.GetBytes();
                    int sequenceLen = elementsBytes.Length;
                    serializer.SerializeU32AsUleb128((uint)sequenceLen);
                    serializer.SerializeFixedBytes(elementsBytes);
                }
                else // TODO: Explore this case
                {
                    Serialization s = new Serialization();
                    element.Serialize(s);
                    byte[] b = s.GetBytes();
                    serializer.SerializeBytes(b);
                }
            }
        }
    }

    /// <summary>
    /// Representation of a byte sequence.
    /// </summary>
    public class BytesSequence : ISerializable
    {
        byte[][] values;

        public BytesSequence(byte[][] values)
        {
            this.values = values;
        }

        public void Serialize(Serialization serializer)
        {
            serializer.SerializeU32AsUleb128((uint)this.values.Length);
            foreach (byte[] element in this.values)
            {
                serializer.SerializeBytes(element);
            }
        }
    }

    /// <summary>
    /// Representation of a map in BCS.
    /// </summary>
    public class BCSMap : ISerializable
    {
        Dictionary<BString, ISerializableTag> value;

        public BCSMap(Dictionary<BString, ISerializableTag> value)
        {
            this.value = value;
        }
        public void Serialize(Serialization serializer)
        {
            Serialization mapSerializer = new Serialization();
            SortedDictionary<string, (byte[], byte[])> byteMap = new SortedDictionary<string, (byte[], byte[])>();
            foreach (KeyValuePair<BString, ISerializableTag> entry in this.value)
            {
                Serialization keySerializer = new Serialization();
                entry.Key.Serialize(keySerializer);
                byte[] bKey = keySerializer.GetBytes();
                
                Serialization valSerializer = new Serialization();
                entry.Value.Serialize(valSerializer);
                byte[] bValue = valSerializer.GetBytes();

                byteMap.Add(entry.Key.value, (bKey, bValue));
            }
            mapSerializer.SerializeU32AsUleb128((uint)byteMap.Count);
            
            foreach(KeyValuePair<string, (byte[], byte[])> entry in byteMap)
            {
                mapSerializer.SerializeFixedBytes(entry.Value.Item1);
                mapSerializer.SerializeFixedBytes(entry.Value.Item2);
            }

            serializer.SerializeFixedBytes(mapSerializer.GetBytes());
        }
    }

    /// <summary>
    /// Representation of a string in BCS.
    /// </summary>
    public class BString : ISerializable
    {
        public string value;

        public BString(string value)
        {
            this.value = value;
        }

        public void Serialize(Serialization serializer)
        {
            serializer.Serialize(value);
        }
    }

    /// <summary>
    /// Representation of Bytes in BCS.
    /// </summary>
    public class Bytes : ISerializable
    {
        byte[] value;

        public Bytes(byte[] value)
        {
            this.value = value;
        }

        public void Serialize(Serialization serializer)
        {
            serializer.Serialize(value);
        }
    }

    /// <summary>
    /// Representation of a Boolean.
    /// </summary>
    public class Bool : ISerializableTag
    {
        bool value;

        public Bool(bool value)
        {
            this.value = value;
        }

        public void Serialize(Serialization serializer)
        {
            serializer.Serialize(value);
        }

        public TypeTag Variant()
        {
            return TypeTag.BOOL;
        }
    }

    /// <summary>
    /// Representation of U8.
    /// </summary>
    public class U8 : ISerializableTag
    {
        byte value;

        public U8(byte value)
        {
            this.value = value;
        }

        public TypeTag Variant()
        {
            return TypeTag.U8;
        }

        public void Serialize(Serialization serializer)
        {
            serializer.Serialize(value);
        }
    }

    /// <summary>
    /// Representation of a U32.
    /// </summary>
    public class U32 : ISerializableTag
    {
        uint value;

        public U32(uint value)
        {
            this.value = value;
        }

        public TypeTag Variant()
        {
            return TypeTag.U32;
        }

        public void Serialize(Serialization serializer)
        {
            serializer.Serialize(value);
        }
    }

    /// <summary>
    /// Representation of U64.
    /// </summary>
    public class U64 : ISerializableTag
    {
        ulong value;

        public U64(ulong value)
        {
            this.value = value;
        }

        public TypeTag Variant()
        {
            return TypeTag.U64;
        }

        public void Serialize(Serialization serializer)
        {
            serializer.Serialize(value);
        }
    }

    /// <summary>
    /// Representation of a U128.
    /// </summary>
    public class U128 : ISerializableTag
    {
        BigInteger value;

        public U128(BigInteger value)
        {
            this.value = value;
        }

        public TypeTag Variant()
        {
            return TypeTag.U128;
        }

        public void Serialize(Serialization serializer)
        {
            serializer.Serialize(value);
        }
    }

    /// <summary>
    /// Representation of an account address.
    /// </summary>
    public class AccountAddress : ISerializableTag
    {
        byte[] value;

        public AccountAddress(byte[] value)
        {
            this.value = value;
        }

        public AccountAddress(string address)
        {
            byte[] addressBytes = BigInteger
                .Parse("00" + address.Replace("0x", ""), System.Globalization.NumberStyles.HexNumber).ToByteArray()
                .Reverse().ToArray();
            this.value = new byte[32];
            // left the bytezz array with 0's to make it 32 bytes long
            Array.Copy(addressBytes, 0, this.value, 32 - addressBytes.Length, addressBytes.Length);
        }

        public String ToHex()
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in this.value)
                sb.Append(b.ToString("X2"));
            return "0x" + sb.ToString();
        }

        public TypeTag Variant()
        {
            return TypeTag.ACCOUNT_ADDRESS;
        }

        public void Serialize(Serialization serializer)
        {
            serializer.WriteBytes(value);
        }
    }

    /// <summary>
    /// Representation of a struct tag.
    /// </summary>
    public class StructTag : ISerializableTag
    {
        AccountAddress address;
        string module;
        string name;
        ISerializableTag[] typeArgs;

        public StructTag(AccountAddress address, string module, string name, ISerializableTag[] typeArgs)
        {
            this.address = address;
            this.module = module;
            this.name = name;
            this.typeArgs = typeArgs;
        }

        public TypeTag Variant()
        {
            return TypeTag.STRUCT;
        }

        public void Serialize(Serialization serializer)
        {
            serializer.SerializeU32AsUleb128((uint)this.Variant());
            this.address.Serialize(serializer);
            serializer.Serialize(this.module);
            serializer.Serialize(this.name);
            serializer.SerializeU32AsUleb128((uint)this.typeArgs.Length);
            for (int i = 0; i < this.typeArgs.Length; i++)
            {
                this.typeArgs[i].Serialize(serializer);
            }
        }
    }
}