using Aptos.HdWallet.Utils;
using Aptos.BCS;
using Chaos.NaCl;
using System;
using NBitcoin;

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
        /// Extended private key length.
        /// </summary>
        public const int ExtendedKeyLength = 64;

        /// <summary>
        /// Private key length.
        /// </summary>
        public const int KeyLength = 32;

        /// <summary>
        /// Hex string representation of private key.
        /// </summary>
        private string _key;

        /// <summary>
        /// The 64-byte extended private key.
        /// This key is used internally for signing.
        /// A public accessor that returns a 32-byte private is found in <see cref="_keyBytes">KeyBytes</see>
        /// </summary>
        private byte[] _extendedKeyBytes;

        /// <summary>
        /// The 32-byte private key
        /// This key is exposed publicly by <see cref="KeyBytes">KeyBytes</see>
        /// </summary>
        private byte[] _keyBytes;

        /// <summary>
        /// The key as a 32-byte hexadecimal string (64 characters).   
        /// </summary>
        public string Key
        {
            get
            {
                if (_key == null && _keyBytes != null)
                {
                    string addressHex = CryptoBytes.ToHexStringLower(_keyBytes);
                    _key = "0x" + addressHex;
                }

                return _key;
            }

            set
            {
                _key = value;
            }
        }

        /// <summary>
        /// The 32-byte private key in bytes.
        /// Checks if we have the 32-byte private key or 64-byte extended key, 
        /// otherwise uses the string representation to create both.
        /// </summary>
        public byte[] KeyBytes
        {
            get
            {
                // if the private key bytes have not being initialized, but a 32-byte (64 character) string private has been set
                if (_keyBytes == null && _extendedKeyBytes == null && _key != null)
                {
                    string key = _key;
                    if (_key[0..2].Equals("0x")) { key = _key[2..]; } // Trim the private key hex string

                    byte[] seed = key.ByteArrayFromHexString(); // Turn private key hex string into byte to be used a seed to derive the extended key
                    _keyBytes = seed;
                    _extendedKeyBytes = Ed25519.ExpandedPrivateKeyFromSeed(seed);
                }
                return _keyBytes;
            }

            set
            {
                if(value.Length != KeyLength)
                    throw new ArgumentException("Invalid key length: ", nameof(value));

                _keyBytes = value;
                _extendedKeyBytes = Ed25519.ExpandedPrivateKeyFromSeed(value);
            }
        }

        /// <summary>
        /// Initializes the PrivateKey object with a 64 byte array that represents the expanded private key from seed.   
        /// For example, using: <c>Ed25519.ExpandedPrivateKeyFromSeed(seed)</c>.   
        /// This constructor is expected to be called from the <see cref="Account.Account()">Account</see> constructor.   
        /// Note: To create a private key from a 32-byte string see <see cref="PrivateKey(string key)">PrivateKey(string key)</see>
        /// </summary>
        /// <param name="privateKey">64-byte array representation of the private key.</param>
        public PrivateKey(byte[] privateKey)
        {
            if (privateKey == null)
                throw new ArgumentNullException(nameof(privateKey));
            if (privateKey.Length != KeyLength)
                throw new ArgumentException("Invalid key length: ", nameof(privateKey));
            KeyBytes = new byte[KeyLength];
            Array.Copy(privateKey, KeyBytes, KeyLength);

            _extendedKeyBytes = new byte[Ed25519.ExpandedPrivateKeySizeInBytes];
            Array.Copy(Ed25519.ExpandedPrivateKeyFromSeed(KeyBytes), _extendedKeyBytes, Ed25519.ExpandedPrivateKeySizeInBytes);
        }

        /// <summary>
        /// Initializes the PrivateKey object with a 64 character (32-byte) ASCII representation of a private key.   
        /// Note: The undelying cryptographic library (Chaos.NaCL) uses an extended private key (64 byte) for fast computation.   
        /// This hex string is used as a seed to create an extended private key when <see cref="KeyBytes">KeyBytes</see> is requested.
        /// </summary>
        /// <param name="key">The private key as an ASCII encoded string.   
        /// Example: <c>0x64f57603b58af16907c18a866123286e1cbce89790613558dc1775abb3fc5c8c</c></param>
        public PrivateKey(string key)
        {
            if(!Utils.IsValidAddress(key))
                throw new ArgumentException("Invalid key", nameof(key));

            Key = key ?? throw new ArgumentNullException(nameof(key));
        }

        /// <summary>
        /// Create a private key from a string literal.
        /// Conforms to the standard Python and Typescript Aptos SDK.
        /// </summary>
        /// <param name="key">The private key as an ASCII encoded string.</param>
        /// <returns>Private key object.</returns>
        public static PrivateKey FromHex(string key)
        {
            return new PrivateKey(key);
        }

        /// <summary>
        /// Initialize the PrivateKey object from the given string.
        /// </summary>
        /// <param name="key">The private key as a hex encoded byte array.</param>
        public PrivateKey(ReadOnlySpan<byte> privateKey)
        {
            if (privateKey.Length != KeyLength)
                throw new ArgumentException("Invalid key length: ", nameof(privateKey));
            KeyBytes = new byte[KeyLength];
            privateKey.CopyTo(KeyBytes.AsSpan());

            _extendedKeyBytes = Ed25519.ExpandedPrivateKeyFromSeed(KeyBytes);
        }

        /// <summary>
        /// Derives public key from the private key bytes.
        /// </summary>
        /// <returns>PublicKey object.</returns>
        public PublicKey PublicKey()
        {
            PublicKey publicKey = new PublicKey(Ed25519.PublicKeyFromSeed(KeyBytes));
            return publicKey;
        }

        public static PrivateKey Random()
        {
            byte[] seed = new byte[Ed25519.PrivateKeySeedSizeInBytes];
            RandomUtils.GetBytes(seed);
            return new PrivateKey(seed);
        }

        /// <summary>
        /// Compartor for two private keys.
        /// </summary>
        /// <param name="lhs">First private key in comparison..</param>
        /// <param name="rhs">Second private key in comparison.</param>
        /// <returns></returns>
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
        /// Sign a message using the extended private key.
        /// </summary>
        /// <param name="message">The message to sign, represented in bytes.</param>
        /// <returns>The signature generated for the message as an object</returns>
        public Signature Sign(byte[] message)
        {
            ArraySegment<byte> signature = new ArraySegment<byte>(new byte[64]);
            Ed25519.Sign(signature,
                new ArraySegment<byte>(message),
                new ArraySegment<byte>(_extendedKeyBytes));
            return new Signature(signature.Array);
        }

        /// <summary>
        /// Serialize private key
        /// </summary>
        /// <param name="serializer">Serializer object</param>
        public void Serialize(Serialization serializer)
        {
            serializer.SerializeBytes(this.KeyBytes);
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