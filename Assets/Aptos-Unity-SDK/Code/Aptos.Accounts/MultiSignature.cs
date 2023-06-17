using System;
using System.Collections.Generic;
using System.Linq;
using Aptos.BCS;
using Aptos.HdWallet.Utils;

namespace Aptos.Accounts
{
    public class MultiSignature : ISerializable
    {
        List<Signature> Signatures;
        byte[] Bitmap;

        public MultiSignature(
            MultiPublicKey PublicKeyMulti,
            List<Tuple<PublicKey, Signature>> SignatureMap
        )
        {
            this.Signatures = new List<Signature>();
            int bitmap = 0;
            foreach(Tuple<PublicKey, Signature> entry in SignatureMap)
            {
                this.Signatures.Add(entry.Item2);
                int index = PublicKeyMulti.Keys.IndexOf(entry.Item1);
                int shift = 31 - index; // 32 bit positions, left to right.
                bitmap = bitmap | (1 << shift);
            }

            // 4-byte big endian bitmap.
            // self.bitmap = bitmap.to_bytes(4, "big")
            uint uBitmap = ((uint)bitmap).ToBigEndian();
            this.Bitmap = BitConverter.GetBytes(uBitmap);
        }

        public byte[] ToBytes()
        {
            List<byte> concatenatedSignatures = new List<byte>();
            foreach(Signature signature in this.Signatures)
            {
                concatenatedSignatures
                    = concatenatedSignatures.Concat(signature.Data()).ToList();
            }
            concatenatedSignatures
                = concatenatedSignatures.Concat(this.Bitmap).ToList();
            return concatenatedSignatures.ToArray();
        }

        public void Serialize(Serialization serializer)
        {
            serializer.SerializeBytes(this.ToBytes());
        }
    }
}