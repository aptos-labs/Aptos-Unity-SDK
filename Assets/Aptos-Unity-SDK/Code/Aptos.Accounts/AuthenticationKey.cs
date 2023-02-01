using Aptos.Accounts.Types;
using Chaos.NaCl;

namespace Aptos.Accounts
{
    /// <summary>
    /// Represents an Authentication Key
    /// During the account creation process, a 32-byte authentication key comes to exist first. 
    /// This authentication key is then returned as it is as the 32-byte account Address.
    /// 
    /// NOTE: Generating the authentication key for an account requires that you provide one of 
    /// the below 1-byte signature scheme identifiers for this account, i.e., 
    /// whether the account is a single signature or a multisig account:
    /// https://aptos.dev/concepts/accounts/#account-Address
    /// </summary>
    public class AuthenticationKey
    {
        public static int LENGTH = 32;
        public static byte MULTI_ED25519_SCHEME = 0x001;
        public static byte ED25519_SCHEME = 0x00;
        public static byte DERIVE_RESOURCE_ACCOUNT_SCHEME = 255;
        public byte[] bytes;

        public AuthenticationKey(byte[] bytes)
        {
            if (bytes.Length != AuthenticationKey.LENGTH)
            {
                //TODO: throw new Error("Expected a byte array of length 32");
            }
            this.bytes = bytes;
        }

        /// <summary>
        /// Converts a K-of-N MultiEd25519PublicKey to AuthenticationKey with:
        /// `auth_key = sha3-256(p_1 | … | p_n | K | 0x01)`. `K` represents the K-of-N required for
        /// authenticating the transaction. `0x01` is the 1-byte scheme for multisig.
        /// </summary>
        /// <returns></returns>
        public static AuthenticationKey FromMultiEd25519PublicKey(MultiEd25519PublicKey publicKey)
        {
            byte[] pubKeyBytes = publicKey.ToBytes();
            byte[] bytes = new byte[pubKeyBytes.Length + 1];

            bytes.SetValue(pubKeyBytes, 0);
            bytes.SetValue(AuthenticationKey.MULTI_ED25519_SCHEME, pubKeyBytes.Length);

            var sha256 = new Org.BouncyCastle.Crypto.Digests.Sha3Digest(256); // SHA256 it
            sha256.BlockUpdate(bytes, 0, bytes.Length);

            byte[] result = new byte[Ed25519.PublicKeySizeInBytes]; // Result hash must be 32 bytes
            sha256.DoFinal(result, 0);

            return new AuthenticationKey(result);
        }

        /// <summary>
        /// Converts single Public Key (bytes) into Authentication Key
        /// auth_key = sha3-256(pubkey_A | 0x00)
        /// where | denotes concatenation. The 0x00 is the 1-byte single-signature scheme identifier.
        /// </summary>
        /// <param name="publicKey"></param>
        /// <returns></returns>
        public static AuthenticationKey FromEd25519PublicKey(byte[] publicKey)
        {
            var pubKeyBytes = publicKey;

            byte[] bytes = new byte[pubKeyBytes.Length + 1];
            bytes.SetValue(pubKeyBytes, 0);
            bytes.SetValue(AuthenticationKey.ED25519_SCHEME, pubKeyBytes.Length);

            var sha256 = new Org.BouncyCastle.Crypto.Digests.Sha3Digest(256); // SHA256 it
            sha256.BlockUpdate(bytes, 0, bytes.Length);
            byte[] result = new byte[Ed25519.PublicKeySizeInBytes]; // Result hash must be 32 bytes
            sha256.DoFinal(result, 0);

            return new AuthenticationKey(result);
        }

        /// <summary>
        /// Derives hexadecimal Address from authentication key array of bytes.
        /// </summary>
        /// <returns></returns> A hexa string representation Address of the authentication key.
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