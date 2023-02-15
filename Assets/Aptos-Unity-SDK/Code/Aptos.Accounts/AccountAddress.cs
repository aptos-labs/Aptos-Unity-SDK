using Chaos.NaCl;
using System;
using Aptos.HdWallet.Utils;
using Aptos.Utilities.BCS;

namespace Aptos.Accounts
{
    /// <summary>
    /// Represents an Aptos account address.
    /// More details can her found <see cref="https://aptos.dev/concepts/accounts">here</see>.
    /// </summary>
    public class AccountAddress
    {
        private static readonly int Length = 32;
        private readonly byte[] AddressBytes;

        /// <summary>
        /// Initializes an account address by setting a 32-byte representation of an address
        /// </summary>
        /// <param name="address">Byte array representing address.</param>
        public AccountAddress(byte[] address)
        {
            if (address.Length != Length)
            {
                throw new ArgumentException("Address must be " + Length + " bytes");
            }
            this.AddressBytes = address;
        }

        /// <summary>
        /// Convert Address bytes into hexadecimal string.
        /// </summary>
        /// <returns>String representation of account address</returns>
        public override string ToString()
        {
            return ToHexString();
        }

        /// <summary>
        /// Convert Address to hexadecimal string.
        /// </summary>
        /// <returns>Address as hexadecimal string</returns>
        private string ToHexString()
        {
            string addressHex = BitConverter.ToString(AddressBytes); // Turn into hexadecimal string
            addressHex = addressHex.Replace("-", "").ToLowerInvariant(); // Remove '-' characters from hexa hash
            return "0x" + addressHex;
        }

        /// <summary>
        /// Returns an AccountAddress object from a hexadecimal Address.
        /// </summary>
        /// <param name="address">Hexadecimal representation of an Address.</param>
        /// <returns>An account address object</returns>
        public static AccountAddress FromHex(string address)
        {
            string addr = address;
            if (address[0..2].Equals("0x")) { addr = address[2..]; }

            if (addr.Length < AccountAddress.Length * 2)
            {
                string pad = new string('0', AccountAddress.Length * 2 - addr.Length);
                addr = pad + addr;
            }

            return new AccountAddress(addr.HexStringToByteArray());
        }

        /// <summary>
        /// Generate an AccountAddress object from a given public key, byte array. 
        /// </summary>
        /// <param name="publicKey">Byte array representing a public key.</param>
        /// <returns>An account address object.</returns>
        public static AccountAddress FromKey(byte[] publicKey)
        {
            var sha256 = new Org.BouncyCastle.Crypto.Digests.Sha3Digest(256); // SHA256 it
            var addressBytes = new byte[Ed25519.PublicKeySizeInBytes + 1]; // +1 to contain signature scheme byte
            Array.Copy(publicKey, 0, addressBytes, 0, Ed25519.PublicKeySizeInBytes); // copy 32 bytes only

            byte sigScheme = 0x00; // signature scheme byte
            addressBytes[publicKey.Length] = sigScheme; // Append signature scheme byte to the end

            sha256.BlockUpdate(addressBytes, 0, addressBytes.Length);
            byte[] result = new byte[Ed25519.PublicKeySizeInBytes]; // Result hash must be 32 bytes
            sha256.DoFinal(result, 0);

            return new AccountAddress(result);
        }

        /// <summary>
        /// Reference implementation to BCS can be found <see cref="https://github.com/aptos-labs/bcs">here</see>.   
        /// More details on creating a BCS signed transaction can be found <see cref="https://aptos.dev/guides/creating-a-signed-transaction/#bcs">here</see>.
        ///
        /// Binary Canonical Serialization (BCS) is a serialization format applied to the raw (unsigned) transaction. 
        /// See BCS for a description of the design goals of BCS.
        /// BCS is not a self-describing format.As such, in order to deserialize a message, 
        /// one must know the message type and layout ahead of time.
        /// </summary>
        public void Serialize(Serialization serializer)
        {
            serializer.SerializeFixedBytes(this.AddressBytes);
        }
    }
}