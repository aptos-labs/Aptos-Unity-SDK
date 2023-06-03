using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Aptos.BCS;
using UnityEngine;

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
            // TODO: Check implementation in C#
            // self.bitmap = bitmap.to_bytes(4, "big")
            this.Bitmap = BitConverter.GetBytes(bitmap);
        }

        public byte[] ToBytes()
        {
            List<byte> concatenatedSignatures = new List<byte>();
            foreach(Signature signature in this.Signatures)
            {
                concatenatedSignatures.Concat(signature.Data());
            }
            concatenatedSignatures.Concat(this.Bitmap);
            return concatenatedSignatures.ToArray();
        }

        public void Serialize(Serialization serializer)
        {
            serializer.SerializeFixedBytes(this.ToBytes());
        }
    }
}