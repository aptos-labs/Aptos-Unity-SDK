// An implementation of BCS in C#
using System;
using System.IO;
using System.Numerics;
using System.Runtime.Serialization;

namespace Aptos.Utilities.BCS
{
    public class Serialization
    {
        #region Serialization

        protected MemoryStream output;

        public Serialization()
        {
            output = new MemoryStream();
        }

        public byte[] GetBytes()
        {
            return output.ToArray();
        }

        public Serialization Serialize(string value)
        {
            SerializeString(value);
            return this;
        }

        public Serialization Serialize(byte[] value)
        {
            SerializeBytes(value);
            return this;
        }

        public Serialization Serialize(bool value)
        {
            SerializeBool(value);
            return this;
        }

        public Serialization Serialize(byte num)
        {
            SerializeU8(num);
            return this;
        }

        public Serialization Serialize(ushort num)
        {
            SerializeU16(num);
            return this;
        }

        public Serialization Serialize(uint num)
        {
            SerializeU32(num);
            return this;
        }

        public Serialization Serialize(ulong num)
        {
            SerializeU64(num);
            return this;
        }

        public Serialization Serialize(BigInteger num)
        {
            SerializeU128(num);
            return this;
        }

        public Serialization Serialize(Sequence args)
        {
            SerializeU32AsUleb128((uint)args.Length);
            foreach (ISerializable element in args.GetValues())
            {
                Serialization s = new Serialization();
                element.Serialize(s);
                byte[] b = s.GetBytes();
                SerializeSingleSequenceBytes(b);
            }
            return this;
        }

        /**
        * Serializes a string. UTF8 string is supported. Serializes the string's bytes length "l" first,
        * and then serializes "l" bytes of the string content.
        *
        * BCS layout for "string": string_length | string_content. string_length is the bytes length of
        * the string that is uleb128 encoded. string_length is a u32 integer.
        **/
        public Serialization SerializeString(string value)
        {
            SerializeBytes(System.Text.Encoding.UTF8.GetBytes(value));
            return this;
        }

        /**
         * Serializes an array of bytes.
         *
         * BCS layout for "bytes": bytes_length | bytes. bytes_length is the length of the bytes array that is
         * uleb128 encoded. bytes_length is a u32 integer.
        **/
        public Serialization SerializeBytes(byte[] bytes)
        {
            // Write the length of the bytes array
            SerializeU32AsUleb128((uint)bytes.Length);
            // Copy the bytes to the rest of the array
            output.Write(bytes);
            return this;
        }

        /// <summary>
        /// Serializes a list of values in a sequence.
        /// Note that for sequences we first add the length for the entire sequence array,
        ///     not the length of the byte array
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public Serialization SerializeSingleSequenceBytes(byte[] bytes)
        {
            // Copy the bytes to the rest of the array
            output.Write(bytes);
            return this;
        }

        static public string ToReadableByteArray(byte[] bytes)
        {
            return string.Join(", ", bytes);
        }

        public Serialization WriteBytes(byte[] bytes)
        {
            output.Write(bytes);
            return this;
        }

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

        public Serialization SerializeU8(byte num)
        {
            output.WriteByte(num);
            return this;
        }

        public Serialization SerializeU16(ushort num)
        {
            byte lower = (byte)(num & 0xFF);
            byte upper = (byte)(num >> 8 & 0xFF);
            output.Write(new[] { upper, lower });
            return this;
        }

        public Serialization SerializeU32(uint num)
        {
            byte byte1 = (byte)(num & 0xFF);
            byte byte2 = (byte)(num >> 8 & 0xFF);
            byte byte3 = (byte)(num >> 16 & 0xFF);
            byte byte4 = (byte)(num >> 24 & 0xFF);
            output.Write(new[] { byte1, byte2, byte3, byte4 });
            return this;
        }

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