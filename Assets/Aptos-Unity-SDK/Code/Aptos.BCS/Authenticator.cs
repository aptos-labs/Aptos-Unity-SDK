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

    /// <summary>
    /// A generic Authenticator.
    /// </summary>
    public class Authenticator : IAuthenticator
    {
        public const int ED25519 = 0;
        public const int MULTI_ED25519 = 1;
        public const int MULTI_AGENT = 2;

        private int Variant;
        private readonly IAuthenticator authenticator;

        /// <summary>
        /// Creates an Authenticator from a given concrete authenticator.
        /// </summary>
        /// <param name="authenticator">A concrete authenticator.</param>
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

        /// <summary>
        /// Returns that type of Authenticator.
        /// </summary>
        /// <returns>An integer that represents the type of Authenticator.</returns>
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

        /// <inheritdoc/>
        public void Serialize(Serialization serializer)
        {
            serializer.SerializeU32AsUleb128((uint)this.Variant);
            this.authenticator.Serialize(serializer);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public override bool Equals(object other)
        {
            if (other is not Authenticator)
                throw new NotImplementedException();

            return (
                this.Variant == ((Authenticator)other).Variant 
                && this.authenticator.Equals(((Authenticator)other).authenticator)
            );
        }

        /// <inheritdoc/>
        public override int GetHashCode() => this.authenticator.GetHashCode();

        /// <inheritdoc/>
        public override string ToString() => this.authenticator.ToString();
    }

    /// <summary>
    /// ED25519 Authenticator.
    /// </summary>
    public class Ed25519Authenticator : IAuthenticator, ISerializable
    {
        /// <summary>
        /// The authenticators public key.
        /// </summary>
        private readonly PublicKey publicKey;

        /// <summary>
        /// The authenticator's public key.
        /// </summary>
        private readonly Signature signature;

        /// <summary>
        /// Creates an Ed25519Authenticator using a given public key and signature.
        /// </summary>
        /// <param name="publicKey"></param>
        /// <param name="signature"></param>
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

        /// <inheritdoc/>
        public void Serialize(Serialization serializer)
        {
            serializer.SerializeBytes(this.publicKey); // Note in Python we call serializer.struct
            this.signature.Serialize(serializer); // Note in Python we call serializer.struct
        }

        /// <inheritdoc/>
        public static Ed25519Authenticator Deserialize(Deserialization deserializer)
        {
            PublicKey key = PublicKey.Deserialize(deserializer);
            Signature signature = Signature.Deserialize(deserializer);

            return new Ed25519Authenticator(key, signature);
        }

        /// <inheritdoc/>
        public override bool Equals(object other)
        {
            if (other is not Ed25519Authenticator)
                throw new NotImplementedException();

            return 
                this.publicKey.Equals(((Ed25519Authenticator)other).publicKey) 
                && this.signature.Equals(((Ed25519Authenticator)other).signature);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
            => publicKey.GetHashCode() + signature.GetHashCode();

        /// <inheritdoc/>
        public override string ToString()
            => "PublicKey: " + this.publicKey + ", Signature: " + this.signature;
    }

    /// <summary>
    /// An Authenticator that uses a list of tuples of account addresses and authenticator pairs.
    /// </summary>
    public class MultiAgentAuthenticator : IAuthenticator
    {
        /// <summary>
        /// The multi agent authenticator's sender's authenticator.
        /// </summary>
        private Authenticator sender;

        /// <summary>
        /// A list of acount address to authenticator tuples.
        /// </summary>
        private List<Tuple<AccountAddress, Authenticator>> secondarySigners;

        /// <summary>
        /// Creates a MultiAgentAuthenticator object from a given sender
        /// authenticator, and a list of acount address to authenticator tuples.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="secondarySigners"></param>
        public MultiAgentAuthenticator(Authenticator sender,
            List<Tuple<AccountAddress, Authenticator>> secondarySigners)
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public override int GetHashCode() => base.GetHashCode();
    }

    /// <summary>
    /// MultiEd25519Authenticator
    /// </summary>
    public class MultiEd25519Authenticator : IAuthenticator
    {
        /// <summary>
        /// The authenticator's multi-public key.
        /// </summary>
        MultiPublicKey PublicKey;

        /// <summary>
        /// The authenticator's multi-signature.
        /// </summary>
        MultiSignature Signature;

        /// <summary>
        /// Creates a MultiEd25519Authenticator from a given multi-public key
        /// and multi-signature.
        /// </summary>
        /// <param name="publicKey"></param>
        /// <param name="signature"></param>
        public MultiEd25519Authenticator(MultiPublicKey publicKey, MultiSignature signature)
        {
            this.PublicKey = publicKey;
            this.Signature = signature;
        }

        /// <inheritdoc/>
        public bool Verify(byte[] data)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public void Serialize(Serialization serializer)
        {
            serializer.Serialize(this.PublicKey);
            serializer.Serialize(this.Signature);
        }

        /// <inheritdoc/>
        public static MultiEd25519Authenticator Deserialize(Deserialization deserializer)
        {
            throw new NotImplementedException();
        }
    }
}