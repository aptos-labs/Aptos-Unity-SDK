using Aptos.Accounts;
using Aptos.HdWallet.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using UnityEngine;

namespace Aptos.BCS
{
    // See type_tag.py
    public enum TypeTag
    {
        BOOL, // int = 0
        U8, // int = 1
        U64, // int = 2
        U128, // int = 3
        ACCOUNT_ADDRESS, // int = 4
        SIGNER, // int = 5
        VECTOR, // int = 6
        STRUCT, // int = 7
        U16,
        U32,
        U256
    }

    public interface ISerializable
    {
        public void Serialize(Serialization serializer);

        public static ISerializable Deserialize(Deserialization deserializer) => throw new NotImplementedException();
    }

    public interface ISerializableTag : ISerializable
    {
        public TypeTag Variant();

        public object GetValue();

        public void SerializeTag(Serialization serializer)
        {
            this.Serialize(serializer);
        }

        public static new ISerializableTag Deserialize(Deserialization deserializer) => throw new NotImplementedException();

        public static ISerializableTag DeserializeTag(Deserialization deserializer)
        {
            TypeTag variant = (TypeTag) deserializer.DeserializeUleb128();

            if (variant == TypeTag.BOOL)
                return Bool.Deserialize(deserializer);
            else if (variant == TypeTag.U8)
                return U8.Deserialize(deserializer);
            // TODO: Implement U16
            //else if (variant == TypeTag.U16)
            //    return U16.Deserialize(deserializer);
            else if (variant == TypeTag.U32)
                return U32.Deserialize(deserializer);
            else if (variant == TypeTag.U64)
                return U64.Deserialize(deserializer);
            else if (variant == TypeTag.U128)
                return U128.Deserialize(deserializer);
            // TODO: Implement U256
            //else if (variant == TypeTag.U256)
            //    return U256.Deserialize(deserializer);
            else if (variant == TypeTag.ACCOUNT_ADDRESS)
                return AccountAddress.Deserialize(deserializer);
            else if (variant == TypeTag.SIGNER)
                throw new NotImplementedException();
            else if (variant == TypeTag.VECTOR)
                throw new NotImplementedException();
            else if (variant == TypeTag.STRUCT)
                return StructTag.Deserialize(deserializer);
                //return ISerializableTag.Deserialize(deserializer);

            throw new NotImplementedException();
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

        public static TagSequence Deserialize(Deserialization deserializer)
        {
            int length = deserializer.DeserializeUleb128();

            List <ISerializableTag> values = new List<ISerializableTag>();

            while (values.Count < length)
            {
                ISerializableTag tag = ISerializableTag.DeserializeTag(deserializer);
                values.Add(tag);
            }

            return new TagSequence(values.ToArray());
        }

        public object GetValue()
        {
            return serializableTags;
        }

        public override bool Equals(object other)
        {
            TagSequence otherTagSeq = (TagSequence)other;
            return Enumerable.SequenceEqual((ISerializableTag[])this.GetValue(), (ISerializableTag[])otherTagSeq.GetValue());
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            foreach (var tag in serializableTags)
            {
                result.Append(tag.ToString());
            }

            return result.ToString();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    /// <summary>
    /// Representation of a Transaction Argument sequence / list.
    /// NOTE: Transaction Arguments have different types hence they cannot be represented using a regular list.
    /// NOTE: This class does not implement deserialization because the developer would know the types beforehand,
    /// and hence would apply the appropriate deserialization based on the type.
    /// 
    /// Fixed and Variable Length Sequences
    /// Sequences can be made of up of any BCS supported types(even complex structures) 
    /// but all elements in the sequence must be of the same type.If the length of a sequence 
    /// is fixed and well known then BCS represents this as just the concatenation of the 
    /// serialized form of each individual element in the sequence. If the length of the sequence 
    /// can be variable, then the serialized sequence is length prefixed with a ULEB128-encoded unsigned integer 
    /// indicating the number of elements in the sequence. All variable length sequences must 
    /// be MAX_SEQUENCE_LENGTH elements long or less.
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

        public object GetValue()
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

        public static Sequence Deserialize(Deserialization deser)
        {
            int length = deser.DeserializeUleb128();
            List<ISerializable> values = new List<ISerializable>();

            while (values.Count < length)
            {
                values.Add(new Bytes(deser.ToBytes()));
            }

            return new Sequence(values.ToArray());
        }

        public override bool Equals(object other)
        {
            Sequence otherSeq = (Sequence)other;
            return Enumerable.SequenceEqual(this.values, otherSeq.values);
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            foreach (var value in values)
                result.Append(value.ToString());
            return result.ToString();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
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
                serializer.SerializeBytes(element);
        }

        public static BytesSequence Deserialize(Deserialization deserializer)
        {
            int length = deserializer.DeserializeUleb128();
            List<byte[]> bytesList = new List<byte[]>();

            while(bytesList.Count < length)
            {
                byte[] bytes = deserializer.ToBytes();
                bytesList.Add(bytes);
            }

            return new BytesSequence(bytesList.ToArray());
        }

        public object GetValue()
        {
            return values;
        }

        public override bool Equals(object other)
        {
            BytesSequence otherSeq = (BytesSequence)other;

            bool equal = true;
            for(int i = 0; i < this.values.Length; i++)
                equal = equal && Enumerable.SequenceEqual(this.values[i], otherSeq.values[i]);
            return equal;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            foreach (byte[] value in values)
                result.Append(value.ByteArrayToReadableString());
            return result.ToString();
        }
    }

    /// <summary>
    /// Representation of a map in BCS.
    /// </summary>
    public class BCSMap : ISerializable
    {
        public Dictionary<BString, ISerializable> values;

        public BCSMap(Dictionary<BString, ISerializable> values)
        {
            this.values = values;
        }

        public object GetValue()
        {
            return values;
        }

        /// <summary>
        /// Maps (Key / Value Stores)
        /// Maps are represented as a variable-length, sorted sequence of(Key, Value) tuples.
        /// Keys must be unique and the tuples sorted by increasing lexicographical order on 
        /// the BCS bytes of each key.
        /// The representation is otherwise similar to that of a variable-length sequence.
        /// In particular, it is preceded by the number of tuples, encoded in ULEB128.
        /// </summary>
        /// <param name="serializer"></param>
        public void Serialize(Serialization serializer)
        {
            Serialization mapSerializer = new Serialization();
            SortedDictionary<string, (byte[], byte[])> byteMap = new SortedDictionary<string, (byte[], byte[])>();
  
            foreach (KeyValuePair<BString, ISerializable> entry in this.values)
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

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            foreach (KeyValuePair<BString, ISerializable> entry in values)
                result.Append("(" + entry.Key.ToString() + ", " + entry.Value.ToString() + ")");
            return result.ToString();
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

        public static string Deserialize(byte[] data)
        {
            return Encoding.UTF8.GetString(data);
        }

        public static byte[] RemoveBOM(byte[] data)
        {
            var bom = Encoding.UTF8.GetPreamble();
            if (data.Length > bom.Length)
            {
                for (int i = 0; i < bom.Length; i++)
                {
                    if (data[i] != bom[i])
                        return data;
                }
            }
            return data.Skip(3).ToArray();
        }

        private static string RemoveBOM(string xml)
        {
            // https://stackoverflow.com/questions/17795167/
            // xml-loaddata-data-at-the-root-level-is-invalid-line-1-position-1
            var preamble = Encoding.UTF8.GetPreamble();
            string byteOrderMarkUtf8 = Encoding.UTF8.GetString(preamble);
            if (xml.StartsWith(byteOrderMarkUtf8))
            {
                xml = xml.Remove(0, byteOrderMarkUtf8.Length);
            }

            return xml;
        }

        public static BString Deserialize(Deserialization deserializer)
        {
            string deserStr = deserializer.DeserializeString();
            return new BString(deserStr);
        }

        public override string ToString()
        {
            return value;
        }

        public override bool Equals(object other)
        {
            if (other is not BString)
                throw new NotImplementedException();

            BString otherBString = (BString)other;

            return this.value == otherBString.value;
        }

        public override int GetHashCode() => this.value.GetHashCode();

        public object GetValue()
        {
            return value;
        }
    }

    /// <summary>
    /// Representation of Bytes in BCS.
    /// </summary>
    public class Bytes : ISerializable
    {
        byte[] values;

        public Bytes(byte[] values)
        {
            this.values = values;
        }

        public void Serialize(Serialization serializer)
        {
            serializer.Serialize(values);
        }

        public Bytes Deserialize(Deserialization deserializer)
        {
            return new Bytes(deserializer.ToBytes());
        }

        public object GetValue()
        {
            return values;
        }

        public override bool Equals(object other)
        {
            if (other is not Bytes)
                throw new NotImplementedException();

            Bytes otherBytes = (Bytes)other;
            bool equal = Enumerable.SequenceEqual(this.values, otherBytes.values);

            return equal;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            foreach (byte value in values)
                result.Append(value.ToString());
            return result.ToString();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
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

        public static bool Deserialize(byte[] data)
        {
            bool ret = BitConverter.ToBoolean(data);
            return ret;
        }

        public static Bool Deserialize(Deserialization deserializer)
        {
            return new Bool(deserializer.DeserializeBool());
        }

        public TypeTag Variant()
        {
            return TypeTag.BOOL;
        }

        public override string ToString()
        {
            return value.ToString();
        }

        public override bool Equals(object other)
        {
            if (other is not Bool)
                throw new NotImplementedException();

            Bool otherBool = (Bool)other;

            return this.value == otherBool.value;
        }


        public override int GetHashCode() => this.value.GetHashCode();

        public object GetValue()
        {
            return value;
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

        public static int Deserialize(byte[] data)
        {
            return BitConverter.ToInt32(data);
        }

        public static U8 Deserialize(Deserialization deserializer)
        {
            throw new NotImplementedException();
        }

        public object GetValue()
        {
            return value;
        }

        public override string ToString()
        {
            return this.value.ToString();
        }

        public override bool Equals(object other)
        {
            if (other is not U8)
                throw new NotImplementedException();

            U8 otherU8 = (U8)other;

            return this.value == otherU8.value;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    /// <summary>
    /// Representation of a U32.
    /// </summary>
    public class U32 : ISerializableTag
    {
        public uint value;

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

        public static uint Deserialize(byte[] data)
        {
            return BitConverter.ToUInt32(data);
        }

        public static U32 Deserialize(Deserialization deserializer)
        {
            U32 val = new U32(deserializer.DeserializeU32());
            return val;
        }

        public override string ToString()
        {
            return value.ToString();
        }

        public override bool Equals(object other)
        {
            if (other is not U32)
                throw new NotImplementedException();

            U32 otherU8 = (U32)other;

            return this.value == otherU8.value;
        }

        public override int GetHashCode() => this.value.GetHashCode();

        public object GetValue()
        {
            return value;
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

        public static ulong Deserialize(byte[] data)
        {
            return BitConverter.ToUInt64(data);
        }

        public static U64 Deserialize(Deserialization deserializer)
        {
            throw new NotImplementedException();
        }

        public object GetValue()
        {
            return value;
        }

        public override bool Equals(object other)
        {
            if (other is not U64)
                throw new NotImplementedException();

            U64 otherU64 = (U64)other;

            return this.value == (ulong) otherU64.GetValue();
        }

        public override string ToString()
        {
            return this.value.ToString();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
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

        public static BigInteger Deserialize(byte[] data)
        {
            return new BigInteger(data);
        }

        public static U128 Deserialize(Deserialization deserializer)
        {
            throw new NotImplementedException();
        }

        public object GetValue()
        {
            return value;
        }

        public override bool Equals(object other)
        {
            if (other is not U128)
                throw new NotImplementedException();

            U128 otherU128 = (U128)other;

            return this.value == otherU128.value;
        }

        public override string ToString()
        {
            return this.value.ToString();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
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
                this.typeArgs[i].Serialize(serializer);
        }

        public static StructTag Deserialize(Deserialization deserializer)
        {
            AccountAddress address = AccountAddress.Deserialize(deserializer);
            string module = deserializer.DeserializeString();
            string name = deserializer.DeserializeString();

            int length = deserializer.DeserializeUleb128();
            List<ISerializableTag> typeArgsList = new List<ISerializableTag>();

            while (typeArgsList.Count < length)
            {
                ISerializableTag val = ISerializableTag.DeserializeTag(deserializer);
                typeArgsList.Add(val);
            }

            ISerializableTag[] typeArgsArr = typeArgsList.ToArray();

            StructTag structTag = new StructTag(
                address,
                module,
                name,
                typeArgsArr
            );

            return structTag;
        }

        public object GetValue()
        {
            throw new NotSupportedException();
        }

        public override bool Equals(object other)
        {
            if (other is not StructTag)
                throw new NotImplementedException();

            StructTag otherStructTag = (StructTag)other;

            return (
                this.address.Equals(otherStructTag.address)
                && this.module.Equals(otherStructTag.module)
                && this.name.Equals(otherStructTag.name)
                && Enumerable.SequenceEqual(this.typeArgs, otherStructTag.typeArgs)
            );;
        }

        public override string ToString()
        {
            string value = string.Format(
                "{0}::{1}::{2}",
                this.address.ToString(),
                this.module.ToString(),
                this.name.ToString()
            );

            if(this.typeArgs.Length > 0)
            {
                value += string.Format("<{0}", this.typeArgs[0].ToString());
                foreach(ISerializableTag typeArg in this.typeArgs[1..])
                    value += string.Format(", {0}", typeArg.ToString());
                value += ">";
            }
            return value;
        }

        public static StructTag FromStr(string typeTag)
        {
            string name = "";
            int index = 0;
            while (index < typeTag.Length)
            {
                char letter = typeTag[index];
                index += 1;

                if (letter.Equals("<"))
                    throw new NotImplementedException();
                else
                    name += letter;
            }

            string[] split = name.Split("::");
            return new StructTag(
                AccountAddress.FromHex(split[0]), 
                split[1], 
                split[2], 
                new ISerializableTag[] { }
            );
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}