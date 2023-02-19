using Chaos.NaCl;
using NBitcoin;
using System;

namespace Aptos.Accounts
{
    /// <summary>
    /// Represents an Aptos Accounts.   
    /// An Aptos account is represented by an extended private key, 
    /// a public key and it's athentication key.
    /// </summary>
    public class Account
    {
        /// Private key representation
        public PrivateKey PrivateKey { get; set; }
        /// Public key representation
        public PublicKey PublicKey { get; set; }
        /// Account address representation
        public AccountAddress AccountAddress { get; set; }

        /// 32-byte representation of the private key
        public byte[] PrivateKeyShort { get; }

        /// <summary>
        /// Create a new Aptos Account.
        /// </summary>
        public Account()
        {
            byte[] seed = new byte[Ed25519.PrivateKeySeedSizeInBytes];
            RandomUtils.GetBytes(seed);

            PrivateKey = new PrivateKey(Ed25519.ExpandedPrivateKeyFromSeed(seed));
            PublicKey = new PublicKey(Ed25519.PublicKeyFromSeed(seed));
            AccountAddress = AccountAddress.FromKey(PublicKey);
            PrivateKeyShort = new byte[32];
            Array.Copy(seed, 0, PrivateKeyShort, 0, 32);
        }

        /// <summary>
        /// Initialize an account with a given private and public keys.
        /// </summary>
        /// <param name="privateKey">The private key.</param>
        /// <param name="publicKey">The public key.</param>
        public Account(string privateKey, string publicKey)
        {
            PrivateKey = new PrivateKey(privateKey);
            PublicKey = new PublicKey(publicKey);
            AccountAddress = AccountAddress.FromKey(PublicKey);
        }

        /// <inheritdoc cref="Account(string, string)"/>
        public Account(byte[] privateKey, byte[] publicKey)
        {
            PrivateKey = new PrivateKey(privateKey);
            PublicKey = new PublicKey(publicKey);
            AccountAddress = AccountAddress.FromKey(PublicKey);
        }

        /// <summary>
        /// Utility function to be in par with the other SDKS
        /// , otherwise use the default constructor Account().
        /// </summary>
        /// <returns>A new account</returns>
        public static Account Generate()
        {
            return new Account();
        }

        /// <summary>
        /// Returns the Authentication Key for the associated account.
        /// </summary>
        /// <returns>String representation of the authentication key</returns>
        public string AuthKey()
        {
            var pubKey = PublicKey;
            var authKey = AuthenticationKey.FromEd25519PublicKey(pubKey);
            return authKey.DerivedAddress();
        }

        /// <summary>
        /// Verify a given signed message with the current account's public key
        /// </summary>
        /// <param name="message">The signed message.</param>
        /// <param name="signature">The signature of the message.</param>
        /// <returns>True is the signature is valid, False otherwise</returns>
        public bool Verify(byte[] message, byte[] signature)
        {
            return PublicKey.Verify(message, signature);
        }

        /// <summary>
        /// Sign a given byte array (data) with the current account's private key
        /// </summary>
        /// <param name="message"></param> The signature of the data.
        /// <returns>The singed messaged in byte form</returns>
        public byte[] Sign(byte[] message)
        {
            return PrivateKey.Sign(message);
        }

        /// <summary>
        /// Get the ED25519 keypair from the given seed (in bytes).
        /// </summary>
        /// <param name="seed"></param> The seed (in bytes).
        /// <returns>ED25519 key pair</returns>
        internal static (byte[] privateKey, byte[] publicKey) EdKeyPairFromSeed(byte[] seed) =>
            (Ed25519.ExpandedPrivateKeyFromSeed(seed), Ed25519.PublicKeyFromSeed(seed));

        /// <summary>
        /// Generates a random seed for the Ed25519 key pair.
        /// </summary>
        /// <returns>The seed as byte array.</returns>
        private static byte[] GenerateRandomSeed()
        {
            byte[] bytes = new byte[Ed25519.PrivateKeySeedSizeInBytes];
            RandomUtils.GetBytes(bytes);
            return bytes;
        }
    }
}