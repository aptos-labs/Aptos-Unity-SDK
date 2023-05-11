using Aptos.Accounts;
using Aptos.BCS;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Aptos.BCS
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
        public const int ED25519 = 0;
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

        public int GetVariant()
        {
            return this.Variant;
        }

        /// <summary>
        /// Returns the Authenticator.
        /// </summary>
        /// <returns>An Authenticator object.</returns>
        public IAuthenticator GetAuthenticator()
        {
            return authenticator;
        }

        /// <summary>
        /// Verifies the signed data.
        /// </summary>
        /// <param name="data"></param>
        /// <returns>True is signature can be verified, false otherwise.</returns>
        public bool Verify(byte[] data)
        {
            return this.authenticator.Verify(data);
        }

        /// <summary>
        /// Serializes the Authenticator.
        /// </summary>
        /// <param name="serializer"></param>
        public void Serialize(Serialization serializer)
        {
            serializer.SerializeU32AsUleb128((uint)this.Variant);
            this.authenticator.Serialize(serializer);
        }

        public static ISerializable Deserialize(Deserialization deserializer)
        {
            int variant = deserializer.DeserializeUleb128();
            ISerializable authenticator;

            if (variant == Authenticator.ED25519)
                authenticator = Ed25519Authenticator.Deserialize(deserializer);
            else if (variant == Authenticator.MULTI_ED25519)
                authenticator = MultiEd25519Authenticator.Deserialize(deserializer);
            else if (variant == Authenticator.MULTI_AGENT)
                authenticator = MultiAgentAuthenticator.Deserialize(deserializer);
            else
                throw new Exception("Invalid type: " + variant);

            return new Authenticator((IAuthenticator)authenticator);
        }

        public override bool Equals(object other)
        {
            if (other is not Authenticator)
                throw new NotImplementedException();

            return (
                this.Variant == ((Authenticator)other).Variant 
                && this.authenticator.Equals(((Authenticator)other).authenticator)
            );
        }

        public override int GetHashCode() => this.authenticator.GetHashCode();

        public override string ToString()
        {
            return this.authenticator.ToString();
        }
    }

    /// <summary>
    /// ED25519 Authenticator.
    /// </summary>
    public class Ed25519Authenticator : IAuthenticator, ISerializable
    {
        private readonly PublicKey publicKey;
        private readonly Signature signature;

        public Ed25519Authenticator(PublicKey publicKey, Signature signature)
        {
            this.publicKey = publicKey;
            this.signature = signature;
        }

        /// <summary>
        /// Verifies the data with the signature.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
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
            this.signature.Serialize(serializer); // Note in Python we call serializer.struct
        }

        public static Ed25519Authenticator Deserialize(Deserialization deserializer)
        {
            PublicKey key = PublicKey.Deserialize(deserializer);
            Signature signature = Signature.Deserialize(deserializer);

            return new Ed25519Authenticator(key, signature);
        }

        public override bool Equals(object other)
        {
            if (other is not Ed25519Authenticator)
                throw new NotImplementedException();

            return 
                this.publicKey.Equals(((Ed25519Authenticator)other).publicKey) 
                && this.signature.Equals(((Ed25519Authenticator)other).signature);
        }

        public override int GetHashCode()
        {
            return publicKey.GetHashCode() + signature.GetHashCode();
        }

        public override string ToString()
        {
            return "PublicKey: " + this.publicKey + ", Signature: " + this.signature; 
        }
    }

    /// <summary>
    /// An Authenticator that uses a list of tuples of account addresses and authenticator pairs.
    /// </summary>
    public class MultiAgentAuthenticator : IAuthenticator
    {
        private Authenticator sender;
        private List<Tuple<AccountAddress, Authenticator>> secondarySigners;

        public MultiAgentAuthenticator(Authenticator sender, List<Tuple<AccountAddress, Authenticator>> secondarySigners)
        {
            this.sender = sender;
            this.secondarySigners = secondarySigners;
        }

        /// <summary>
        /// Returns the list (Sequence) of corresponding account addresses.
        /// </summary>
        /// <returns>A Sequence of account addresses</returns>
        public Sequence SecondaryAddresses()
        {
            ISerializable[] secondaryAddresses = secondarySigners.Select(signer => signer.Item1).ToArray();
            Sequence secondaryAddressesSeq = new Sequence(secondaryAddresses);

            return secondaryAddressesSeq;
        }

        /// <summary>
        /// Verifies the data with all the account addresses.
        /// </summary>
        /// <param name="data"></param>
        /// <returns>True if all accouncts can verify, false otherwise.</returns>
        public bool Verify(byte[] data)
        {
            if (!this.sender.Verify(data))
                return false;

            return secondarySigners.All(signer => signer.Item2.Verify(data));   
        }

        /// <summary>
        /// Serializes the MultiAgentAuthenticator.
        /// </summary>
        /// <param name="serializer"></param>
        public void Serialize(Serialization serializer)
        {
            AccountAddress[] secondaryAddresses = secondarySigners.Select(signer => signer.Item1).ToArray();
            Authenticator[] authenticators = secondarySigners.Select(signer => signer.Item2).ToArray();

            Sequence secondaryAddressesSeq = new Sequence(secondaryAddresses);
            Sequence authenticatorsSeq = new Sequence(authenticators);

            serializer.Serialize(this.sender);
            serializer.Serialize(secondaryAddressesSeq);
            serializer.Serialize(authenticatorsSeq);
        }

        public static MultiAgentAuthenticator Deserialize(Deserialization deserializer)
        {
            Authenticator sender = (Authenticator)Authenticator.Deserialize(deserializer);
            AccountAddress[] secondaryAddresses = deserializer.DeserializeSequence(typeof(AccountAddress)).Cast<AccountAddress>().ToArray();
            Authenticator[] secondaryAuthenticator = deserializer.DeserializeSequence(typeof(Authenticator)).Cast<Authenticator>().ToArray();

            List<Tuple<AccountAddress, Authenticator>> secondarySigners = new List<Tuple<AccountAddress, Authenticator>>();
            for(int i = 0; i < secondaryAddresses.Length; i++)
            {
                secondarySigners.Add(Tuple.Create(secondaryAddresses[i], secondaryAuthenticator[i]));
            }

            return new MultiAgentAuthenticator(sender, secondarySigners);
        }

        public override bool Equals(object other)
        {
            if (other is not MultiAgentAuthenticator)
                throw new NotImplementedException();

            AccountAddress[] secondaryAddresses = secondarySigners.Select(signer => signer.Item1).ToArray();
            Authenticator[] authenticators = secondarySigners.Select(signer => signer.Item2).ToArray();

            AccountAddress[] otherSecondaryAddresses = ((MultiAgentAuthenticator)other).secondarySigners.Select(signer => signer.Item1).ToArray();
            Authenticator[] otherAuthenticators = ((MultiAgentAuthenticator)other).secondarySigners.Select(signer => signer.Item2).ToArray();

            return (
                this.sender.Equals(((MultiAgentAuthenticator)other).sender)
                    && this.secondarySigners.Count == ((MultiAgentAuthenticator)other).secondarySigners.Count
                    && Enumerable.SequenceEqual(secondaryAddresses, otherSecondaryAddresses)
                    && Enumerable.SequenceEqual(authenticators, otherAuthenticators)
            );
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    /// <summary>
    /// MultiEd25519Authenticator
    /// </summary>
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

        public static MultiEd25519Authenticator Deserialize(Deserialization deserializer)
        {
            throw new NotImplementedException();
        }

        //public override bool Equals(object other)
        //{
        //    if (other is not MultiEd25519Authenticator)
        //        throw new NotImplementedException();

        //    return base.Equals(other);
        //}

        //public override int GetHashCode()
        //{
        //    return base.GetHashCode();
        //}
    }
}