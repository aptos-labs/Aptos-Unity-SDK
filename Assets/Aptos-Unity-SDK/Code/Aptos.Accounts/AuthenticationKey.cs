using Aptos.Accounts.Types;
using Chaos.NaCl;
using System;

namespace Aptos.Accounts
{
    /// <summary>
    /// Represents an Authentication Key \n
    /// During the account creation process, a 32-byte authentication key comes to exist first. \n
    /// This authentication key is then returned as it is as the 32-byte account Address. \n
    /// 
    /// NOTE: Generating the authentication key for an account requires that you provide one of  \n
    /// the below 1-byte signature scheme identifiers for this account, i.e., \n
    /// whether the account is a single signature or a multisig account: \n
    /// More info on account addresses found <see cref="https://aptos.dev/concepts/accounts/#account-Address">here</see>.
    /// </summary>
    public class AuthenticationKey
    {
        /// Byte length of authentication key.
        public static int LENGTH = 32;
        /// Byte that represents multi-ed25519 scheme.
        public static byte MULTI_ED25519_SCHEME = 0x001;
        /// Byte that represents single key ed25519 scheme.
        public static byte ED25519_SCHEME = 0x00;
        /// Byte that represents derive resource account scheme.
        public static byte DERIVE_RESOURCE_ACCOUNT_SCHEME = 255;

        /// Byte array representing authentication key
        public byte[] bytes;

        public AuthenticationKey(byte[] bytes)
        {
            if (bytes.Length != AuthenticationKey.LENGTH)
            {
                throw new ArgumentException("Byte array must be " + AuthenticationKey.LENGTH + " bytes");
            }
            this.bytes = bytes;
        }

        /// <summary>
        /// Converts a K-of-N MultiEd25519PublicKey to AuthenticationKey with: \n
        /// `auth_key = sha3-256(p_1 | … | p_n | K | 0x01)`. `K` represents the K-of-N required for \n
        /// authenticating the transaction. `0x01` is the 1-byte scheme for multisig. \n
        /// </summary>
        /// <returns>Authentication key object from a multi ED25519 key</returns>
        public static AuthenticationKey FromMultiEd25519PublicKey(MultiEd25519PublicKey publicKey)
        {
            byte[] pubKeyBytes = publicKey.ToBytes();
            byte[] bytes = new byte[pubKeyBytes.Length + 1];

            Array.Copy(pubKeyBytes, bytes, pubKeyBytes.Length);
            bytes[pubKeyBytes.Length] = AuthenticationKey.MULTI_ED25519_SCHEME;

            var sha256 = new Org.BouncyCastle.Crypto.Digests.Sha3Digest(256); // SHA256 it
            sha256.BlockUpdate(bytes, 0, bytes.Length);

            byte[] result = new byte[Ed25519.PublicKeySizeInBytes]; // Result hash must be 32 bytes
            sha256.DoFinal(result, 0);

            return new AuthenticationKey(result);
        }

        /// <summary>
        /// Converts single Public Key (bytes) into Authentication Key \n
        /// auth_key = sha3-256(pubkey_A | 0x00) \n
        /// where | denotes concatenation. The 0x00 is the 1-byte single-signature scheme identifier. \n
        /// </summary>
        /// <param name="publicKey">Publick key, in byte array format, used to generate the authentication key</param>
        /// <returns>Authentication key object</returns>
        public static AuthenticationKey FromEd25519PublicKey(byte[] publicKey)
        {
            var pubKeyBytes = publicKey;

            byte[] bytes = new byte[pubKeyBytes.Length + 1];
            Array.Copy(pubKeyBytes, bytes, pubKeyBytes.Length);
            bytes[pubKeyBytes.Length] = AuthenticationKey.ED25519_SCHEME;

            var sha256 = new Org.BouncyCastle.Crypto.Digests.Sha3Digest(256); // SHA256 it
            sha256.BlockUpdate(bytes, 0, bytes.Length);
            byte[] result = new byte[Ed25519.PublicKeySizeInBytes]; // Result hash must be 32 bytes
            sha256.DoFinal(result, 0);

            return new AuthenticationKey(result);
        }

        /// <summary>
        /// Derives hexadecimal Address from authentication key array of bytes.
        /// </summary>
        /// <returns>A hexa string representation Address of the authentication key</returns>
        public string DerivedAddress()
        {
            string hexString = CryptoBytes.ToHexStringLower(bytes);
            if (!hexString.StartsWith("0x"))
            {
                hexString = "0x" + hexString;
            }
            return hexString;
        }
    }
}