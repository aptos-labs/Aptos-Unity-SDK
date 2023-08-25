using Chaos.NaCl;
using System;
using Aptos.HdWallet.Utils;
using Aptos.BCS;
using System.Text;
using System.Linq;
using Aptos.Unity.Rest.Model.Resources;
using Org.BouncyCastle.Utilities;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.TextCore.Text;

namespace Aptos.Accounts
{
    public class AuthKeyScheme
    {
        public const byte Ed25519 = 0x00;
        public const byte MultiEd25519 = 0x01;
        public const byte DeriveObjectAddressFromGuid = 0xFD;
        public const byte DeriveObjectAddressFromSeed = 0xFE;
        public const byte DeriveResourceAccountAddress = 0xFF;
    }

    /// <summary>
    /// Represents an Aptos account address.
    /// More details can her found <see cref="https://aptos.dev/concepts/accounts">here</see>.
    /// </summary>
    public class AccountAddress: ISerializableTag
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
        /// Represent an account address in a way that is compliant with the v1 address
        /// standard.The standard is defined as part of AIP-40, read more here:
        /// https://github.com/aptos-foundation/AIPs/blob/main/aips/aip-40.md
        /// </summary>
        /// <returns>String representation of account address</returns>
        public override string ToString()
        {
            string addressHex = BitConverter.ToString(AddressBytes); // Turn into hexadecimal string
            addressHex = addressHex.Replace("-", "").ToLowerInvariant(); // Remove '-' characters from hexa hash
            if (IsSpecial())
            {
                addressHex = addressHex.TrimStart('0');
                if (string.IsNullOrEmpty(addressHex))
                {
                    addressHex = "0";
                }
            }
            return "0x" + addressHex;
        }

        /// <summary>
        /// Returns whether the address is a "special" address. Addresses are considered
        /// special if the first 63 characters of the hex string are zero.In other words,
        /// an address is special if the first 31 bytes are zero and the last byte is
        /// smaller than `0b10000` (16). In other words, special is defined as an address
        /// that matches the following regex: `^0x0{63}[0-9a-f]$`. In short form this means
        /// the addresses in the range from `0x0` to `0xf` (inclusive) are special.
        ///
        /// For more details see the v1 address standard defined as part of AIP-40:
        /// https://github.com/aptos-foundation/AIPs/blob/main/aips/aip-40.md
        /// </summary>
        /// <returns>Boolean representing if the address is special or not.</returns>
        private bool IsSpecial()
        {
            return
                AddressBytes.Take(AddressBytes.Length - 1).All(b => b == 0) &&
                (AddressBytes[AddressBytes.Length - 1] < 0b10000);
        }

        /// <summary>
        /// Returns an AccountAddress object from a hexadecimal Address.
        /// </summary>
        /// <param name="address">Hexadecimal representation of an Address.</param>
        /// <returns>An account address object</returns>
        public static AccountAddress FromHex(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("Address string is empty.");

            if (address.Length == 1)
            {
                address = "0x" + address;
            }

            string addr = address;

            if (address[0..2].Equals("0x")) { addr = address[2..]; }

            if (addr.Length < AccountAddress.Length * 2)
            {
                string pad = new string('0', AccountAddress.Length * 2 - addr.Length);
                addr = pad + addr;
            }

            return new AccountAddress(addr.ByteArrayFromHexString());
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

        public static AccountAddress FromMultiEd25519(MultiPublicKey keys)
        {
            var sha256 = new Org.BouncyCastle.Crypto.Digests.Sha3Digest(256); // SHA256 it
            byte[] keyBytes = keys.ToBytes();
            byte authKeyScheme = AuthKeyScheme.MultiEd25519;
            sha256.BlockUpdate(keyBytes, 0, keyBytes.Length);
            sha256.Update(authKeyScheme);
            byte[] result = new byte[Ed25519.PublicKeySizeInBytes];
            sha256.DoFinal(result, 0);
            return new AccountAddress(result);
        }

        public static AccountAddress ForResourceAccount(AccountAddress creator, byte[] seed)
        {
            var sha256 = new Org.BouncyCastle.Crypto.Digests.Sha3Digest(256); // SHA256 it
            sha256.BlockUpdate(creator.AddressBytes, 0, creator.AddressBytes.Length);
            sha256.BlockUpdate(seed, 0, seed.Length);
            sha256.Update(AuthKeyScheme.DeriveResourceAccountAddress);
            byte[] result = new byte[Ed25519.PublicKeySizeInBytes];
            sha256.DoFinal(result, 0);
            return new AccountAddress(result);
        }

        public static AccountAddress ForGuidObject(AccountAddress creator, int creationNum)
        {
            var sha256 = new Org.BouncyCastle.Crypto.Digests.Sha3Digest(256); // SHA256 it
            Serialization serializer = new Serialization();
            serializer.SerializeU64((ulong)creationNum);
            byte[] output = serializer.GetBytes();
            sha256.BlockUpdate(output, 0, output.Length);
            sha256.BlockUpdate(creator.AddressBytes, 0, creator.AddressBytes.Length);
            sha256.Update(AuthKeyScheme.DeriveObjectAddressFromGuid);
            byte[] result = new byte[Ed25519.PublicKeySizeInBytes];
            sha256.DoFinal(result, 0);
            return new AccountAddress(result);
        }

        public static AccountAddress ForNamedObject(AccountAddress creator, byte[] seed)
        {
            var sha256 = new Org.BouncyCastle.Crypto.Digests.Sha3Digest(256); // SHA256 it
            sha256.BlockUpdate(creator.AddressBytes, 0, creator.AddressBytes.Length);
            sha256.BlockUpdate(seed, 0, seed.Length);
            sha256.Update(AuthKeyScheme.DeriveObjectAddressFromSeed);
            byte[] result = new byte[Ed25519.PublicKeySizeInBytes];
            sha256.DoFinal(result, 0);
            return new AccountAddress(result);
        }

        public static AccountAddress ForNamedToken(AccountAddress creator, string collectionName, string tokenName)
        {
            byte[] result = Encoding.ASCII.GetBytes(collectionName + "::" + tokenName);
            return AccountAddress.ForNamedObject(
                creator, result
            );
        }

        public static AccountAddress ForNamedCollection(AccountAddress creator, string collectionName)
        {
            byte[] collectionNameEncode = Encoding.ASCII.GetBytes(collectionName);
            return AccountAddress.ForNamedObject(creator, collectionNameEncode);
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

        public static AccountAddress Deserialize(Deserialization deserializer)
        {
            return new AccountAddress(deserializer.FixedBytes(AccountAddress.Length));
        }

        public TypeTag Variant()
        {
            return TypeTag.ACCOUNT_ADDRESS;
        }

        public object GetValue()
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object other)
        {
            if (other is not AccountAddress)
                throw new NotImplementedException("::: " + other.GetType());

            AccountAddress otherAcctAddr = (AccountAddress)other;

            return this.ToString().Equals(otherAcctAddr.ToString());
        }

        public override int GetHashCode()
        {
            return this.AddressBytes.GetHashCode();
        }
    }
}