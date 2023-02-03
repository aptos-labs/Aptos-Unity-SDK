using NUnit.Framework;
using Aptos.Utilities.BCS;
using System.Numerics;
using System.Collections.Generic;

namespace Aptos.Unity.Test
{
    public class SerializationTest
    {
        [Test]
        public void SerializeTrue()
        {
            byte[] res = new Serialization().SerializeBool(true).GetBytes();
            Assert.AreEqual(new byte[] { 1 }, res);
        }

        [Test]
        public void SerializeFalse()
        {
            byte[] res = new Serialization().SerializeBool(false).GetBytes();
            Assert.AreEqual(new byte[] { 0 }, res);
        }

        [Test]
        public void SerializeU8()
        {
            byte[] res = new Serialization().SerializeU8(123).GetBytes();
            Assert.AreEqual(new byte[] { 123 }, res);
        }


        [Test]
        public void SerializeU32()
        {
            byte[] res = new Serialization().SerializeU32(57615782).GetBytes();
            Assert.AreEqual(new byte[] { 166, 37, 111, 3 }, res);
        }

        [Test]
        public void SerializeU64()
        {
            byte[] res = new Serialization().SerializeU64(9432012321182).GetBytes();
            Assert.AreEqual(new byte[] { 158, 113, 190, 15, 148, 8, 0, 0 }, res);
        }

        [Test]
        public void SerializeU128()
        {
            byte[] res = new Serialization().SerializeU128(BigInteger.Parse("10")).GetBytes();
            Assert.AreEqual(new byte[] { 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, res);

            res = new Serialization().SerializeU128(BigInteger.Parse("749382032131231323910498053")).GetBytes();
            Assert.AreEqual(new byte[] { 5, 231, 86, 201, 40, 241, 231, 92, 209, 223, 107, 2, 0, 0, 0, 0 }, res);
        }

        [Test]
        public void SerializeU32AsUleb128()
        {
            byte[] res = new Serialization().SerializeU32AsUleb128(1160).GetBytes();
            Assert.AreEqual(new byte[] { 136, 9 }, res);
        }

        [Test]
        public void SerializeString()
        {
            byte[] res = new Serialization().SerializeString("potato UTF8: 🥔").GetBytes();
            byte[] exp = new byte[] { 17, 112, 111, 116, 97, 116, 111, 32, 85, 84, 70, 56, 58, 32, 240, 159, 165, 148 };
            Assert.AreEqual(exp, res);
        }

        [Test]
        public void SerializeStringLong()
        {
            byte[] res = new Serialization().SerializeString(
                "potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔"
            ).GetBytes();
            byte[] exp = new byte[]
            {
            231, 2, 112, 111, 116, 97, 116, 111, 32, 85, 84, 70,
            56, 58, 32, 240, 159, 165, 148, 32, 112, 111, 116, 97,
            116, 111, 32, 85, 84, 70, 56, 58, 32, 240, 159, 165,
            148, 32, 112, 111, 116, 97, 116, 111, 32, 85, 84, 70,
            56, 58, 32, 240, 159, 165, 148, 32, 112, 111, 116, 97,
            116, 111, 32, 85, 84, 70, 56, 58, 32, 240, 159, 165,
            148, 32, 112, 111, 116, 97, 116, 111, 32, 85, 84, 70,
            56, 58, 32, 240, 159, 165, 148, 32, 112, 111, 116, 97,
            116, 111, 32, 85, 84, 70, 56, 58, 32, 240, 159, 165,
            148, 32, 112, 111, 116, 97, 116, 111, 32, 85, 84, 70,
            56, 58, 32, 240, 159, 165, 148, 32, 112, 111, 116, 97,
            116, 111, 32, 85, 84, 70, 56, 58, 32, 240, 159, 165,
            148, 32, 112, 111, 116, 97, 116, 111, 32, 85, 84, 70,
            56, 58, 32, 240, 159, 165, 148, 32, 112, 111, 116, 97,
            116, 111, 32, 85, 84, 70, 56, 58, 32, 240, 159, 165,
            148, 32, 112, 111, 116, 97, 116, 111, 32, 85, 84, 70,
            56, 58, 32, 240, 159, 165, 148, 32, 112, 111, 116, 97,
            116, 111, 32, 85, 84, 70, 56, 58, 32, 240, 159, 165,
            148, 32, 112, 111, 116, 97, 116, 111, 32, 85, 84, 70,
            56, 58, 32, 240, 159, 165, 148, 32, 112, 111, 116, 97,
            116, 111, 32, 85, 84, 70, 56, 58, 32, 240, 159, 165,
            148, 32, 112, 111, 116, 97, 116, 111, 32, 85, 84, 70,
            56, 58, 32, 240, 159, 165, 148, 32, 112, 111, 116, 97,
            116, 111, 32, 85, 84, 70, 56, 58, 32, 240, 159, 165,
            148, 32, 112, 111, 116, 97, 116, 111, 32, 85, 84, 70,
            56, 58, 32, 240, 159, 165, 148, 32, 112, 111, 116, 97,
            116, 111, 32, 85, 84, 70, 56, 58, 32, 240, 159, 165,
            148, 32, 112, 111, 116, 97, 116, 111, 32, 85, 84, 70,
            56, 58, 32, 240, 159, 165, 148, 32, 112, 111, 116, 97,
            116, 111, 32, 85, 84, 70, 56, 58, 32, 240, 159, 165,
            148
            };
            Assert.AreEqual(exp, res);
        }

        [Test]
        public void SerializeMapOne()
        {
            Dictionary<BString, ISerializableTag> map = new Dictionary<BString, ISerializableTag>();
            map.Add(new BString("x"), new U32(12345));
            map.Add(new BString("b"), new U32(99234));
            map.Add(new BString("c"), new U32(23829));

            Serialization ser = new Serialization();
            BCSMap bcsMap = new BCSMap(map);
            bcsMap.Serialize(ser);
        }

        [Test]
        public void SerializeMapTwo()
        {
            Dictionary<BString, ISerializableTag> map = new Dictionary<BString, ISerializableTag>();
            map.Add(new BString("b"), new U32(12345));
            map.Add(new BString("x"), new U32(99234));
            map.Add(new BString("c"), new U32(23829));

            Serialization ser = new Serialization();
            BCSMap bcsMap = new BCSMap(map);
            bcsMap.Serialize(ser);
        }

        [Test]
        public void SerializeMapThree()
        {
            Dictionary<BString, ISerializableTag> map = new Dictionary<BString, ISerializableTag>();
            map.Add(new BString("b"), new U32(99234));
            map.Add(new BString("x"), new U32(12345));
            map.Add(new BString("c"), new U32(23829));

            Serialization ser = new Serialization();
            BCSMap bcsMap = new BCSMap(map);
            bcsMap.Serialize(ser);
        }

        [Test]
        public void SerializeStringBytes()
        {
            byte[] res = new Serialization().SerializeBytes(new byte[1160]).GetBytes();
            // 1160 for byte array + 2 for length
            byte[] exp = new byte[1162];
            exp[0] = 136;
            exp[1] = 9;
            Assert.AreEqual(exp, res);
        }

        [Test]
        public void SerializeMultiple()
        {
            Serialization serializer = new Serialization();
            serializer.Serialize("potato");
            serializer.Serialize((uint)123);
            serializer.Serialize(true);
            serializer.Serialize((uint)456);
            byte[] res = serializer.GetBytes();
            byte[] exp = new byte[] { 6, 112, 111, 116, 97, 116, 111, 123, 0, 0, 0, 1, 200, 1, 0, 0 };
            Assert.AreEqual(exp, res);
        }
    }
}