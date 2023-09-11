using Chaos.NaCl;
using System;
using Aptos.HdWallet.Utils;
using Aptos.BCS;
using System.Text;
using System.Linq;

namespace Aptos.Accounts
{
    /// <summary>
    /// A class representing the available authorization key schemes for Aptos Blockchain accounts.
    /// </summary>
    public class AuthKeyScheme
    {
        /// <summary>
        /// The ED25519 authorization key scheme value.
        /// </summary>
        public const byte Ed25519 = 0x00;

        /// <summary>
        /// The multi-ED25519 authorization key scheme value.
        /// </summary>
        public const byte MultiEd25519 = 0x01;

        /// <summary>
        /// The authorization key scheme value used to derive an object address from a GUID.
        /// </summary>
        public const byte DeriveObjectAddressFromGuid = 0xFD;

        /// <summary>
        /// The authorization key scheme value used to derive an object address from a seed.
        /// </summary>
        public const byte DeriveObjectAddressFromSeed = 0xFE;

        /// <summary>
        /// The authorization key scheme value used to derive a resource account address.
        /// </summary>
        public const byte DeriveResourceAccountAddress = 0xFF;
    }

    /// <summary>
    /// Represents an Aptos account address.
    /// More details can her found <see cref="https://aptos.dev/concepts/accounts">here</see>.
    /// </summary>
    public class AccountAddress: ISerializableTag
    {
        /// <summary>
        /// The length of the data in bytes.
        /// </summary>
        private static readonly int Length = 32;

        /// <summary>
        /// The address data itself represented in byte array formatting.
        /// </summary>
        private readonly byte[] AddressBytes;

        /// <summary>
        /// Initializes an account address by setting a 32-byte representation of an address.
        /// </summary>
        /// <param name="address">Byte array representing address.</param>
        public AccountAddress(byte[] address)
        {
            if (address.Length != Length)
                throw new ArgumentException("Address must be " + Length + " bytes");

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
            string addressHex = BitConverter.ToString(AddressBytes);
            addressHex = addressHex.Replace("-", "").ToLowerInvariant();
            if (IsSpecial())
            {
                addressHex = addressHex.TrimStart('0');
                if (string.IsNullOrEmpty(addressHex))
                    addressHex = "0";
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

        /// <summary>
        /// Create an AccountAddress instance from a MultiPublicKey.
        ///
        /// This function creates an AccountAddress instance from a provided MultiPublicKey. The function generates a new address by appending the Multi ED25519 authorization key
        /// scheme value to the byte representation of the provided MultiPublicKey, and then computes the SHA3-256 hash of the resulting byte array. The resulting hash is used to create
        /// a new AccountAddress instance.
        /// </summary>
        /// <param name="keys">A MultiPublicKey instance representing the multiple public keys to create the AccountAddress instance from.</param>
        /// <returns>An AccountAddress instance created from the provided MultiPublicKey.</returns>
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

        /// <summary>
        /// Create an AccountAddress instance for a resource account.
        ///
        /// This function creates an AccountAddress instance for a resource account given the creator's address and a seed value. The function generates a new address by concatenating the byte
        /// representation of the creator's address, the provided seed value, and the DERIVE_RESOURCE_ACCOUNT_ADDRESS authorization key scheme value. It then computes the SHA3-256
        /// hash of the resulting byte array to generate a new AccountAddress instance.
        /// </summary>
        /// <param name="creator">An AccountAddress instance representing the address of the account that will create the resource account.</param>
        /// <param name="seed">A byte array used to create a unique resource account.</param>
        /// <returns>An AccountAddress instance representing the newly created resource account.</returns>
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

        /// <summary>
        /// Generates an `AccountAddress` for a GUID object.
        ///
        /// This function takes in a creator address and a `creationNum` which it uses to serialize into an array of bytes.
        /// It then appends the creator address and `deriveObjectAddressFromGuid` to this array. It uses this byte array
        /// to compute a SHA-256 hash. This hash is then returned as a new `AccountAddress`.
        /// </summary>
        /// <param name="creator">The account address of the creator.</param>
        /// <param name="creationNum">The creation number of the object.</param>
        /// <returns>An `AccountAddress` which is generated for a GUID object.</returns>
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

        /// <summary>
        /// Create an AccountAddress instance for a named object.
        ///
        /// This function creates an AccountAddress instance for a named object given the creator's address and a seed value. The function generates a new address by concatenating the byte representation
        /// of the creator's address, the provided seed value, and the DERIVE_OBJECT_ADDRESS_FROM_SEED authorization key scheme value. It then computes the SHA3-256 hash of the resulting byte
        /// array to generate a new AccountAddress instance.
        /// </summary>
        /// <param name="creator">An AccountAddress instance representing the address of the account that will create the named object.</param>
        /// <param name="seed">A byte array used to create a unique named object.</param>
        /// <returns>An AccountAddress instance representing the newly created named object.</returns>
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

        /// <summary>
        /// Generates an AccountAddress for a named token by concatenating the collectionName, the tokenName, and the separator "::"
        /// as a Data and calling the forNamedObject function with the resulting Data as the seed.
        /// </summary>
        /// <param name="creator">The AccountAddress of the account that creates the token./param>
        /// <param name="collectionName">A String that represents the name of the collection to which the token belongs.</param>
        /// <param name="tokenName">A String that represents the name of the token.</param>
        /// <returns>An AccountAddress object that represents the named token.</returns>
        public static AccountAddress ForNamedToken(AccountAddress creator, string collectionName, string tokenName)
        {
            byte[] result = Encoding.ASCII.GetBytes(collectionName + "::" + tokenName);
            return AccountAddress.ForNamedObject(
                creator, result
            );
        }

        /// <summary>
        /// Derive an AccountAddress for a named collection.
        ///
        /// This function takes the creator's AccountAddress and the name of the collection as a String. The collection name is
        /// then converted to data using UTF-8 encoding. The forNamedObject function is called with the creator's address and the
        /// collection name data as the seed. This returns an AccountAddress derived from the creator's address and collection name
        /// seed, which represents the named collection.
        /// </summary>
        /// <param name="creator">The creator's AccountAddress.</param>
        /// <param name="collectionName">The name of the collection as a String.</param>
        /// <returns>An AccountAddress that represents the named collection.</returns>
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