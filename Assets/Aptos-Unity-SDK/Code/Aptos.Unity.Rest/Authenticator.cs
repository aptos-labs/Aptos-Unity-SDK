using Aptos.Accounts;
using Aptos.Utilities.BCS;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Aptos.Authenticator
{
    /// <summary>
    /// Each transaction submitted to the Aptos blockchain contains a `TransactionAuthenticator`.
    /// During transaction execution, the executor will check if every `AccountAuthenticator`'s
    /// signature on the transaction hash is well-formed and whether the sha3 hash of the
    /// `AccountAuthenticator`'s `AuthenticationKeyPreimage` matches the `AuthenticationKey` stored
    /// under the participating signer's account address.
    /// </summary>
    public interface IAuthenticator: ISerializable
    {
        public bool Verify(byte[] data);
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

    public class Ed25519Authenticator : IAuthenticator, ISerializable
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
            return publicKey.Verify(data, signature);
        }

        /// <summary>
        /// Serialize authenticator object.
        /// </summary>
        /// <param name="serializer">Serializer object</param>
        public void Serialize(Serialization serializer)
        {
            serializer.SerializeBytes(this.publicKey); // Note in Python we call serializer.struct
            //this.signature.Serialize(serializer); // Note in Python we call serializer.struct
            serializer.Serialize(signature);
        }
    }

    public class MultiAgentAuthenticator : IAuthenticator
    {
        private Authenticator sender;
        private List<Tuple<Accounts.AccountAddress, Authenticator>> secondarySigners;

        public MultiAgentAuthenticator(Authenticator sender, List<Tuple<Accounts.AccountAddress, Authenticator>> secondarySigners)
        {
            this.sender = sender;
            this.secondarySigners = secondarySigners;
        }

        public List<Accounts.AccountAddress> SecondaryAddresses()
        {
            List<Accounts.AccountAddress> secondaryAddresses = secondarySigners.Select(signer => signer.Item1).ToList();
            return secondaryAddresses;
        }

        public bool Verify(byte[] data)
        {
            //throw new System.NotImplementedException();
            if (!this.sender.Verify(data))
                return false;

            //return all([x[1].verify(data) for x in self.secondary_signers])
            return secondarySigners.All(signer => signer.Item2.Verify(data));   
        }

        public void Serialize(Serialization serializer)
        {
            //this.sender.Serialize(serializer);
            //Serialization acctAddressSerializer = new Serialization();

            //List<Accounts.AccountAddress> secondaryAddresses = secondarySigners.Select(signer => signer.Item1).ToList();
            //List<byte[]> secAddresssesBytes = secondaryAddresses.Select(adddress => {
            //    Serialization ser = new Serialization();
            //    adddress.Serialize(ser);
            //    return ser.GetBytes();
            //}).ToList();

            //Sequence secAddressesSeq = new Sequence(secAddresssesBytes.ToArray());
            //List< Authenticator> authenticators = secondarySigners.Select(signer => signer.Item2).ToList();

            Accounts.AccountAddress[] secondaryAddresses = secondarySigners.Select(signer => signer.Item1).ToArray();
            Authenticator[] authenticators = secondarySigners.Select(signer => signer.Item2).ToArray();

            serializer.Serialize(this.sender);
            serializer.Serialize(secondaryAddresses);
            serializer.Serialize(authenticators);
        }
    }

    public class MultiEd25519Authenticator : IAuthenticator
    {
        public MultiEd25519Authenticator()
        {
            throw new System.NotImplementedException();
        }

        public bool Verify(byte[] data)
        {
            throw new System.NotImplementedException();
        }

        public void Serialize(Serialization serializer)
        {
            throw new System.NotImplementedException();
        }
    }
}