// An implementation of BCS in C#
using System;
using System.IO;
using System.Numerics;
using System.Runtime.Serialization;

namespace Aptos.Utilities.BCS
{
    /// <summary>
    /// An implementation of BCS in C#
    /// </summary>
    public class Serialization
    {
        #region Serialization

        protected MemoryStream output;

        public Serialization()
        {
            output = new MemoryStream();
        }

        /// <summary>
        /// Return the serialization buffer as a byte array.
        /// </summary>
        /// <returns>Serialization buffer as a byte array.</returns>
        public byte[] GetBytes()
        {
            return output.ToArray();
        }

        /// <summary>
        /// Serialize a string value.
        /// </summary>
        /// <param name="value">String value to serialize.</param>
        /// <returns>The current Serialization object.</returns>
        public Serialization Serialize(string value)
        {
            SerializeString(value);
            return this;
        }

        /// <summary>
        /// Serialize a byte array.
        /// </summary>
        /// <param name="value">Byte array to serialize.</param>
        /// <returns>The current Serialization object.</returns>
        public Serialization Serialize(byte[] value)
        {
            SerializeBytes(value);
            return this;
        }

        /// <summary>
        /// Serialize a boolean value.
        /// </summary>
        /// <param name="value">Boolean value to serialize.</param>
        /// <returns>The current Serialization object.</returns>
        public Serialization Serialize(bool value)
        {
            SerializeBool(value);
            return this;
        }

        /// <summary>
        /// Serialize a single byte
        /// </summary>
        /// <param name="num">Byte to serialize.</param>
        /// <returns>The current Serialization object.</returns>
        public Serialization Serialize(byte num)
        {
            SerializeU8(num);
            return this;
        }

        /// <summary>
        /// Serialize an unsigned short value.
        /// </summary>
        /// <param name="num">The number to serialize.</param>
        /// <returns>The current Serialization object.</returns>
        public Serialization Serialize(ushort num)
        {
            SerializeU16(num);
            return this;
        }

        /// <summary>
        /// Serialize an unsigned integer value.
        /// </summary>
        /// <param name="num">The unsigned integer to serialize.</param>
        /// <returns>The current Serialization object.</returns>
        public Serialization Serialize(uint num)
        {
            SerializeU32(num);
            return this;
        }

        /// <summary>
        /// Serialize an unsigned long number.
        /// </summary>
        /// <param name="num">The unsigned long number to serialize.</param>
        /// <returns>The current Serialization object.</returns>
        public Serialization Serialize(ulong num)
        {
            SerializeU64(num);
            return this;
        }

        /// <summary>
        /// Serialize a big integer number.
        /// </summary>
        /// <param name="num">The big integer number to serialize.</param>
        /// <returns>The current Serialization object.</returns>
        public Serialization Serialize(BigInteger num)
        {
            SerializeU128(num);
            return this;
        }

        /// <summary>
        /// Serialize a sequence.
        /// </summary>
        /// <param name="args">The sequence to serialize.</param>
        /// <returns>The current Serialization object.</returns>
        public Serialization Serialize(Sequence args)
        {
            SerializeU32AsUleb128((uint)args.Length);
            foreach (ISerializable element in args.GetValues())
            {
                Serialization s = new Serialization();
                element.Serialize(s);
                byte[] b = s.GetBytes();
                SerializeFixedBytes(b);
            }
            return this;
        }

        /// <summary>
        /// Serializes a string. UTF8 string is supported. Serializes the string's bytes length "l" first,
        /// and then serializes "l" bytes of the string content.
        /// 
        /// BCS layout for "string": string_length | string_content. string_length is the bytes length of
        /// the string that is uleb128 encoded. string_length is a u32 integer.
        /// </summary>
        /// <param name="value">String value to serialize.</param>
        /// <returns>The current Serialization object.</returns>
        public Serialization SerializeString(string value)
        {
            SerializeBytes(System.Text.Encoding.UTF8.GetBytes(value));
            return this;
        }

        /// <summary>
        /// Serializes an array of bytes.
        /// BCS layout for "bytes": bytes_length | bytes. bytes_length is the length of the bytes array that is
        /// uleb128 encoded. bytes_length is a u32 integer.
        /// </summary>
        /// <param name="bytes">Byte array to serialize.</param>
        /// <returns>The current Serialization object.</returns>
        public Serialization SerializeBytes(byte[] bytes)
        {
            // Write the length of the bytes array
            SerializeU32AsUleb128((uint)bytes.Length);
            // Copy the bytes to the rest of the array
            output.Write(bytes);
            return this;
        }

        /// <summary>
        /// Serializes a list of values represented in a byte array. 
        /// This can be a sequence or a value represented as a byte array.
        /// Note that for sequences we first add the length for the entire sequence array,
        /// not the length of the byte array.
        /// </summary>
        /// <param name="bytes">Byte array to be serialized.</param>
        /// <returns>The current Serialization object.</returns>
        public Serialization SerializeFixedBytes(byte[] bytes)
        {
            // Copy the bytes to the rest of the array
            output.Write(bytes);
            return this;
        }

