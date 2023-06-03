using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Aptos.BCS;
using UnityEngine;

namespace Aptos.Accounts
{
    public class MultiPublicKey : ISerializable
    {
        public List<PublicKey> Keys;
        public int Threshold;

        public int MIN_KEYS = 2;
        public int MAX_KEYS = 32;
        public int MIN_THRESHOLD = 1;

        public MultiPublicKey(List<PublicKey> Keys, int Threshold, bool Checked = true)
        {
            if(Checked)
            {
                if(!(this.MIN_KEYS <= Keys.Count && Keys.Count <= this.MAX_KEYS))
                {
                    throw new Exception("Must have between " + this.MIN_KEYS + " and " + this.MAX_KEYS + " keys.");
                }

                if(!(this.MIN_THRESHOLD <= Threshold && Threshold < Keys.Count))
                {
                    throw new Exception("Threshold must be between " + this.MIN_THRESHOLD + " and " + Keys.Count);
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
            {
                //byte[] pubKeyBytes = key.Key.As
                //concatenatedKeys.Concat(key.KeyBytes);
                foreach(byte aByte in key.KeyBytes)
                {
                    concatenatedKeys.Add(aByte);
                }
            }
            concatenatedKeys.Concat(BitConverter.GetBytes(this.Threshold));
            return concatenatedKeys.ToArray();
        }

        public MultiPublicKey FromBytes(byte[] Key)
        {
            // Get key count and threshold limits.
            int minKeys = this.MIN_KEYS;
            int maxKeys = this.MAX_KEYS;
            int minThreshold = this.MIN_THRESHOLD;

            // Get number of signers.
            int nSigners = (int)Key.Length / PublicKey.KeyLength;
            if(!(minKeys <= nSigners && nSigners <= maxKeys))
            {
                throw new Exception(string.Format("Must have between {0} and {1} keys.", minKeys, maxKeys));
            }

            // Get threshold.
            int threshold = (int)Key[-1];
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
            serializer.SerializeFixedBytes(this.ToBytes());
        }
    }
}