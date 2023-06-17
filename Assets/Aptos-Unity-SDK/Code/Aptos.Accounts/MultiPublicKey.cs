using System;
using System.Collections.Generic;
using Aptos.BCS;

namespace Aptos.Accounts
{
    public class MultiPublicKey : ISerializable
    {
        public List<PublicKey> Keys;
        public byte Threshold;

        public static int MIN_KEYS = 2;
        public static int MAX_KEYS = 32;
        public static int MIN_THRESHOLD = 1;

        public MultiPublicKey(List<PublicKey> Keys, byte Threshold, bool Checked = true)
        {
            if(Checked)
            {
                if(!(MIN_KEYS <= Keys.Count && Keys.Count <= MAX_KEYS))
                {
                    throw new Exception("Must have between " + MIN_KEYS + " and " + MAX_KEYS + " keys.");
                }

                if(!(MIN_THRESHOLD <= Threshold && Threshold < Keys.Count))
                {
                    throw new Exception("Threshold must be between " +MIN_THRESHOLD + " and " + Keys.Count);
                }
            }

            this.Keys = Keys;
            this.Threshold = Threshold;
        }

        public override string ToString()
        {
            return string.Format("{0}-of-{1} Multi-Ed25519 public key", this.Threshold, this.Keys.Count);
        }

        public byte[] ToBytes()
        {
            List<byte> concatenatedKeys = new List<byte>();
            foreach (PublicKey key in this.Keys)
                foreach (byte aByte in key.KeyBytes)
                    concatenatedKeys.Add(aByte);

            concatenatedKeys.Add(this.Threshold);
            return concatenatedKeys.ToArray();
        }

        public static MultiPublicKey FromBytes(byte[] Key)
        {
            // Get key count and threshold limits.
            int minKeys = MIN_KEYS;
            int maxKeys = MAX_KEYS;
            int minThreshold = MIN_THRESHOLD;

            // Get number of signers.
            int nSigners = (int)Key.Length / PublicKey.KeyLength;
            if(!(minKeys <= nSigners && nSigners <= maxKeys))
            {
                throw new Exception(string.Format("Must have between {0} and {1} keys.", minKeys, maxKeys));
            }

            // Get threshold.
            byte threshold = Key[Key.Length - 1];
            if(!(minThreshold <= threshold && threshold <= nSigners)) {
                throw new Exception(string.Format("Threshold must be between {0} and {1}.", minThreshold, nSigners));
            }
            List<PublicKey> keys = new List<PublicKey>(); // Initialize empty keys list.

            for(int i = 0; i < nSigners; i++) // Loop over all signers.
            {
                // Extract public key for signle signer.
                int startByte = i * PublicKey.KeyLength;
                int endByte = (i + 1) * PublicKey.KeyLength;

                byte[] tempKey = Key[startByte.. endByte];
                PublicKey publicKey = new PublicKey(tempKey);
                keys.Add(publicKey);
            }
            return new MultiPublicKey(keys, threshold);
        }

        public void Serialize(Serialization serializer)
        {
            serializer.SerializeBytes(this.ToBytes());
        }
    }
}