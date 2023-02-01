using Aptos.Accounts;
using Aptos.HdWallet;
using NUnit.Framework;

namespace Aptos.Unity.Test
{
    public class WalletTest
    {
        private const string mnemo = "stadium valid laundry unknown tuition train december camera fiber vault sniff ripple";
        private static readonly byte[] SeedNoPhrase =
        {
            125, 168, 253, 127, 208, 60, 18, 9, 
            188, 118, 79, 248, 22, 177, 237, 218, 
            150, 207, 109, 18, 216, 194, 161, 200, 
            81, 195, 154, 226, 124, 148, 120, 121, 
            218, 142, 242, 104, 202, 44, 246, 159, 
            208, 250, 42, 58, 204, 203, 89, 114, 
            96, 203, 231, 176, 7, 227, 4, 176, 
            222, 227, 185, 220, 247, 250, 223, 167
        };

        private static readonly byte[] MessageUt8Bytes = {
            87, 69, 76, 67, 79, 77, 69, 32,
            84, 79, 32, 65, 80, 84, 79, 83, 33 };
        private const string Message = "WELCOME TO APTOS!";

        private static readonly byte[] SignatureBytes =
        {
            170, 66, 187, 194, 169, 252, 117, 27,
            238, 238, 59, 49, 43, 132, 82, 196,
            69, 199, 212, 171, 134, 152, 3, 107,
            12, 249, 242, 228, 106, 9, 139, 176,
            44, 54, 159, 188, 141, 254, 253, 35,
            26, 18, 141, 138, 75, 185, 173, 207,
            228, 94, 7, 24, 139, 117, 140, 58,
            211, 152, 215, 248, 78, 130, 239, 5
        };

        [Test]
        public void CreateWallet()
        {
            Wallet wallet = new Wallet(mnemo);
            Assert.AreEqual(SeedNoPhrase, wallet.DeriveMnemonicSeed());
            Assert.IsNotNull(wallet.Account);
            Assert.IsNotNull(wallet.Account.AccountAddress);
            Assert.IsNotNull(wallet.Account.PrivateKey);
            Assert.IsNotNull(wallet.Account.PublicKey);
            Assert.IsNotNull(wallet.Account.PrivateKey.KeyBytes);
            Assert.IsNotNull(wallet.Account.PublicKey.KeyBytes);
        }

        [Test]
        public void SignMessage()
        {
            Wallet wallet = new Wallet(mnemo);
            Account acct = wallet.Account;
            byte[] signature = acct.Sign(MessageUt8Bytes);
            Assert.AreEqual(SignatureBytes, signature);
        }

        [Test]
        public void VerifySignature()
        {
            Wallet wallet = new Wallet(mnemo);
            Account acct = wallet.Account;
            bool verify = acct.Verify(MessageUt8Bytes, SignatureBytes);
            Assert.IsTrue(verify);
        }
    }
}