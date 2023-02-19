using NUnit.Framework;
using Aptos.Accounts;
using System;

namespace Aptos.Unity.Test
{
    public class AccountTest
    { 
        private static readonly byte[] PrivateKeyBytes = {
            100, 245, 118, 3, 181, 138, 241, 105,
            7, 193, 138, 134, 97, 35, 40, 110,
            28, 188, 232, 151, 144, 97, 53, 88,
            220, 23, 117, 171, 179, 252, 92, 140,
            88, 110, 60, 141, 68, 125, 118, 121,
            34, 46, 19, 144, 51, 227, 130, 2,
            53, 227, 61, 165, 9, 30, 155, 11,
            184, 241, 161, 18, 207, 12, 143, 245
        };
        private const string PrivateKeyHex = "0x64f57603b58af16907c18a866123286e1cbce89790613558dc1775abb3fc5c8c";

        private static readonly byte[] PublicKeyBytes = {
            88, 110, 60, 141, 68, 125, 118, 121, 
            34, 46, 19, 144, 51, 227, 130, 2, 
            53, 227, 61, 165, 9, 30, 155, 11, 
            184, 241, 161, 18, 207, 12, 143, 245
        };
        private const string PublicKeyHex = "0x586e3c8d447d7679222e139033e3820235e33da5091e9b0bb8f1a112cf0c8ff5";

        private static readonly byte[] PrivateKeyBytesInvalid = {
            100, 245, 118, 3, 181, 138, 241, 105,
            7, 193, 138, 134, 97, 35, 40, 110,
            28, 188, 232, 151, 144, 97, 53, 88,
            220, 23, 117, 171, 179, 252, 92, 140,
            88, 110, 60, 141, 68, 125, 118, 121,
            34, 46, 19, 144, 51, 227, 130, 2,
            53, 227, 61, 165, 9, 30, 155, 11,
        };

        private static readonly byte[] PublicKeyBytesInvalid = {
            88, 110, 60, 141, 68, 125, 118, 121,
            34, 46, 19, 144, 51, 227, 130, 2,
            53, 227, 61, 165, 9, 30, 155, 11,
            184, 241, 161, 18, 207, 12, 143, 245
        };

        private const string AccountAddressHex = "0x9f628c43d1c1c0f54683cf5ccbd2b944608df4ff2649841053b1790a4d7c187d";
        private const string AccountAuthKeyHex = "0x9f628c43d1c1c0f54683cf5ccbd2b944608df4ff2649841053b1790a4d7c187d";

