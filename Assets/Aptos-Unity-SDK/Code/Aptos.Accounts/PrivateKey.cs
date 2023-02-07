using Aptos.HdWallet.Utils;
using Chaos.NaCl;
using NBitcoin.DataEncoders;
using System;

namespace Aptos.Accounts
{
    /// <summary>
    /// Represents a 64-byte extended private key.
    /// An extended private key is a requirement from Choas.NaCl.
    /// 
    /// Note that the hexadecimal string representation is of the 32-byte private key on it's own.
    /// </summary>
    public class PrivateKey
    {
        /// <summary>
        /// Private key length.
        /// </summary>
        public const int KeyLength = 64;

        /// <summary>
        /// Hex string representation of pruvate key
        /// </summary>
        private string _key;

        /// <summary>
        /// Byte representation of private key
        /// </summary>
        private byte[] _keyBytes;

        /// <summary>
        /// String representation of private key
        /// </summary>
        private string _keyBase58;

        /// <summary>
        /// Byte representation of private key
        /// </summary>
        private byte[] _keyBytesBase58;

        /// <summary>
        /// The key as a 32-byte hexadecimal string (64 characters)
        /// NOTE: We maintain the full 64-byte (128 characters) representation of the extended private key
        /// , then we slice it in half since the other half contains the public key.
        /// </summary>
        public string Key
        {
            get
            {
                if (_key == null && _keyBytes != null)
                {
                    //string addressHex = CryptoBytes.ToHexStringLower(_keyBytes.Slice(0, 32));
                    string addressHex = CryptoBytes.ToHexStringLower(_keyBytes);
                    _key = "0x" + addressHex;
                }

                return _key[0..66]; // account for "0x"
            }

            set
            {
                _key = value;
            }
        }

        /// <summary>
        /// The key in bytes.
        /// Checks if we have the hexadecimal string representation of a 64-byte extended
        /// , then return the bytes accordingly.
        /// </summary>
        public byte[] KeyBytes
        {
            get
            {
                if (_keyBytes == null && _key != null)
                {
                    string key = _key;
                    if (_key[0..2].Equals("0x")) { key = _key[2..]; }
                    _keyBytes = key.HexStringToByteArray();
                }
                return _keyBytes;
            }

            set
            {
                _keyBytes = value;
            }
        }

        /// <summary>
        /// The key as base-58 encoded string
        /// Base58 encoding scheme is used to facilitate switching 
        /// from byte to alphanumeric text format (ASCII)
        /// </summary>
        public string KeyBase58
        {
            get
            {
                if (_keyBase58 == null && _keyBytes != null)
                {
                    _keyBase58 = Encoders.Base58.EncodeData(_keyBytes);
                }
                return _keyBase58;
            }

            set
            {
                _keyBase58 = value;
            }
        }

        /// <summary>
        /// The private key bytes
        /// </summary>
        public byte[] KeyBytesFromBase58
        {
            get
            {
                if (_keyBytes == null && _keyBase58 != null)
                {
                    _keyBytes = Encoders.Base58.DecodeData(_keyBase58);
                }
                return _keyBytes;
            }
            set
            {
                _keyBytes = value;
            }
        }

        /// <summary>
        /// Initializes the PrivateKey object with a given byte array.
        /// </summary>
        /// <param name="privateKey"></param> Byte array representation of the private key
        public PrivateKey(byte[] privateKey)
        {
            if (privateKey == null)
                throw new ArgumentNullException(nameof(privateKey));
            if (privateKey.Length != KeyLength)
                throw new ArgumentException("Invalid key length: ", nameof(privateKey));
            KeyBytes = new byte[KeyLength];
            Array.Copy(privateKey, KeyBytes, KeyLength);
        }

        /// <summary>
        /// Initializes the PrivateKey object with a given ASCII representation of private key
        /// </summary>
        /// <param name="key"></param> The private key as a Base58 encoded string
        public PrivateKey(string key)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
        }

        /// <summary>
        /// Initialize the PrivateKey object from the given string.
        /// </summary>
        /// <param name="key">The private key as base58 encoded byte array.</param>
        public PrivateKey(ReadOnlySpan<byte> privateKey)
        {
            if (privateKey.Length != KeyLength)
                throw new ArgumentException("Invalid key length: ", nameof(privateKey));
            KeyBytes = new byte[KeyLength];
            privateKey.CopyTo(KeyBytes.AsSpan());
        }

        public static bool operator ==(PrivateKey lhs, PrivateKey rhs)
        {

            if (lhs is null)
            {
                if (rhs is null)
                {
                    return true;
                }

                // Only the left side is null.
                return false;
            }
            // Equals handles case of null on right side.
            return lhs.Equals(rhs);
        }

        public static bool operator !=(PrivateKey lhs, PrivateKey rhs) => !(lhs == rhs);


        /// <summary>
        /// Sign a message using the current private key
        /// </summary>
        /// <param name="message">The message to sign, represented in bytes.</param>
        /// <returns>The signature generated for the message.</returns>
        public byte[] Sign(byte[] message)
        {
            ArraySegment<byte> signature = new ArraySegment<byte>(new byte[64]);
            Ed25519.Sign(signature,
                new ArraySegment<byte>(message),
                new ArraySegment<byte>(KeyBytes));
            return signature.Array;
        }

        /// <inheritdoc cref="Equals(object)"/>
        public override bool Equals(object obj)
        {
            if(obj is PrivateKey privateKey)
            {
                return privateKey.Key == this.Key;
            }

            return false;
        }

        /// <inheritdoc cref="GetHashCode"/>
        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }

        /// <inheritdoc cref="ToString"/>
        public override string ToString()
        {
            return Key;
        }

        /// <summary>
        /// Convert a PrivateKey object to hexadecimal string representation of private key.
        /// </summary>
        /// <param name="privateKey">The PrivateKey object.</param>
        /// <returns>Hexadecimal string representing the private key.</returns>
        public static implicit operator string(PrivateKey privateKey)
        {
            return privateKey.Key;
        }
    }
}