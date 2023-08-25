using NUnit.Framework;
using Aptos.Accounts;
using System;
using Aptos.BCS;
using Aptos.HdWallet.Utils;
using System.Collections.Generic;
using System.Text;

namespace Aptos.Unity.Test
{
    public class AccountTest
    {
        // Extended PrivateKey for reference
        private static readonly byte[] ExtendedPrivateKeyBytes = {
            100, 245, 118, 3, 181, 138, 241, 105,
            7, 193, 138, 134, 97, 35, 40, 110,
            28, 188, 232, 151, 144, 97, 53, 88,
            220, 23, 117, 171, 179, 252, 92, 140,
            88, 110, 60, 141, 68, 125, 118, 121,
            34, 46, 19, 144, 51, 227, 130, 2,
            53, 227, 61, 165, 9, 30, 155, 11,
            184, 241, 161, 18, 207, 12, 143, 245
        };

        private static readonly byte[] PrivateKeyBytes = {
            100, 245, 118, 3, 181, 138, 241, 105,
            7, 193, 138, 134, 97, 35, 40, 110,
            28, 188, 232, 151, 144, 97, 53, 88,
            220, 23, 117, 171, 179, 252, 92, 140
        };

        private const string PrivateKeyHex = "0x64f57603b58af16907c18a866123286e1cbce89790613558dc1775abb3fc5c8c";

        private static readonly byte[] PrivateKeySerializedOutput =
        {
            32, 100, 245, 118, 3, 181, 138, 241,
            105, 7, 193, 138, 134, 97, 35, 40,
            110, 28, 188, 232, 151, 144, 97, 53,
            88, 220, 23, 117, 171, 179, 252, 92, 140
        };

        private static readonly byte[] PublicKeyBytes = {
            88, 110, 60, 141, 68, 125, 118, 121,
            34, 46, 19, 144, 51, 227, 130, 2,
            53, 227, 61, 165, 9, 30, 155, 11,
            184, 241, 161, 18, 207, 12, 143, 245
        };
        private const string PublicKeyHex = "0x586e3c8d447d7679222e139033e3820235e33da5091e9b0bb8f1a112cf0c8ff5";

        private const string AccountAddress = "0x9f628c43d1c1c0f54683cf5ccbd2b944608df4ff2649841053b1790a4d7c187d";

        private static readonly byte[] PublicKeySerializedOutput = {
            32, 88, 110, 60, 141, 68, 125, 118,
            121, 34, 46, 19, 144, 51, 227, 130,
            2, 53, 227, 61, 165, 9, 30, 155,
            11, 184, 241, 161, 18, 207, 12, 143, 245
        };

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

        Signature signatureObject = new Signature(SignatureBytes);

        private const string SignatureHex = "0xaa42bbc2a9fc751beeee3b312b8452c445c7d4ab8698036b0cf9f2e46a098bb02c369fbc8dfefd231a128d8a4bb9adcfe45e07188b758c3ad398d7f84e82ef05";

        private static readonly byte[] SignatureSerializedOutput ={
            64, 170, 66, 187, 194, 169, 252, 117,
            27, 238, 238, 59, 49, 43, 132, 82,
            196, 69, 199, 212, 171, 134, 152, 3,
            107, 12, 249, 242, 228, 106, 9, 139,
            176, 44, 54, 159, 188, 141, 254, 253,
            35, 26, 18, 141, 138, 75, 185, 173,
            207, 228, 94, 7, 24, 139, 117, 140,
            58, 211, 152, 215, 248, 78, 130, 239, 5 };

        [Test]
        public void GeneratePrivateKeysWithBytesSuccess()
        {
            PrivateKey privateKey = new PrivateKey(PrivateKeyBytes);
            Assert.IsNotNull(privateKey.KeyBytes, "PrivateKey.KeyBytes != null");
            Assert.AreEqual(32, privateKey.KeyBytes.Length);
            Assert.AreEqual(privateKey.KeyBytes, PrivateKeyBytes);

            string privateKeyHex = privateKey.Key.ToString();
            Assert.AreEqual(privateKeyHex, PrivateKeyHex);
        }