        private static readonly byte[] MessageUtf8Bytes = {
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
        public void GenerateKeysWithBytesSuccess()
        {
            PrivateKey privateKey = new PrivateKey(PrivateKeyBytes);
            PublicKey publicKey = new PublicKey(PublicKeyBytes);

            Assert.IsNotNull(privateKey.KeyBytes, "PrivateKey.KeyBytes != null");
            Assert.IsNotNull(publicKey.KeyBytes, "PublicKey.KeyBytes != null");

            string privateKeyHex = privateKey.Key.ToString();
            string publicKeyHex = publicKey.ToString();

            Assert.AreEqual(privateKeyHex, PrivateKeyHex);
            Assert.AreEqual(publicKeyHex, PublicKeyHex);
        }

        [Test]
        public void GenerateKeysWithStringSuccess()
        {
            PrivateKey privateKey = new PrivateKey(PrivateKeyHex);
            PublicKey publicKey = new PublicKey(PublicKeyHex);

            Assert.IsNotNull(privateKey.KeyBytes, "PrivateKey.KeyBytes != null");
            Assert.IsNotNull(publicKey.KeyBytes, "PublicKey.KeyBytes != null");

            Assert.AreEqual(privateKey.KeyBytes, PrivateKeyBytes);
            Assert.AreEqual(publicKey.KeyBytes, PublicKeyBytes);

            string privateKeyHex = privateKey.Key;
            string publicKeyHex = publicKey;

            Assert.AreEqual(privateKeyHex, PrivateKeyHex);
            Assert.AreEqual(publicKeyHex, PublicKeyHex);
        }

        [Test]
        public void PrivateKeyFromHexSignSuccess()
        {
            PrivateKey privateKey = new PrivateKey(PrivateKeyHex);
            Assert.AreEqual(privateKey.Key, PrivateKeyHex);
            Assert.AreEqual(privateKey.KeyBytes, PrivateKeyBytes);

            byte[] signature = privateKey.Sign(MessageUtf8Bytes);
            Assert.AreEqual(signature, SignatureBytes);
        }

        [Test]
        public void PrivateKeyFromBytesSignSuccess()
        {
            PrivateKey privateKey = new PrivateKey(PrivateKeyBytes);
            Assert.AreEqual(privateKey.Key, PrivateKeyHex);
            Assert.AreEqual(privateKey.KeyBytes, PrivateKeyBytes);

            byte[] signature = privateKey.Sign(MessageUtf8Bytes);
            Assert.AreEqual(signature, SignatureBytes);
        }

        [Test]
        public void InvalidKeyGeneration()
        {
            Assert.Throws<ArgumentException>(delegate ()
            {
                PrivateKey privateKey = new PrivateKey(PrivateKeyBytesInvalid);
                PublicKey publicKey = new PublicKey(PublicKeyBytesInvalid);
            });
        }

        [Test]
        public void GenerateAccountAddressFromPublicKey()
        {
            PublicKey publicKey = new PublicKey(PublicKeyBytes);
            AccountAddress accountAddress = AccountAddress.FromKey(publicKey);
            Assert.AreEqual(accountAddress.ToString(), AccountAddressHex);
        }

        [Test]
        public void CreateAccountFromKeys()
        {
            Account acc = new Account(PrivateKeyBytes, PublicKeyBytes);
            PrivateKey privateKey = acc.PrivateKey;
            PublicKey publicKey = acc.PublicKey;

            PublicKey validPublicKey = new PublicKey(PublicKeyBytes);
            PrivateKey validPrivateKey = new PrivateKey(PrivateKeyBytes);

            Assert.IsTrue(privateKey.Key == validPrivateKey.Key);
            Assert.IsTrue(publicKey.Key == validPublicKey.Key);
        }

        [Test]
        public void CreateDefaultAccountSuccess()
        {
            Account acc = new Account();
            Assert.IsNotNull(acc.PrivateKey, "Account generated PrivateKey");
            Assert.IsNotNull(acc.PublicKey, "Account generated PublicKey");
            Assert.IsNotNull(acc.AccountAddress, "Account generated Account Address");
        }

        [Test]
        public void InvalidAccountCreationWithShorterKeys()
        {
            Assert.Throws<ArgumentException>(delegate ()
            {
                Account acc = new Account(PrivateKeyBytesInvalid, PublicKeyBytesInvalid);
            });
        }

        [Test]
        public void AuthKeyGeneration()
        {
            Account acc = new Account(PrivateKeyBytes, PublicKeyBytes);
            string authKey = acc.AuthKey();
            Assert.AreEqual(authKey, AccountAuthKeyHex);
        }

        [Test]
        public void AccountSignSuccess()
        {
            Account acc = new Account(PrivateKeyBytes, PublicKeyBytes);
            byte[] signature = acc.Sign(MessageUtf8Bytes);
            Assert.AreEqual(signature, SignatureBytes);
        }

        [Test]
        public void AccountSignVerify()
        {
            Account acc = new Account(PrivateKeyBytes, PublicKeyBytes);
            byte[] signature = acc.Sign(MessageUtf8Bytes);
            Assert.AreEqual(signature, SignatureBytes);
            bool verify = acc.Verify(MessageUtf8Bytes, signature);
            Assert.IsTrue(verify);
        }
    }
}