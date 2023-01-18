using Chaos.NaCl;

namespace Aptos.Accounts.Types
{
    /// <summary>
    /// Represents a multi-signature public key
    /// https://aptos.dev/guides/creating-a-signed-transaction/#multisignature-transactions
    /// </summary>
    public class MultiEd25519PublicKey
    {
        public static int MAX_SIGNATURES_SUPPORTED = 32;
        public PublicKey[] PublicKeys { get; }
        public int threshold { get; }

        /// <summary>
        /// Public key for a K-of-N multisig transaction. A K-of-N multisig transaction means that for such a
        /// transaction to be executed, at least K out of the N authorized signers have signed the transaction
        /// and passed the check conducted by the chain.
        /// 
        /// https://aptos.dev/guides/creating-a-signed-transaction#multisignature-transactions | Creating a Signed Transaction}
        /// 
        /// </summary>
        /// <param name="PublicKeys"></param>
        /// <param name="threshold"></param>
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
        /// Converts a MultiEd25519PublicKey into bytes 
        /// with: bytes = p1_bytes | ... | pn_bytes | threshold
        /// </summary>
        /// <returns></returns>
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