        [Test]
        public void GenerateKeysWithBytesSuccess()
        {
            PrivateKey privateKey = new PrivateKey(PrivateKeyBytes);
            PublicKey publicKey = new PublicKey(PublicKeyBytes);

            Assert.IsNotNull(privateKey.KeyBytes, "PrivateKey.KeyBytes != null");
            Assert.IsNotNull(publicKey.KeyBytes, "PublicKey.KeyBytes != null");

            string privateKeyHex = privateKey.ToString();
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
        public void GeneratePublicKeyFromPrivateKeySuccess()
        {
            PrivateKey privateKey = new PrivateKey(PrivateKeyHex);
            PublicKey publicKey = privateKey.PublicKey();

            Assert.AreEqual(PublicKeyBytes, publicKey.KeyBytes);
        }

        [Test]
        public void PrivateKeyFromHexSignSuccess()
        {
            PrivateKey privateKey = new PrivateKey(PrivateKeyHex);
            Assert.AreEqual(privateKey.Key, PrivateKeyHex);
            Assert.AreEqual(privateKey.KeyBytes, PrivateKeyBytes);

            Signature signature = privateKey.Sign(MessageUtf8Bytes);
            Assert.AreEqual(signature, signatureObject);
        }

        [Test]
        public void PrivateKeyFromBytesSignSuccess()
        {
            PrivateKey privateKey = new PrivateKey(PrivateKeyBytes);
            Assert.AreEqual(privateKey.Key, PrivateKeyHex);
            Assert.AreEqual(privateKey.KeyBytes, PrivateKeyBytes);

            Signature signature = privateKey.Sign(MessageUtf8Bytes);
            Assert.AreEqual(signature, signatureObject);
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
        public void PublicKeySerialization()
        {
            Serialization serializer = new Serialization();
            PublicKey publicKey = new PublicKey(PublicKeyBytes);
            publicKey.Serialize(serializer);
            byte[] output = serializer.GetBytes();

            Assert.AreEqual(output, PublicKeySerializedOutput);
        }

        [Test]
        public void PublicKeyDeserialization()
        {
            Serialization serializer = new Serialization();
            PublicKey publicKey = new PublicKey(PublicKeyBytes);
            publicKey.Serialize(serializer);
            byte[] output = serializer.GetBytes();

            Assert.AreEqual(output, PublicKeySerializedOutput);

            Deserialization deserializer = new Deserialization(output);
            PublicKey actualDeserialized = PublicKey.Deserialize(deserializer);

            Assert.AreEqual(publicKey, actualDeserialized);
        }

        [Test]
        public void PrivateKeySerialization()
        {
            Serialization serializer = new Serialization();
            PrivateKey privateKey = new PrivateKey(PrivateKeyBytes);
            privateKey.Serialize(serializer);
            byte[] output = serializer.GetBytes();

            Assert.AreEqual(output, PrivateKeySerializedOutput);
        }

        [Test]
        public void SignatureEquality()
        {
            Signature sigOne = new Signature(SignatureBytes);
            Signature sigTwo = new Signature(SignatureBytes);
            Assert.IsTrue(sigOne.Equals(sigTwo));
        }

        [Test]
        public void SignatureSerialization()
        {
            Serialization serializer = new Serialization();
            Signature sig = new Signature(SignatureBytes);
            sig.Serialize(serializer);
            byte[] output = serializer.GetBytes();
            Assert.AreEqual(output, SignatureSerializedOutput);
        }

        [Test]
        public void SignatureDeserialization()
        {
            Serialization serializer = new Serialization();
            Signature sig = new Signature(SignatureBytes);
            sig.Serialize(serializer);
            byte[] output = serializer.GetBytes();

            Deserialization deser = new Deserialization(output);
            Signature actualSig = Signature.Deserialize(deser);
            Assert.AreEqual(sig, actualSig);
        }

        [Test]
        public void GenerateAccountAddressFromPublicKey()
        {
            PublicKey publicKey = new PublicKey(PublicKeyBytes);
            Accounts.AccountAddress accountAddress = Accounts.AccountAddress.FromKey(publicKey);
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
        public void GenerateAccountFromPrivateKeyStringSuccess()
        {
            Account acc = Account.LoadKey(PrivateKeyHex);
            PrivateKey privateKey = acc.PrivateKey;
            PublicKey publicKey = acc.PublicKey;

            PublicKey expectedPublicKey = new PublicKey(PublicKeyBytes);
            PrivateKey expectedPrivateKey = new PrivateKey(PrivateKeyBytes);

            Assert.IsTrue(privateKey.Key == expectedPrivateKey.Key);
            Assert.IsTrue(publicKey.Key == expectedPublicKey.Key);

            Assert.AreEqual(AccountAddress, acc.AccountAddress.ToString());
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
            Signature signature = acc.Sign(MessageUtf8Bytes);
            Assert.AreEqual(signature, signatureObject);
        }

        [Test]
        public void AccountSignVerify()
        {
            Account acc = new Account(PrivateKeyBytes, PublicKeyBytes);
            Signature signature = acc.Sign(MessageUtf8Bytes);
            Assert.AreEqual(signature, signatureObject);
            bool verify = acc.Verify(MessageUtf8Bytes, signature);
            Assert.IsTrue(verify);
        }

        [Test]
        public void TestRotationProofChallenge()
        {
            Account OriginatingAccount = Account.LoadKey(
                "005120c5882b0d492b3d2dc60a8a4510ec2051825413878453137305ba2d644b"
            );

            Account TargetAccount = Account.LoadKey(
                "19d409c191b1787d5b832d780316b83f6ee219677fafbd4c0f69fee12fdcdcee"
            );

            RotationProofChallenge rotationProofChallenge = new RotationProofChallenge(
                1234,
                OriginatingAccount.AccountAddress,
                OriginatingAccount.AccountAddress,
                TargetAccount.PublicKey.KeyBytes
            );

            Serialization serializer = new Serialization();
            rotationProofChallenge.Serialize(serializer);
            string rotationProofChallengeBcs = serializer.GetBytes().HexString();

            string expectedBytes = string.Format("{0}{1}{2}{3}{4}{5}",
                "0000000000000000000000000000000000000000000000000000000000000001",
                "076163636f756e7416526f746174696f6e50726f6f664368616c6c656e6765d2",
                "0400000000000015b67a673979c7c5dfc8d9c9f94d02da35062a19dd9d218087",
                "bd9076589219c615b67a673979c7c5dfc8d9c9f94d02da35062a19dd9d218087",
                "bd9076589219c620a1f942a3c46e2a4cd9552c0f95d529f8e3b60bcd44408637",
                "9ace35e4458b9f22");

            Assert.AreEqual(expectedBytes, rotationProofChallengeBcs, rotationProofChallengeBcs);
        }

        [Test]
        public void TestMultisig()
        {
            // Generate signatory private keys.
            PrivateKey privateKey1 = PrivateKey.FromHex(
                "4e5e3be60f4bbd5e98d086d932f3ce779ff4b58da99bf9e5241ae1212a29e5fe"
            );
            PrivateKey privateKey2 = PrivateKey.FromHex(
                "1e70e49b78f976644e2c51754a2f049d3ff041869c669523ba95b172c7329901"
            );

            // Generate multisig public key with threshold of 1.
            List<PublicKey> publicKeys = new List<PublicKey>();
            publicKeys.Add(privateKey1.PublicKey());
            publicKeys.Add(privateKey2.PublicKey());
            MultiPublicKey multiSigPublicKey = new MultiPublicKey(
                publicKeys, 1
            );

            // Get public key BCS representation.
            Serialization serializer = new Serialization();
            multiSigPublicKey.Serialize(serializer);
            string publicKeyBcs = serializer.GetBytes().HexString();
            // Check against expected BCS representation.
            string expectedPublicKeyBcs = string.Format("{0}{1}",
                "41754bb6a4720a658bdd5f532995955db0971ad3519acbde2f1149c3857348006c",
                "1634cd4607073f2be4a6f2aadc2b866ddb117398a675f2096ed906b20e0bf2c901"
            );

            Assert.AreEqual(expectedPublicKeyBcs, publicKeyBcs,
                publicKeyBcs
                + "\n" +
                privateKey1.PublicKey().KeyBytes.HexString()
                + "\n" +
                privateKey2.PublicKey().KeyBytes.HexString()
                + "\n" +
                multiSigPublicKey.Threshold
                + "\n" +
                ToReadableByteArray(BitConverter.GetBytes(multiSigPublicKey.Threshold))
            );

            // Get public key bytes representation.
            byte[] publicKeyBytes = multiSigPublicKey.ToBytes();

            // Convert back to multisig class instance from bytes.
            MultiPublicKey multisigPublicKey = MultiPublicKey.FromBytes(publicKeyBytes);

            // Get public key BCS representation.
            serializer = new Serialization();
            multiSigPublicKey.Serialize(serializer);
            string publicKeyBCs = serializer.GetBytes().HexString();

            // Assert BCS representation is the same.
            Assert.AreEqual(publicKeyBcs, expectedPublicKeyBcs);

            // Have one signer sign arbitrary message.
            Signature signature = privateKey2.Sign(Encoding.UTF8.GetBytes("multisig"));

            // Compose multisig signature.
            List<Tuple<PublicKey, Signature>> signMap = new List<Tuple<PublicKey, Signature>>()
            {
                Tuple.Create<PublicKey, Signature>(privateKey2.PublicKey(), signature)
            };
            MultiSignature multiSignature = new MultiSignature(multisigPublicKey, signMap);

            // Get signature BCS representation.
            serializer = new Serialization();
            multiSignature.Serialize(serializer);
            byte[] multisigBcsBytes = serializer.GetBytes();
            string multisigSignatureBcs = serializer.GetBytes().HexString();

            // Check against expected BCS representation.
            string expectedMultisigSignatureBcs = string.Format("{0}{1}{2}",
                "4402e90d8f300d79963cb7159ffa6f620f5bba4af5d32a7176bfb5480b43897cf",
                "4886bbb4042182f4647c9b04f02dbf989966f0facceec52d22bdcc7ce631bfc0c",
                "40000000"
            );

            Assert.AreEqual(
                multisigSignatureBcs,
                expectedMultisigSignatureBcs,
                "BCS HEX: " + multisigSignatureBcs + "\n" + multisigBcsBytes.Length);
        }

        [Test]
        public void TestMultiEd25519()
        {
            PrivateKey privateKey1 = PrivateKey.FromHex(
                "4e5e3be60f4bbd5e98d086d932f3ce779ff4b58da99bf9e5241ae1212a29e5fe"
            );

            PrivateKey privateKey2 = PrivateKey.FromHex(
                "1e70e49b78f976644e2c51754a2f049d3ff041869c669523ba95b172c7329901"
            );

            MultiPublicKey multiSigPublicKey = new MultiPublicKey(
                new List<PublicKey>() { privateKey1.PublicKey(), privateKey2.PublicKey() }, 1
            );

            AccountAddress expected = Accounts.AccountAddress.FromHex(
                "835bb8c5ee481062946b18bbb3b42a40b998d6bf5316ca63834c959dc739acf0"
            );

            AccountAddress actual = Accounts.AccountAddress.FromMultiEd25519(multiSigPublicKey);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestResourceAccount()
        {
            AccountAddress baseAddress = Accounts.AccountAddress.FromHex("b0b");
            AccountAddress expected = Accounts.AccountAddress.FromHex(
                "ee89f8c763c27f9d942d496c1a0dcf32d5eacfe78416f9486b8db66155b163b0"
            );

            byte[] seed = { 0x0b, 0x00, 0x0b };
            AccountAddress actual = Accounts.AccountAddress.ForResourceAccount(baseAddress, seed);
            Assert.AreEqual(actual, expected);
        }

        // TODO: Inquire on missing test for GUID Object

        [Test]
        public void TestNamedObject()
        {
            AccountAddress baseAddress = Accounts.AccountAddress.FromHex("b0b");
            AccountAddress expected = Accounts.AccountAddress.FromHex(
                "f417184602a828a3819edf5e36285ebef5e4db1ba36270be580d6fd2d7bcc321"
            );

            byte[] seed = Encoding.ASCII.GetBytes("bob's collection");
            AccountAddress actual = Accounts.AccountAddress.ForNamedObject(baseAddress, seed);
            Assert.AreEqual(actual, expected);
        }

        static public string ToReadableByteArray(byte[] bytes)
        {
            return string.Join(", ", bytes);
        }

        [Test]
        public void TestCollection()
        {
            AccountAddress baseAddress = Accounts.AccountAddress.FromHex("b0b");
            AccountAddress expected = Accounts.AccountAddress.FromHex(
                "f417184602a828a3819edf5e36285ebef5e4db1ba36270be580d6fd2d7bcc321"
            );
            AccountAddress actual = Accounts.AccountAddress.ForNamedCollection(baseAddress, "bob's collection");
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public void TestToken()
        {
            AccountAddress baseAddress = Accounts.AccountAddress.FromHex("b0b");
            AccountAddress expected = Accounts.AccountAddress.FromHex(
                "e20d1f22a5400ba7be0f515b7cbd00edc42dbcc31acc01e31128b2b5ddb3c56e"
            );
            AccountAddress actual = Accounts.AccountAddress.ForNamedToken(
                baseAddress, "bob's collection", "bob's token"
            );
            Assert.AreEqual(actual, expected);
        }

        [Test]
        public void TestToStandardString()
        {
            // Test special address: 0x0
            Assert.AreEqual(
                Accounts.AccountAddress.FromHex("0x0000000000000000000000000000000000000000000000000000000000000000").ToString(),
                "0x0"
            );

            // Test special address: 0x1
            Assert.AreEqual(
                Accounts.AccountAddress.FromHex("0x0000000000000000000000000000000000000000000000000000000000000001").ToString(),
                "0x1"
            );

            // Test special address: 0x4
            Assert.AreEqual(
                Accounts.AccountAddress.FromHex("0x0000000000000000000000000000000000000000000000000000000000000004").ToString(),
                "0x4"
            );

            // Test special address: 0xF
            Assert.AreEqual(
                Accounts.AccountAddress.FromHex("0x000000000000000000000000000000000000000000000000000000000000000f").ToString(),
                "0xf"
            );

            // Test special address from short no 0x: d
            Assert.AreEqual(
                Accounts.AccountAddress.FromHex("d").ToString(),
                "0xd"
            );

            // Test non-special address from long:
            // 0x0000000000000000000000000000000000000000000000000000000000000010
            Assert.AreEqual(
                Accounts.AccountAddress.FromHex("0x0000000000000000000000000000000000000000000000000000000000000010").ToString(),
                "0x0000000000000000000000000000000000000000000000000000000000000010"
            );

            // Test non-special address from long:
            // 0x000000000000000000000000000000000000000000000000000000000000001f
            Assert.AreEqual(
                Accounts.AccountAddress.FromHex("0x000000000000000000000000000000000000000000000000000000000000001f").ToString(),
                "0x000000000000000000000000000000000000000000000000000000000000001f"
            );

            // Test non-special address from long:
            // 0x00000000000000000000000000000000000000000000000000000000000000a0
            Assert.AreEqual(
                Accounts.AccountAddress.FromHex("0x00000000000000000000000000000000000000000000000000000000000000a0").ToString(),
                "0x00000000000000000000000000000000000000000000000000000000000000a0"
            );

            // Test non-special address from long no 0x:
            // ca843279e3427144cead5e4d5999a3d0ca843279e3427144cead5e4d5999a3d0
            Assert.AreEqual(
                Accounts.AccountAddress.FromHex("ca843279e3427144cead5e4d5999a3d0ca843279e3427144cead5e4d5999a3d0").ToString(),
                "0xca843279e3427144cead5e4d5999a3d0ca843279e3427144cead5e4d5999a3d0"
            );

            // Test non-special address from long no 0x:
            // 1000000000000000000000000000000000000000000000000000000000000000
            Assert.AreEqual(
                Accounts.AccountAddress.FromHex("1000000000000000000000000000000000000000000000000000000000000000").ToString(),
                "0x1000000000000000000000000000000000000000000000000000000000000000"
            );

            // Demonstrate that neither leading nor trailing zeroes get trimmed for
            // non-special addresses:
            // 0f00000000000000000000000000000000000000000000000000000000000000
            Assert.AreEqual(
                Accounts.AccountAddress.FromHex("0f00000000000000000000000000000000000000000000000000000000000000").ToString(),
                "0x0f00000000000000000000000000000000000000000000000000000000000000"
            );
        }
    }
}