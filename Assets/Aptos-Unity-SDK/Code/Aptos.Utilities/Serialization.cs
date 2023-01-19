// An implementation of BCS in C#
using System;
using System.Numerics;
using System.Runtime.Serialization;
// MemoryStream

namespace Aptos.Utilities.BCS
{
    public static class Serialization
    {
        #region Serialization

        /// <summary>
        /// Binary Canonical Serialization
        /// </summary>
        ///
        /// This takes in a boxed object, and if it can serialize, it will.  This likely will require more
        /// work around a more idiomatic implementation
        /// 
        /// <returns></returns>
        public static byte[] Serialize(string value)
        {
            return SerializeString(value);
        }

        public static byte[] Serialize(byte[] value)
        {
            return SerializeBytes(value);
        }

        public static byte[] Serialize(bool value)
        {
            return SerializeBool(value);
        }

        public static byte[] Serialize(byte num)
        {
            return SerializeU8(num);
        }

        public static byte[] Serialize(ushort num)
        {
            return SerializeU16(num);
        }

        public static byte[] Serialize(uint num)
        {
            return SerializeU32(num);
        }

        public static byte[] Serialize(ulong num)
        {
            return SerializeU64(num);
        }

        public static byte[] Serialize(BigInteger num)
        {
            return SerializeU128(num);
        }
        
        public static byte[] SerializeString(string value)
        {
            return SerializeBytes(System.Text.Encoding.UTF8.GetBytes(value));
        }   

        public static byte[] SerializeBytes(byte[] bytes)
        {
            byte[] output = new byte[bytes.Length + 8];
            // Copy the length to first 8 bytes
            Array.Copy(SerializeLength((ulong)bytes.Length), output, 8);
            // Copy the bytes to the rest of the array
            Array.Copy(bytes, 0, output, 8, bytes.Length);
            return output;
        }

        public static byte[] SerializeBool(bool value)
        {
            byte val = (byte)(value ? 1 : 0);
            return new[] { val };
        }

        public static byte[] SerializeU8(byte num)
        {
            return new[] { num };
        }

        public static byte[] SerializeU16(ushort num)
        {
            byte lower = (byte)(num & 0xFF);
            byte upper = (byte)(num >> 8 & 0xFF);
            return new[] { upper, lower };
        }

        public static byte[] SerializeU32(uint num)
        {
            byte byte1 = (byte)(num & 0xFF);
            byte byte2 = (byte)(num >> 8 & 0xFF);
            byte byte3 = (byte)(num >> 16 & 0xFF);
            byte byte4 = (byte)(num >> 24 & 0xFF);
            return new[] { byte4, byte3, byte2, byte1 };
        }

        public static byte[] SerializeU64(ulong num)
        {
            byte byte1 = (byte)(num & 0xFF);
            byte byte2 = (byte)(num >> 8 & 0xFF);
            byte byte3 = (byte)(num >> 16 & 0xFF);
            byte byte4 = (byte)(num >> 24 & 0xFF);
            byte byte5 = (byte)(num >> 32 & 0xFF);
            byte byte6 = (byte)(num >> 40 & 0xFF);
            byte byte7 = (byte)(num >> 48 & 0xFF);
            byte byte8 = (byte)(num >> 56 & 0xFF);
            return new[] { byte8, byte7, byte6, byte5, byte4, byte3, byte2, byte1 };
        }

        public static byte[] SerializeU128(BigInteger value)
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
            if (content.Length == 16)
            {
                return content;
            }

            byte[] output = new byte[16];
            Array.Copy(content, 0, output, 0, content.Length);
            return output;
        }

        public static byte[] SerializeLength(ulong length)
        {
            return SerializeU64(length);
        }

        public static byte[] SerializeVariantIndex(uint value)
        {
            return SerializeU32(value);
        }

        public static byte[] SerializeOptionTag(bool value)
        {
            return SerializeBool(value);
        }

        #endregion
    }
}