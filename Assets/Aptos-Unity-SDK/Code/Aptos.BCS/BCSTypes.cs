using Aptos.Accounts;
using Aptos.HdWallet.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

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

    /// <summary>
    /// An interfaces that enforces types to implement a serialization method.
    /// </summary>
    public interface ISerializable
    {
        /// <summary>
        /// Serialize the object.
        /// </summary>
        /// <param name="serializer"></param>
        public void Serialize(Serialization serializer);

        /// <summary>
        /// Deserializes a byte array hosted inside the Deserializer.
        /// </summary>
        /// <param name="deserializer"></param>
        /// <returns></returns>
        public static ISerializable Deserialize(Deserialization deserializer) => throw new NotImplementedException();
    }

    /// <summary>
    /// An interface that encorces all type tags to implement a
    /// serializaiton method and also return it's internal values.
    /// A static deserializaton method is also provides.
    /// </summary>
    public interface ISerializableTag : ISerializable
    {
        /// <summary>
        /// Returns the type of type tag.
        /// </summary>
        /// <returns>A TypeTag enum.</returns>
        public TypeTag Variant();

        /// <summary>
        /// Gets the internal value.
        /// </summary>
        /// <returns></returns>
        public object GetValue();

        /// <summary>
        /// Serializes the type tag using it's own serializaton method.
        /// </summary>
        /// <param name="serializer"></param>
        public void SerializeTag(Serialization serializer) => this.Serialize(serializer);

        /// <summary>
        /// Deserializes a byte array hosted inside the Deserializer.
        /// </summary>
        /// <param name="deserializer"></param>
        /// <returns></returns>
        public static new ISerializableTag Deserialize(Deserialization deserializer) => throw new NotImplementedException();

        /// <summary>
        /// Deserialize a tag based on it's type.
        /// </summary>
        /// <param name="deserializer"></param>
        /// <returns>An object.</returns>
        public static ISerializableTag DeserializeTag(Deserialization deserializer)
        {
            TypeTag variant = (TypeTag)deserializer.DeserializeUleb128();

            if (variant == TypeTag.BOOL)
                return Bool.Deserialize(deserializer);
            else if (variant == TypeTag.U8)
                return U8.Deserialize(deserializer);
            else if (variant == TypeTag.U16)
                return U16.Deserialize(deserializer);
            else if (variant == TypeTag.U32)
                return U32.Deserialize(deserializer);
            else if (variant == TypeTag.U64)
                return U64.Deserialize(deserializer);
            else if (variant == TypeTag.U128)
                return U128.Deserialize(deserializer);
            else if (variant == TypeTag.U256)
                return U256.Deserialize(deserializer);
            else if (variant == TypeTag.ACCOUNT_ADDRESS)
                return AccountAddress.Deserialize(deserializer);
            else if (variant == TypeTag.SIGNER)
                throw new NotImplementedException();
            else if (variant == TypeTag.VECTOR)
                throw new NotImplementedException();
            else if (variant == TypeTag.STRUCT)
                return StructTag.Deserialize(deserializer);
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Representation of a tag sequence.
    /// </summary>
    public class TagSequence : ISerializable
    {
        /// <summary>
        /// A list of serializable tags.
        /// </summary>
        ISerializableTag[] serializableTags;

        /// <summary>
        /// Creates a TagSequence objects from a list of serializable tags.
        /// </summary>
        /// <param name="serializableTags">A list of serializable tags.</param>
        public TagSequence(ISerializableTag[] serializableTags)
            => this.serializableTags = serializableTags;

        /// <inheritdoc/>
        public void Serialize(Serialization serializer)
        {
            serializer.SerializeU32AsUleb128((uint)this.serializableTags.Length);
            foreach (ISerializableTag element in this.serializableTags)
                element.SerializeTag(serializer);
        }

        /// <inheritdoc/>
        public static TagSequence Deserialize(Deserialization deserializer)
        {
            int length = deserializer.DeserializeUleb128();

            List<ISerializableTag> values = new List<ISerializableTag>();

            while (values.Count < length)
            {
                ISerializableTag tag = ISerializableTag.DeserializeTag(deserializer);
                values.Add(tag);
            }
            return new TagSequence(values.ToArray());
        }

        /// <summary>
        /// Gets the internal list of objects inside the TagSequence.
        /// </summary>
        /// <returns>The list of objects.</returns>
        public object GetValue() => serializableTags;

        /// <inheritdoc/>
        public override bool Equals(object other)
        {
            TagSequence otherTagSeq = (TagSequence)other;
            return Enumerable.SequenceEqual(
                (ISerializableTag[])this.GetValue(),
                (ISerializableTag[])otherTagSeq.GetValue()
            );
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            foreach (var tag in serializableTags)
                result.Append(tag.ToString());
            return result.ToString();
        }

        /// <inheritdoc/>
        public override int GetHashCode() => base.GetHashCode();
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
        /// <summary>
        /// The internal list of objects that are to be serialized or deserialized.
        /// </summary>
        ISerializable[] values;

        /// <summary>
        /// The length of the Sequence.
        /// </summary>
        public int Length { get => values.Length; }

        /// <summary>
        /// Gets the internal list of objects inside the Sequence.
        /// </summary>
        /// <returns>The list of object.</returns>
        public object GetValue() => values;

        /// <summary>
        /// Creates a Sequence object from a list of serializable objects,
        /// e.g. U8, AccountAddress.
        /// </summary>
        /// <param name="serializable">A list of serializable objects.</param>
        public Sequence(ISerializable[] serializable) => this.values = serializable;

        /// <inheritdoc/>
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
                else
                {
                    Serialization s = new Serialization();
                    element.Serialize(s);
                    byte[] b = s.GetBytes();
                    serializer.SerializeBytes(b);
                }
            }
        }

        /// <inheritdoc/>
        public static Sequence Deserialize(Deserialization deser)
        {
            int length = deser.DeserializeUleb128();
            List<ISerializable> values = new List<ISerializable>();

            while (values.Count < length)
                values.Add(new Bytes(deser.ToBytes()));
            return new Sequence(values.ToArray());
        }

        /// <inheritdoc/>
        public override bool Equals(object other)
        {
            Sequence otherSeq = (Sequence)other;
            return Enumerable.SequenceEqual(this.values, otherSeq.values);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            foreach (var value in values)
                result.Append(value.ToString());
            return result.ToString();
        }

        /// <inheritdoc/>
        public override int GetHashCode() => base.GetHashCode();
    }

    /// <summary>
    /// Representation of a byte sequence.
    /// </summary>
    public class BytesSequence : ISerializable
    {
        /// <summary>
        /// A list of a list of bytes.
        /// </summary>
        byte[][] values;

        /// <summary>
        /// Creates a ByteSequence object from a list of a list of bytes.
        /// </summary>
        /// <param name="values">A lsit of a list of bytes.</param>
        public BytesSequence(byte[][] values) => this.values = values;

        /// <inheritdoc/>
        public void Serialize(Serialization serializer)
        {
            serializer.SerializeU32AsUleb128((uint)this.values.Length);
            foreach (byte[] element in this.values)
                serializer.SerializeBytes(element);
        }

        /// <inheritdoc/>
        public static BytesSequence Deserialize(Deserialization deserializer)
        {
            int length = deserializer.DeserializeUleb128();
            List<byte[]> bytesList = new List<byte[]>();

            while (bytesList.Count < length)
            {
                byte[] bytes = deserializer.ToBytes();
                bytesList.Add(bytes);
            }

            return new BytesSequence(bytesList.ToArray());
        }

        /// <inheritdoc/>
        public object GetValue() => values;

        public override bool Equals(object other)
        {
            BytesSequence otherSeq = (BytesSequence)other;

            bool equal = true;
            for (int i = 0; i < this.values.Length; i++)
                equal = equal && Enumerable.SequenceEqual(this.values[i], otherSeq.values[i]);
            return equal;
        }

        /// <inheritdoc/>
        public override int GetHashCode() => base.GetHashCode();

        /// <inheritdoc/>
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
        /// <summary>
        /// A dictionary mapping to values that are serializable.
        /// </summary>
        public Dictionary<BString, ISerializable> values;

        /// <summary>
        /// Creates a BCSMap from a Dictionary.
        /// </summary>
        /// <param name="values">A dictionary mapping to values that are
        /// serializable.</param>
        public BCSMap(Dictionary<BString, ISerializable> values)
            => this.values = values;

        /// <summary>
        /// Gets the internal dictionary of a BCSMap.
        /// </summary>
        /// <returns></returns>
        public object GetValue() => values;

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
            SortedDictionary<string, (byte[], byte[])> byteMap
                = new SortedDictionary<string, (byte[], byte[])>();

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

            foreach (KeyValuePair<string, (byte[], byte[])> entry in byteMap)
            {
                mapSerializer.SerializeFixedBytes(entry.Value.Item1);
                mapSerializer.SerializeFixedBytes(entry.Value.Item2);
            }

            serializer.SerializeFixedBytes(mapSerializer.GetBytes());
        }

        /// <inheritdoc/>
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
        /// <summary>
        /// The internal string value.
        /// </summary>
        public string value;

        /// <summary>
        /// Creates a BString from a string.
        /// </summary>
        /// <param name="value">A string value.</param>
        public BString(string value) => this.value = value;

        /// <summary>
        /// Serializes the BString object using the given Serializer.
        /// </summary>
        /// <param name="serializer">The Serializer object.</param>
        public void Serialize(Serialization serializer) => serializer.Serialize(value);

        /// <summary>
        /// Deserializes a give byte array into a UTF8 compliant string.
        /// </summary>
        /// <param name="data">A string represented as a byte array.</param>
        /// <returns></returns>
        public static string Deserialize(byte[] data) => Encoding.UTF8.GetString(data);

        /// <summary>
        /// Utility function used to RemoveBOM prefixes.
        /// </summary>
        /// <param name="data">A string represented as a byte array.</param>
        /// <returns>The cleaned byte array.</returns>
        public static byte[] RemoveBOM(byte[] data)
        {
            var bom = Encoding.UTF8.GetPreamble();
            if (data.Length > bom.Length)
                for (int i = 0; i < bom.Length; i++)
                    if (data[i] != bom[i])
                        return data;
            return data.Skip(3).ToArray();
        }

        /// <summary>
        /// Deserializes a byte array contained by the Deserializer.
        /// </summary>
        /// <param name="deserializer">The Deserializer that contains the bytes.</param>
        /// <returns>A BString object.</returns>
        public static BString Deserialize(Deserialization deserializer)
        {
            string deserStr = deserializer.DeserializeString();
            return new BString(deserStr);
        }

        /// <inheritdoc/>
        public override string ToString() => value;

        /// <inheritdoc/>
        public override bool Equals(object other)
        {
            BString otherBString;

            if (other is string)
                otherBString = new BString((string) other);
            else if (other is not BString)
                throw new NotImplementedException();
            else 
                otherBString = (BString)other;
            return this.value == otherBString.value;
        }

        /// <inheritdoc/>
        public override int GetHashCode() => this.value.GetHashCode();

        /// <inheritdoc/>
        public object GetValue() => this.value;
    }

    /// <summary>
    /// Representation of Bytes in BCS.
    /// </summary>
    public class Bytes : ISerializable
    {
        /// <summary>
        /// The internals byte array.
        /// </summary>
        byte[] values;

        /// <summary>
        /// Creates a Bytes object from a given byte array.
        /// </summary>
        /// <param name="values">A list of bytes to serialize.</param>
        public Bytes(byte[] values) => this.values = values;

        /// <inheritdoc/>
        public void Serialize(Serialization serializer)
            => serializer.Serialize(values);

        /// <inheritdoc/>
        public Bytes Deserialize(Deserialization deserializer)
            => new Bytes(deserializer.ToBytes());

        /// <summary>
        /// Gets the byte array containes within the Bytes object.
        /// </summary>
        /// <returns></returns>
        public byte[] GetValue() => values;

        /// <inheritdoc/>
        public override bool Equals(object other)
        {
            if (other is not Bytes)
                throw new NotImplementedException();

            Bytes otherBytes = (Bytes)other;
            return Enumerable.SequenceEqual(this.values, otherBytes.values);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            foreach (byte value in values)
                result.Append(value.ToString());
            return result.ToString();
        }

        /// <inheritdoc/>
        public override int GetHashCode() => base.GetHashCode();
    }

    /// <summary>
    /// Representation of a Boolean.
    /// </summary>
    public class Bool : ISerializableTag
    {
        /// <summary>
        /// The internal boolean value.
        /// </summary>
        bool value;

        /// <summary>
        /// Creates a Bool object from a given boolean.
        /// </summary>
        /// <param name="value">A bolean value to serialize.</param>
        public Bool(bool value) => this.value = value;

        /// <inheritdoc/>
        public void Serialize(Serialization serializer)
            => serializer.Serialize(value);

        /// <inheritdoc/>
        public static bool Deserialize(byte[] data)
            => BitConverter.ToBoolean(data);

        /// <inheritdoc/>
        public static Bool Deserialize(Deserialization deserializer)
            => new Bool(deserializer.DeserializeBool());

        /// <inheritdoc/>
        public TypeTag Variant() => TypeTag.BOOL;

        /// <inheritdoc/>
        public override string ToString() => value.ToString();

        /// <inheritdoc/>
        public override bool Equals(object other)
        {
            if (other is not Bool)
                throw new NotImplementedException();
            Bool otherBool = (Bool)other;
            return this.value == otherBool.value;
        }

        /// <inheritdoc/>
        public override int GetHashCode() => this.value.GetHashCode();

        /// <inheritdoc/>
        public object GetValue() => value;
    }

    /// <summary>
    /// Representation of U8.
    /// </summary>
    public class U8 : ISerializableTag
    {
        /// <summary>
        /// The internal U8 value as a byte.
        /// </summary>
        byte value;

        /// <summary>
        /// Creates a U8 object from a given byte.
        /// </summary>
        /// <param name="value">A byte value to serialize as u8.</param>
        public U8(byte value) => this.value = value;

        /// <inheritdoc/>
        public TypeTag Variant() => TypeTag.U8;

        /// <inheritdoc/>
        public void Serialize(Serialization serializer) => serializer.Serialize(value);

        /// <inheritdoc/>
        public static int Deserialize(byte[] data) => BitConverter.ToInt32(data);

        /// <inheritdoc/>
        public static U8 Deserialize(Deserialization deserializer)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public object GetValue() => value;

        /// <inheritdoc/>
        public override string ToString() => this.value.ToString();

        /// <inheritdoc/>
        public override bool Equals(object other)
        {
            if (other is not U8)
                throw new NotImplementedException();
            U8 otherU8 = (U8)other;
            return this.value == otherU8.value;
        }

        /// <inheritdoc/>
        public override int GetHashCode() => base.GetHashCode();
    }

    /// <summary>
    /// Representation of a U32.
    /// </summary>
    public class U16 : ISerializableTag
    {
        /// <summary>
        /// The internal U16 values as a uint data type.
        /// </summary>
        public uint value;

        /// <summary>
        /// Creates a U16 object from a given uint value.
        /// </summary>
        /// <param name="value">A uint value to serialize as u16.</param>
        public U16(uint value) => this.value = value;

        /// <inheritdoc/>
        public TypeTag Variant() => TypeTag.U16;

        /// <inheritdoc/>
        public void Serialize(Serialization serializer) => serializer.Serialize(value);

        /// <inheritdoc/>
        public static ushort Deserialize(byte[] data) => BitConverter.ToUInt16(data);

        /// <inheritdoc/>
        public static U16 Deserialize(Deserialization deserializer)
        {
            U16 val = new U16(deserializer.DeserializeU32());
            return val;
        }

        /// <inheritdoc/>
        public override string ToString() => value.ToString();

        /// <inheritdoc/>
        public override bool Equals(object other)
        {
            if (other is not U16)
                throw new NotImplementedException();
            U16 otherU16 = (U16)other;
            return this.value == otherU16.value;
        }

        /// <inheritdoc/>
        public override int GetHashCode() => this.value.GetHashCode();

        /// <inheritdoc/>
        public object GetValue() => value;
    }

    /// <summary>
    /// Representation of a U32.
    /// </summary>
    public class U32 : ISerializableTag
    {
        /// <summary>
        /// The internal U32 values as a uint data type.
        /// </summary>
        public uint value;

        /// <summary>
        /// Creates a U32 object from a uint value.
        /// </summary>
        /// <param name="value">A uint value to serialize as u32.</param>
        public U32(uint value) => this.value = value;

        /// <inheritdoc/>
        public TypeTag Variant() => TypeTag.U32;

        /// <inheritdoc/>
        public void Serialize(Serialization serializer) => serializer.Serialize(value);

        /// <inheritdoc/>
        public static uint Deserialize(byte[] data) => BitConverter.ToUInt32(data);

        /// <inheritdoc/>
        public static U32 Deserialize(Deserialization deserializer)
        {
            U32 val = new U32(deserializer.DeserializeU32());
            return val;
        }

        /// <inheritdoc/>
        public override string ToString() => value.ToString();

        /// <inheritdoc/>
        public override bool Equals(object other)
        {
            if (other is not U32)
                throw new NotImplementedException();
            U32 otherU8 = (U32)other;
            return this.value == otherU8.value;
        }

        /// <inheritdoc/>
        public override int GetHashCode() => this.value.GetHashCode();

        /// <inheritdoc/>
        public object GetValue() => value;
    }

    /// <summary>
    /// Representation of U64.
    /// </summary>
    public class U64 : ISerializableTag
    {
        /// <summary>
        /// The internal U64 value as a ulong data type.
        /// </summary>
        ulong value;

        /// <summary>
        /// Creates a U64 object from a given ulong value.
        /// </summary>
        /// <param name="value">A ulong value to serialize as u64.</param>
        public U64(ulong value) => this.value = value;

        /// <inheritdoc/>
        public TypeTag Variant() => TypeTag.U64;

        /// <inheritdoc/>
        public void Serialize(Serialization serializer) => serializer.Serialize(value);

        /// <inheritdoc/>
        public static ulong Deserialize(byte[] data) => BitConverter.ToUInt64(data);

        /// <inheritdoc/>
        public static U64 Deserialize(Deserialization deserializer)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public object GetValue() => value;

        /// <inheritdoc/>
        public override bool Equals(object other)
        {
            if (other is not U64)
                throw new NotImplementedException();
            U64 otherU64 = (U64)other;
            return this.value == (ulong)otherU64.GetValue();
        }

        /// <inheritdoc/>
        public override string ToString() => this.value.ToString();

        /// <inheritdoc/>
        public override int GetHashCode() => base.GetHashCode();
    }

    /// <summary>
    /// Representation of a U128.
    /// </summary>
    public class U128 : ISerializableTag
    {
        /// <summary>
        /// The internal U128 value as a BigInteger data type.
        /// </summary>
        BigInteger value;

        /// <summary>
        /// Creates a U128 objeect from a BigInteger value.
        /// </summary>
        /// <param name="value">A BigInteger value to serialize as u128.</param>
        public U128(BigInteger value) => this.value = value;

        /// <inheritdoc/>
        public TypeTag Variant() => TypeTag.U128;

        /// <inheritdoc/>
        public void Serialize(Serialization serializer)
            => serializer.Serialize(value);

        /// <inheritdoc/>
        public static BigInteger Deserialize(byte[] data)
            => new BigInteger(data);

        /// <inheritdoc/>
        public static U128 Deserialize(Deserialization deserializer)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public object GetValue() => value;

        /// <inheritdoc/>
        public override bool Equals(object other)
        {
            if (other is not U128)
                throw new NotImplementedException();
            U128 otherU128 = (U128)other;
            return this.value == otherU128.value;
        }

        /// <inheritdoc/>
        public override string ToString() => this.value.ToString();

        /// <inheritdoc/>
        public override int GetHashCode() => base.GetHashCode();
    }

    /// <summary>
    /// Representation of a 256.
    /// </summary>
    public class U256 : ISerializableTag
    {
        /// <summary>
        /// The internal U256 value as a BigInteger data type.
        /// </summary>
        BigInteger value;

        /// <summary>
        /// Creates a U256 object from a given BigInteger value.
        /// </summary>
        /// <param name="value">A BigInteger value to serialize as u256.</param>
        public U256(BigInteger value) => this.value = value;

        /// <inheritdoc/>
        public TypeTag Variant() => TypeTag.U256;

        /// <inheritdoc/>
        public void Serialize(Serialization serializer)
            => serializer.Serialize(value);

        /// <inheritdoc/>
        public static BigInteger Deserialize(byte[] data)
            => new BigInteger(data);

        /// <inheritdoc/>
        public static U256 Deserialize(Deserialization deserializer)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public object GetValue() => value;

        /// <inheritdoc/>
        public override bool Equals(object other)
        {
            if (other is not U256)
                throw new NotImplementedException();
            U256 otherU256 = (U256)other;
            return this.value == otherU256.value;
        }

        /// <inheritdoc/>
        public override string ToString() => this.value.ToString();

        /// <inheritdoc/>
        public override int GetHashCode() => base.GetHashCode();
    }

    /// <summary>
    /// Representation of a struct tag.
    /// </summary>
    public class StructTag : ISerializableTag
    {
        /// <summary>
        /// The account address of the struct tag.
        /// </summary>
        AccountAddress address;

        /// <summary>
        /// The module name of the struct tag.
        /// </summary>
        string module;

        /// <summary>
        /// The function name of the struct tag.
        /// </summary>
        string name;

        /// <summary>
        /// A set of type arguments, if any.
        /// </summary>
        ISerializableTag[] typeArgs;

        /// <summary>
        /// Creates a StructTag object from an address, module, function name,
        /// and type arguments.
        /// </summary>
        /// <param name="address">An AccountAddress.</param>
        /// <param name="module">The module name.</param>
        /// <param name="name">The function name.</param>
        /// <param name="typeArgs">A list of type arguments.</param>
        public StructTag(AccountAddress address, string module, string name, ISerializableTag[] typeArgs)
        {
            this.address = address;
            this.module = module;
            this.name = name;
            this.typeArgs = typeArgs;
        }

        /// <inheritdoc/>
        public TypeTag Variant() => TypeTag.STRUCT;

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public object GetValue()
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
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
            ); ;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            string value = string.Format(
                "{0}::{1}::{2}",
                this.address.ToString(),
                this.module.ToString(),
                this.name.ToString()
            );

            if (this.typeArgs.Length > 0)
            {
                value += string.Format("<{0}", this.typeArgs[0].ToString());
                foreach (ISerializableTag typeArg in this.typeArgs[1..])
                    value += string.Format(", {0}", typeArg.ToString());
                value += ">";
            }
            return value;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public override int GetHashCode() => base.GetHashCode();
    }
}