using NUnit.Framework;
using Aptos.HdWallet.Utils;
using System;

namespace Aptos.Unity.Test
{
    public class UtilsTest
    {
        private const string PrivateKeyHex = "0x64f57603b58af16907c18a866123286e1cbce89790613558dc1775abb3fc5c8c";
        private const string PrivateKeyHexTrimmed = "64f57603b58af16907c18a866123286e1cbce89790613558dc1775abb3fc5c8c";
        private const string InvalidPrivateKeyLengthHexOne = "0x64f57603b58af16907c18a866123286e1cbce89790613558dc1775abb3fc5c8c98ykj";
        private const string InvalidPrivateKeyLengthHexTwo = "0x64f57603b58af16907c18a866123286e1cbce89790613558dc1775abb3";
        private const string InvalidCharactersPrivateKeyHex = "0x64f57603b58af16907c18a8|3286e1cbce89790613558dc1775abb3fc5c8c";

        [Test]
        public void IsValidHexAddressTrue()
        {
            Assert.IsTrue(Utils.IsValidAddress(PrivateKeyHex));
        }

        [Test]
        public void IsValidTrimmedHexAddressTrue()
        {
            Assert.IsTrue(Utils.IsValidAddress(PrivateKeyHexTrimmed));
        }

        [Test]
        public void IsInvalidLengthHexAddress()
        {
            Assert.IsFalse(Utils.IsValidAddress(InvalidPrivateKeyLengthHexOne));

        }

        [Test]
        public void IsInvalidShorterLengthHexAddress()
        {
            Assert.IsFalse(Utils.IsValidAddress(InvalidPrivateKeyLengthHexTwo));

        }

        [Test]
        public void IsInvalidCharacterHexAddress()
        {
            Assert.IsFalse(Utils.IsValidAddress(InvalidPrivateKeyLengthHexTwo));

        }
    }
}