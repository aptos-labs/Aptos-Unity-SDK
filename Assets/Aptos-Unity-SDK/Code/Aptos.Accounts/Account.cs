using Aptos.BCS;
using Chaos.NaCl;
using NBitcoin;
using System;

namespace Aptos.Accounts
{
    /// <summary>
    /// Represents an Aptos Accounts.   
    /// An Aptos account is represented by an extended private key, 
    /// a public key and it's authentication key.
    /// </summary>
    public class Account
    {
        /// <summary>
        /// Private key representation.
        /// </summary>
        public PrivateKey PrivateKey { get; set; }

        /// <summary>
        /// Public key representation.
        /// </summary>
        public PublicKey PublicKey { get; set; }

        /// <summary>
        /// Account address representation.
        /// </summary>
        public AccountAddress AccountAddress { get; set; }

        /// <summary>
        /// 32-byte representation of the private key.
        /// </summary>
        public byte[] PrivateKeyShort { get; }

        /// <summary>
        /// Create a new Aptos Account.
        /// </summary>
        public Account()
        {
            byte[] seed = new byte[Ed25519.PrivateKeySeedSizeInBytes];
            RandomUtils.GetBytes(seed);

            PrivateKey = new PrivateKey(seed);
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
        /// Utility function to be in par with the other SDKs
        /// , otherwise use the default constructor Account().
        /// </summary>
        /// <returns>A new account.</returns>
        public static Account Generate() => new Account();

        /// <summary>
        /// Creates an account from private key in string format.
        /// </summary>
        /// <param name="privateKeyHex">The private key.</param>
        /// <returns>A new account.</returns>
        public static Account LoadKey(string privateKeyHex)
        {
            PrivateKey privateKey = new PrivateKey(privateKeyHex);
            PublicKey publicKey = privateKey.PublicKey();
            return new Account(privateKey, publicKey);
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
        public bool Verify(byte[] message, Signature signature)
        {
            return PublicKey.Verify(message, signature);
        }

        /// <summary>
        /// Sign a given byte array (data) with the current account's private key
        /// </summary>
        /// <param name="message"></param> The signature of the data.
        /// <returns>The signature as an object</returns>
        public Signature Sign(byte[] message) => PrivateKey.Sign(message);

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

    /// <summary>
    /// Used for the rotating proof challenge on the Aptos Blockchain.
    /// </summary>
    public class RotationProofChallenge
    {
        /// <summary>
        /// The account used for the challenge (i.e., 0x1).
        /// </summary>
        AccountAddress TypeInfoAccountAddress = AccountAddress.FromHex("0x1");

        /// <summary>
        /// The name of the module for the challenge.
        /// </summary>
        string TypeInfoModuleName = "account";

        /// <summary>
        /// The struct's name.
        /// </summary>
        string TypeInfoStructName = "RotationProofChallenge";

        /// <summary>
        /// The current sequence number of the account.
        /// </summary>
        int SequenceNumber;

        /// <summary>
        /// The account currently wanting to be changed over.
        /// </summary>
        AccountAddress Originator;

        /// <summary>
        /// The account currently wanting to be changed over.
        /// </summary>
        AccountAddress CurrentAuthKey;

        /// <summary>
        /// The public key of the new account.
        /// </summary>
        byte[] NewPublicKey;

        /// <summary>
        /// Create a new Rotation Proof Challenge.
        /// </summary>
        /// <param name="SequenceNumber">The current sequence number of the account.</param>
        /// <param name="Originator">The account currently wanting to be changed over.</param>
        /// <param name="CurrentAuthKey">The account currently wanting to be changed over.</param>
        /// <param name="NewPublicKey"The public key of the new account.></param>
        public RotationProofChallenge(
            int SequenceNumber,
            AccountAddress Originator,
            AccountAddress CurrentAuthKey,
            byte[] NewPublicKey
        )
        {
            this.SequenceNumber = SequenceNumber;
            this.Originator = Originator;
            this.CurrentAuthKey = CurrentAuthKey;
            this.NewPublicKey = NewPublicKey;
        }

        /// <summary>
        /// Serialize the account object using a provided Serializer object.
        ///
        /// This function takes a Serializer object and serializes the account object's properties, which includes the
        /// typeInfoAccountAddress, typeInfoModuleName, typeInfoStructName, sequence_number, originator, currentAuthKey and newPublicKey
        /// The Serializer object serializes values in the order specified, which is the order of the calls in this function.
        /// </summary>
        /// <param name="serializer">The Serializer object to serialize the account object with.</param>
        public void Serialize(Serialization serializer)
        {
            this.TypeInfoAccountAddress.Serialize(serializer);
            serializer.SerializeString(this.TypeInfoModuleName);
            serializer.SerializeString(this.TypeInfoStructName);
            serializer.SerializeU64((ulong)this.SequenceNumber);
            this.Originator.Serialize(serializer);
            this.CurrentAuthKey.Serialize(serializer);
            serializer.SerializeBytes(this.NewPublicKey);
        }
    }
}