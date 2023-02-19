using Chaos.NaCl;

namespace Aptos.Accounts.Types
{
    /// <summary>
    /// Represents a multi-signature public key.
    /// More details can be found <see cref="https://aptos.dev/guides/creating-a-signed-transaction/#multisignature-transactions">here</see>.
    /// </summary>
    public class MultiEd25519PublicKey
    {
        /// Max number of signature supported
        public static int MAX_SIGNATURES_SUPPORTED = 32;

        /// List of public keys.
        public PublicKey[] PublicKeys { get; }

        /// The max number of keys accepted in the multi-sig.
        public int threshold { get; }

        /// <summary>
        /// Public key for a K-of-N multisig transaction. A K-of-N multisig transaction means that for such a \n
        /// transaction to be executed, at least K out of the N authorized signers have signed the transaction \n
        /// and passed the check conducted by the chain. \n
        ///  See <see cref="https://aptos.dev/guides/creating-a-signed-transaction#multisignature-transactions">Creating a Signed Transaction</see>.
        /// </summary>
        /// <param name="PublicKeys">List of public keys used to create the multi-sig</param>
        /// <param name="threshold">The max number of keys accepted in the multi-sig.</param>
        public MultiEd25519PublicKey(PublicKey[] PublicKeys, int threshold)
        {
            if (threshold > MAX_SIGNATURES_SUPPORTED)
            {
                //throw new Error("'threshold' cannot be larger than " MAX_SIGNATURES_SUPPORTED);
            }
            this.PublicKeys = PublicKeys;
            this.threshold = threshold;
        }

        /// <summary>
        /// Converts a MultiEd25519PublicKey into bytes \n
        /// with: <code>bytes = p1_bytes | ... | pn_bytes | threshold</code>.
        /// </summary>
        /// <returns>Byte array representing multi-sign public key.</returns>
        public byte[] ToBytes()
        {
            byte[] bytes = new byte[(PublicKeys.Length * Ed25519.PublicKeySizeInBytes) + 1];
            for (int i = 0; i < PublicKeys.Length; i++)
            {
                bytes.SetValue(PublicKeys[i], i * Ed25519.PublicKeySizeInBytes);
            }

            bytes[this.PublicKeys.Length * Ed25519.PublicKeySizeInBytes] = (byte)threshold;

            return bytes;
        }
    }
}