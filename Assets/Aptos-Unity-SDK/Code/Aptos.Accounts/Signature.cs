using Aptos.BCS;
using Chaos.NaCl;
using System;

namespace Aptos.Accounts
{
    /// <summary>
    /// Representation of a ED25519 signature
    /// </summary>
    public class Signature: ISerializable
    {
        /// <summary>
        /// Signature length
        /// </summary>
        public const int SignatureLength = 64;

        /// <summary>
        /// Byte representation of the signature
        /// </summary>
        private readonly byte[] _signatureBytes;

        /// <summary>
        /// Hex-string representation of the signature
        /// </summary>
        private string _signature;

        /// <summary>
        /// Initialize the signature.
        /// </summary>
        /// <param name="signature">The raw signature in byte array format.</param>
        public Signature(byte[] signature)
        {
            _signatureBytes = signature;
        }

        /// <summary>
        /// The signature data in 64-bytes.
        /// </summary>
        /// <returns>64-byte array representing the signature data</returns>
        public byte[] Data() => _signatureBytes;

        /// <summary>
        /// Serialize signature
        /// </summary>
        /// <param name="serializer">Serializer object</param>
        public void Serialize(Serialization serializer)
        {
            serializer.SerializeBytes(this._signatureBytes);
        }

        public static Signature Deserialize(Deserialization deserializer)
        {
            byte[] sigBytes = deserializer.ToBytes();
            if (sigBytes.Length != Signature.SignatureLength)
                throw new Exception("Length mismatch");

            return new Signature(sigBytes);
        }

        /// <inheritdoc cref="GetHashCode"/>
        public override int GetHashCode() => this.ToString().GetHashCode();

        /// <inheritdoc cref="ToString"/>
        public override string ToString()
        {
            string signatureHex = CryptoBytes.ToHexStringLower(_signatureBytes);
            _signature = "0x" + signatureHex;

            return _signature;
        }

        /// <inheritdoc cref="Equals(object)"/>
        public override bool Equals(object obj)
        {
            if (obj is Signature signature)
                return signature.ToString() == this.ToString();

            return false;
        }
    }
}