        /// <summary>
        /// Utility method to print a byte array.
        /// </summary>
        /// <param name="bytes">Byte array to turn into string.</param>
        /// <returns>String representation of a byte array.</returns>
        static public string ToReadableByteArray(byte[] bytes)
        {
            return string.Join(", ", bytes);
        }

        /// <summary>
        /// Write an array bytes directly to the serialization buffer.
        /// </summary>
        /// <param name="bytes">Byte array to write to the serialization buffer.</param>
        /// <returns>The current Serialization object.</returns>
        public Serialization WriteBytes(byte[] bytes)
        {
            output.Write(bytes);
            return this;
        }

        /// <summary>
        /// Serialize an unsigned integer value. 
        /// Usually used to serialize the length of values.
        /// </summary>
        /// <param name="value">Unsigned integer to serialize.</param>
        /// <returns>The current Serialization object.</returns>
        public Serialization SerializeU32AsUleb128(uint value)
        {
            while (value >= 0x80)
            {
                // Write 7 (lowest) bits of data and set the 8th bit to 1.
                byte b = (byte)(value & 0x7f);
                output.WriteByte((byte)(b | 0x80));
                value >>= 7;
            }

            // Write the remaining bits of data and set the highest bit to 0
            output.WriteByte((byte)(value & 0x7f));
            return this;
        }

        /// <summary>
        /// Serialize a boolean value
        /// </summary>
        /// <param name="value">Boolean value to serialize.</param>
        /// <returns>The current Serialization object.</returns>
        public Serialization SerializeBool(bool value)
        {
            if (value)
            {
                output.WriteByte(0x01);
            }
            else
            {
                output.WriteByte(0x00);
            }

            return this;
        }

        /// <summary>
        /// Serialize a unsigned byte number.
        /// </summary>
        /// <param name="num">Byte to serialize.</param>
        /// <returns>The current Serialization object.</returns>
        public Serialization SerializeU8(byte num)
        {
            output.WriteByte(num);
            return this;
        }

        /// <summary>
        /// Serialize unsigned short (U16).
        /// </summary>
        /// <param name="num">Unsigned short number to serialize.</param>
        /// <returns>The current Serialization object.</returns>
        public Serialization SerializeU16(ushort num)
        {
            byte lower = (byte)(num & 0xFF);
            byte upper = (byte)(num >> 8 & 0xFF);
            output.Write(new[] { upper, lower });
            return this;
        }

        /// <summary>
        /// Serialize an unsigned integer number.
        /// </summary>
        /// <param name="num">Unsigned integer number.</param>
        /// <returns>The current Serialization object.</returns>
        public Serialization SerializeU32(uint num)
        {
            byte byte1 = (byte)(num & 0xFF);
            byte byte2 = (byte)(num >> 8 & 0xFF);
            byte byte3 = (byte)(num >> 16 & 0xFF);
            byte byte4 = (byte)(num >> 24 & 0xFF);
            output.Write(new[] { byte1, byte2, byte3, byte4 });
            return this;
        }

        /// <summary>
        /// Serialize an unsigned long number.
        /// </summary>
        /// <param name="num">Unsigned long number to serialize.</param>
        /// <returns>The current Serialization object.</returns>
        public Serialization SerializeU64(ulong num)
        {
            byte byte1 = (byte)(num & 0xFF);
            byte byte2 = (byte)(num >> 8 & 0xFF);
            byte byte3 = (byte)(num >> 16 & 0xFF);
            byte byte4 = (byte)(num >> 24 & 0xFF);
            byte byte5 = (byte)(num >> 32 & 0xFF);
            byte byte6 = (byte)(num >> 40 & 0xFF);
            byte byte7 = (byte)(num >> 48 & 0xFF);
            byte byte8 = (byte)(num >> 56 & 0xFF);
            output.Write(new[] { byte1, byte2, byte3, byte4, byte5, byte6, byte7, byte8 });
            return this;
        }

        /// <summary>
        /// Serialize a big integer value.
        /// </summary>
        /// <param name="value">Big integer value to serialize.</param>
        /// <returns>The current Serialization object.</returns>
        public Serialization SerializeU128(BigInteger value)
        {
            // Ensure the BigInteger is unsigned
            if (value.Sign == -1)
            {
                throw new SerializationException("Invalid value for an unsigned int128");
            }

            // This is already little-endian
            byte[] content = value.ToByteArray(isUnsigned: true, isBigEndian: false);

            // BigInteger.toByteArray() may add a most-significant zero
            // byte for signing purpose: ignore it.
            if (!(content.Length <= 16 || content[0] == 0))
            {
                throw new SerializationException("Invalid value for an unsigned int128");
            }

            // Ensure we're padded to 16
            output.Write(content);

            if (content.Length != 16)
            {
                output.Write(new byte[16 - content.Length]);
            }

            return this;
        }

        #endregion
    }
}