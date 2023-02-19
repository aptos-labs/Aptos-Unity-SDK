using Aptos.Accounts;
using Aptos.HdWallet.Utils;
using Chaos.NaCl;
using NBitcoin;
using System;

namespace Aptos.HdWallet
{
    /// <summary>
    /// Represents an Aptos wallet.
    /// </summary>
    public class Wallet
    {
        /// <summary>
        /// The derivation path.
        /// </summary>
        private const string DerivationPath = "m/44'/637'/x'/0'/0'";
        /// <summary>
        /// The seed mode used for key generation.
        /// Aptos currently supports BIP39
        /// </summary>
        private readonly SeedMode _seedMode;

        /// <summary>
        /// The seed derived from the mnemonic and/or passphrase.
        /// </summary>
        private byte[] _seed;

        /// <summary>
        /// The method used for <see cref="SeedMode.Ed25519Bip32"/> key generation.
        /// </summary>
        private Ed25519Bip32 _ed25519Bip32;

        /// <summary>
        /// The passphrase string.
        /// </summary>
        private string Passphrase { get; }

        /// <summary>
        /// The key pair.
        /// </summary>
        public Account Account { get; private set; }

        /// <summary>
        /// The mnemonic words.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public Mnemonic Mnemonic { get; }

        /// <summary>
        /// Initialize a wallet from passed word count and word list for the mnemonic and passphrase.
        /// </summary>
        /// <param name="wordCount">The mnemonic word count.</param>
        /// <param name="wordList">The language of the mnemonic words.</param>
        /// <param name="passphrase">The passphrase.</param>
        /// <param name="seedMode">The seed generation mode.</param>
        public Wallet(WordCount wordCount, Wordlist wordList, string passphrase = "", SeedMode seedMode = SeedMode.Ed25519Bip32)
            : this(new Mnemonic(wordList, wordCount), passphrase, seedMode)
        {
        }

        /// <summary>
        /// Initialize a wallet from the passed mnemonic and passphrase.
        /// </summary>
        /// <param name="mnemonic">The mnemonic.</param>
        /// <param name="passphrase">The passphrase.</param>
        /// <param name="seedMode">The seed generation mode.</param>
        public Wallet(Mnemonic mnemonic, string passphrase = "", SeedMode seedMode = SeedMode.Ed25519Bip32)
        {
            Mnemonic = mnemonic;
            Passphrase = passphrase;

            _seedMode = seedMode;
            InitializeSeed();
        }

        /// <summary>
        /// Initialize a wallet from the passed mnemonic and passphrase.
        /// </summary>
        /// <param name="mnemonicWords">The mnemonic words.</param>
        /// <param name="wordList">The language of the mnemonic words. Defaults to <see cref="WordList.English"/>.</param>
        /// <param name="passphrase">The passphrase.</param>
        /// <param name="seedMode">The seed generation mode.</param>
        public Wallet(string mnemonicWords, Wordlist wordList = null, string passphrase = "", SeedMode seedMode = SeedMode.Ed25519Bip32)
        {
            Mnemonic = new Mnemonic(mnemonicWords, wordList ?? Wordlist.English);
            Passphrase = passphrase;

            _seedMode = seedMode;
            InitializeSeed();
        }

        /// <summary>
        /// Initializes a wallet from the passed seed byte array.
        /// </summary>
        /// <param name="seed">The seed used for key derivation.</param>
        /// <param name="passphrase">The passphrase.</param>
        /// <param name="seedMode">The seed mode.</param>
        public Wallet(byte[] seed, string passphrase = "", SeedMode seedMode = SeedMode.Ed25519Bip32)
        {
            if (seed.Length != Ed25519.ExpandedPrivateKeySizeInBytes)
                throw new ArgumentException("invalid seed length", nameof(seed));

            Passphrase = passphrase;

            _seedMode = seedMode;
            _seed = seed;
            InitializeFirstAccount();
        }

        /// <summary>
        /// Gets the account at the passed index using the ed25519 bip32 derivation path.
        /// </summary>
        /// <param name="index">The index of the account.</param>
        /// <returns>The account.</returns>
        public Account GetAccount(int index)
        {
            if (_seedMode != SeedMode.Ed25519Bip32)
                throw new Exception($"seed mode: {_seedMode} cannot derive Ed25519 based BIP32 keys");

            string path = DerivationPath.Replace("x", index.ToString());
            (byte[] account, byte[] _) = _ed25519Bip32.DerivePath(path);
            (byte[] privateKey, byte[] publicKey) = EdKeyPairFromSeed(account);
            return new Account(privateKey, publicKey);
        }

        /// <summary>
        /// Derive a seed from the passed mnemonic and/or passphrase, depending on <see cref="SeedMode"/>.
        /// </summary>
        /// <returns>The seed.</returns>
        public byte[] DeriveMnemonicSeed()
        {
            if (_seed != null) return _seed;
            return _seedMode switch
            {
                SeedMode.Ed25519Bip32 => Mnemonic.DeriveSeed(),
                SeedMode.Bip39 => Mnemonic.DeriveSeed(Passphrase),
                _ => Mnemonic.DeriveSeed()
            };
        }

        /// <summary>
        /// Initializes the first account with a key pair derived from the initialized seed.
        /// </summary>
        private void InitializeFirstAccount()
        {
            if (_seedMode == SeedMode.Ed25519Bip32)
            {
                _ed25519Bip32 = new Ed25519Bip32(_seed);
                Account = GetAccount(0);
            }
            else
            {
                (byte[] privateKey, byte[] publicKey) = EdKeyPairFromSeed(_seed.Slice(0, 32));
                Account = new Account(privateKey, publicKey);
            }
        }

        /// <summary>
        /// Derive the mnemonic seed and generate the key pair for the configured wallet.
        /// </summary>
        private void InitializeSeed()
        {
            _seed = DeriveMnemonicSeed();

            InitializeFirstAccount();
        }

        /// <summary>
        /// Gets the corresponding ed25519 key pair from the passed seed.
        /// </summary>
        /// <param name="seed">The seed</param>
        /// <returns>The key pair.</returns>
        internal static (byte[] privateKey, byte[] publicKey) EdKeyPairFromSeed(byte[] seed) =>
            (Ed25519.ExpandedPrivateKeyFromSeed(seed), Ed25519.PublicKeyFromSeed(seed));
    }
}