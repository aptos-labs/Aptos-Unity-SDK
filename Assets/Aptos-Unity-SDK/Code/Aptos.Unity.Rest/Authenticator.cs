using Aptos.Accounts;
using Aptos.Utilities.BCS;
using System;
using System.Collections.Generic;

namespace Aptos.Authenticator
{
    /// <summary>
    /// Each transaction submitted to the Aptos blockchain contains a `TransactionAuthenticator`.
    /// During transaction execution, the executor will check if every `AccountAuthenticator`'s
    /// signature on the transaction hash is well-formed and whether the sha3 hash of the
    /// `AccountAuthenticator`'s `AuthenticationKeyPreimage` matches the `AuthenticationKey` stored
    /// under the participating signer's account address.
    /// </summary>
    public interface IAuthenticator
    {
        public bool Verify(byte[] data);

        public void Serialize(Serialization serializer);
    }


    public class Authenticator : IAuthenticator
    {
        public const int ED25519 = 1;
        public const int MULTI_ED25519 = 1;
        public const int MULTI_AGENT = 2;

        private int Variant;
        private readonly IAuthenticator authenticator;

        public Authenticator(IAuthenticator authenticator)
        {
            // TODO: Fix comparison
            if (authenticator.GetType() == typeof(Ed25519Authenticator))
                this.Variant = Authenticator.ED25519;
            else if (authenticator.GetType() == typeof(MultiEd25519Authenticator))
                this.Variant = Authenticator.MULTI_ED25519;
            else if (authenticator.GetType() == typeof(MultiAgentAuthenticator))
                this.Variant = Authenticator.MULTI_AGENT;
            else
                throw new ArgumentException("Invalid type");

            this.authenticator = authenticator;
        }

        public bool Verify(byte[] data)
        {
            //throw new System.NotImplementedException();
            return this.authenticator.Verify(data);
        }

        // TODO: Review Authenticator serialization implementation
        public void Serialize(Serialization serializer)
        {
            serializer.SerializeU32AsUleb128((uint)this.Variant);
            ////serializer.Serialize(authenticator.Serialize());
            //Serialization authSerializer = authenticator.Serialize(serializer);
            //byte[] output = authSerializer.GetBytes();
            //serializer.SerializeFixedBytes(output);
            //return serializer;
            authenticator.Serialize(serializer); // TODO: implement serializer.struct
        }
    }

    public class Ed25519Authenticator : IAuthenticator
    {
        private readonly PublicKey publicKey;
        private readonly Signature signature;

        public Ed25519Authenticator(PublicKey publicKey, Signature signature)
        {
            this.publicKey = publicKey;
            this.signature = signature;
        }

        public bool Verify(byte[] data)
        {
            //throw new System.NotImplementedException();
            return publicKey.Verify(data, signature);
        }

        /// <summary>
        /// Serialize authenticator object.
        /// </summary>
        /// <param name="serializer">Serializer object</param>
        public void Serialize(Serialization serializer)
        {
            serializer.SerializeBytes(this.publicKey); // Note in Python we call serializer.struct
            this.signature.Serialize(serializer); // Note in Python we call serializer.struct
        }
    }

    public class MultiAgentAuthenticator : IAuthenticator
    {
        private Authenticator sender;
        private List<Tuple<Utilities.BCS.AccountAddress, Authenticator>> seondarySigners;

        public MultiAgentAuthenticator(Authenticator sender, List<Tuple<Utilities.BCS.AccountAddress, Authenticator>> secondarySigners)
        {
            this.sender = sender;
            this.seondarySigners = secondarySigners;
        }

        public bool Verify(byte[] data)
        {
            throw new System.NotImplementedException();
        }

        public void Serialize(Serialization serializer)
        {
            //throw new System.NotImplementedException();
            //sender.Serialize(serializer);
            //serializer.SerializeFixedBytes
        }
    }

    public class MultiEd25519Authenticator : IAuthenticator
    {
        public void Serialize(Serialization serializer)
        {
            throw new System.NotImplementedException();
        }

        public bool Verify(byte[] data)
        {
            throw new System.NotImplementedException();
        }
    }
}