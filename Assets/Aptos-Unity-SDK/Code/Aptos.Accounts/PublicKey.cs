using Chaos.NaCl;
using NBitcoin.DataEncoders;
using System;

namespace Aptos.Accounts
{
    /// <summary>
    /// Represents a 32-byte public key
    /// </summary>
    public class PublicKey
    {
        /// <summary>
        /// Public key length.
        /// </summary>
        public const int KeyLength = 32;

        /// <summary>
        /// String representation of public key
        /// </summary>
        private string _keyBase58;

        /// <summary>
        /// Byte representation of public key
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
        /// The public key bytes
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
        /// Initializes the PublicKey object with a given byte array.
        /// </summary>
        /// <param name="publicKey">The public key as byte array.</param>
        public PublicKey(byte[] publicKey)
        {
            if (publicKey == null)
                throw new ArgumentNullException(nameof(publicKey));
            if (publicKey.Length != KeyLength)
                throw new ArgumentException("invalid key length: ", nameof(publicKey));
            KeyBytesFromBase58 = new byte[KeyLength];
            Array.Copy(publicKey, KeyBytesFromBase58, KeyLength);
        }

        /// <summary>
        /// Initializes the PublicKey object with a given ASCII representation of public key
        /// </summary>
        /// <param name="key"></param> The public key as a Base58 encoded string
        public PublicKey(string key)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
        }

        /// <summary>
        /// Initialize the PublicKey object from the given string.
        /// </summary>
        /// <param name="publicKey">The public key as base58 encoded byte array.</param>
        public PublicKey(ReadOnlySpan<byte> publicKey)
        {
            if (publicKey.Length != KeyLength)
                throw new ArgumentException("invalid key length", nameof(publicKey));
            KeyBytesFromBase58 = new byte[KeyLength];
            publicKey.CopyTo(KeyBytesFromBase58.AsSpan());
        }

        /// <summary>
        /// Verify a signed message using the current public key.
        /// </summary>
        /// <param name="message">Message that was signed.</param>
        /// <param name="signature">The signature from the message.</param>
        /// <returns></returns>
        public bool Verify(byte[] message, byte[] signature)
        {
            return Ed25519.Verify(signature, message, KeyBytesFromBase58);
        }

        /// <summary>
        /// Check if PubliKey is a valid on the Ed25519 curve.
        /// </summary>
        /// <returns>Returns true if public key is on the curve.</returns>
        public bool IsOnCurve()
        {
            return KeyBytesFromBase58.IsOnCurve();
        }
    }
}