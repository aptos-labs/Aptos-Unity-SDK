using Chaos.NaCl;
using NBitcoin.DataEncoders;
using System;

namespace Aptos.Accounts
{
    /// <summary>
    /// Represents a 64-byte extended private key
    /// </summary>
    public class PrivateKey
    {
        /// <summary>
        /// Private key length.
        /// </summary>
        public const int KeyLength = 64;

        /// <summary>
        /// String representation of private key
        /// </summary>
        private string _keyBase58;

        /// <summary>
        /// Byte representation of private key
        /// </summary>
        private byte[] _keyBytes;

        /// <summary>
        /// The key as base-58 encoded string
        /// Base58 encoding scheme is used to facilitate switching 
        /// from byte to alphanumeric text format (ASCII)
        /// </summary>
        public string Key
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
            KeyBytesFromBase58 = new byte[KeyLength];
            Array.Copy(privateKey, KeyBytesFromBase58, KeyLength);
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
        public PrivateKey(ReadOnlySpan<byte> key)
        {
            if (key.Length != KeyLength)
                throw new ArgumentException("Invalid key length: ", nameof(key));
            KeyBytesFromBase58 = new byte[KeyLength];
            key.CopyTo(KeyBytesFromBase58.AsSpan());
        }

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
                new ArraySegment<byte>(KeyBytesFromBase58));
            return signature.Array;
        }
    }
}