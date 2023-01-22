using Chaos.NaCl;
using NBitcoin;
using System;

namespace Aptos.Accounts
{
    /// <summary>
    /// Represents an Aptos Accounts
    /// An Aptos account is represented by an extended private key
    /// , a public key and it's athentication key
    /// </summary>
    public class Account
    {
        public PrivateKey PrivateKey { get; set; }
        public PublicKey PublicKey { get; set; }
        public AccountAddress AccountAddress { get; set; }

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
            PrivateKeyShort = seed;
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
        /// <returns></returns> A new account.
        public static Account Generate()
        {
            return new Account();
        }

        /// <summary>
        /// Returns the Authentication Key for the associated account.
        /// </summary>
        /// <returns></returns>
        public string AuthKey()
        {
            var pubKey = PublicKey;
            var authKey = AuthenticationKey.FromEd25519PublicKey(pubKey);
            return authKey.DerivedAddress();
        }

        /// <summary>
        /// Verify a given signed message with the current account's public key
        /// </summary>
        /// <param name="message"></param> The signed message.
        /// <param name="signature"></param> The signature of the message
        /// <returns></returns>
        public bool Verify(byte[] message, byte[] signature)
        {
            return PublicKey.Verify(message, signature);
        }

        /// <summary>
        /// Sign a given byte array (data) with the current account's private key
        /// </summary>
        /// <param name="message"></param> The signature of the data.
        /// <returns></returns>
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