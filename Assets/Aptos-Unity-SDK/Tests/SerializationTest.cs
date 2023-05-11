using NUnit.Framework;
using Aptos.BCS;
using System.Numerics;
using System.Collections.Generic;
using System;
using System.Text;
using System.Linq;

namespace Aptos.Unity.Test
{
    public class SerializationTest
    {
        /// <summary>
        /// Python SDK Code:
        /// <code>
        /// in_value = True
        /// ser = Serializer()
        /// ser.bool (in_value)
        /// out = ser.output()
        /// print([x for x in ser.output()])
        /// </code>
        /// </summary>
        [Test]
        public void BoolTrueSerialize()
        {
            byte[] res = new Serialization().SerializeBool(true).GetBytes();
            Assert.AreEqual(new byte[] { 1 }, res);
        }

        [Test]
        public void BoolTrueDeserialize()
        {
            bool actual = true;
            byte[] res = new Serialization().SerializeBool(actual).GetBytes();
            bool expected = new Deserialization(res).DeserializeBool();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Python SDK Code:
        /// <code>
        /// in_value = False
        /// ser = Serializer()
        /// ser.bool (in_value)
        /// out = ser.output()
        /// print([x for x in ser.output()])
        /// </code>
        /// </summary>
        [Test]
        public void BoolFalseSerialize()
        {
            byte[] res = new Serialization().SerializeBool(false).GetBytes();
            Assert.AreEqual(new byte[] { 0 }, res);
        }

        [Test]
        public void BoolFalseDeserialize()
        {
            bool expected = false;
            byte[] res = new Serialization().SerializeBool(expected).GetBytes();
            bool actual = new Deserialization(res).DeserializeBool();
            Assert.AreEqual(expected, actual);
        }


        //[Test]
        //public void DeserializeBoolError()
        //{
        //    bool expected = false;
        //    byte[] res = new Serialization().SerializeU32(32).GetBytes();
        //    Deserialization deser = new Deserialization(res);
        //    Assert.Throws<ArgumentException>(() => deser.DeserializeBool());
        //}

        /// <summary>
        /// Python SDK Code:
        /// <code>
        /// in_value = b"1234567890"
        /// ser = Serializer()
        /// ser.to_bytes(in_value)
        /// out = ser.output()
        /// print([x for x in out])
        /// </code>
        /// </summary>
        [Test]
        public void ByteArraySerialize()
        {
            byte[] value = Encoding.UTF8.GetBytes("1234567890");
            byte[] actual = new Serialization().SerializeBytes(value).GetBytes();
            byte[] expected = { 10, 49, 50, 51, 52, 53, 54, 55, 56, 57, 48 };
            Assert.AreEqual(expected, actual, ToReadableByteArray(actual));
        }

        [Test]
        public void ByteArrayDeserialize()
        {
            byte[] value = Encoding.UTF8.GetBytes("1234567890");
            byte[] expected = new Serialization().SerializeBytes(value).GetBytes();

            byte[] actual = new Deserialization(expected).ToBytes();
            Assert.AreEqual(value.Length, actual.Length, "EXPECTED LENGHT: " + value.Length + " ACTUAL LENGHT: " + actual.Length);
            Assert.AreEqual(value, actual, ToReadableByteArray(actual));
        }

        /// <summary>
        /// Python SDK Code:
        /// <code>
        /// in_value = 123
        /// ser = Serializer()
        /// ser.u8(in_value)
        /// out = ser.output()
        /// </code>
        /// </summary>
        [Test]
        public void U8Serialize()
        {
            byte[] res = new Serialization().SerializeU8(123).GetBytes();
            Assert.AreEqual(new byte[] { 123 }, res);
        }

        [Test]
        public void U8Deserialize()
        {
            byte expected = 123;
            byte[] bytes = new Serialization().SerializeU8(expected).GetBytes(); // { 123}
            int actual = new Deserialization(bytes).DeserializeU8();
            Assert.AreEqual(expected, actual);
        }


        /// <summary>
        /// Python SDK Code:
        /// <code>
        /// in_value = 57615782
        /// ser = Serializer()
        /// ser.u32(in_value)
        /// out = ser.output()
        /// print([x for x in ser.output()])
        /// </code>
        /// </summary>
        [Test]
        public void U32Serialize()
        {
            byte[] res = new Serialization().SerializeU32(57615782).GetBytes();
            Assert.AreEqual(new byte[] { 166, 37, 111, 3 }, res);
        }

        [Test]
        public void U32Deserialize()
        {
            uint expected = 57615782;
            byte[] bytes = new Serialization().SerializeU32(expected).GetBytes();
            //Assert.AreEqual(new byte[] { 166, 37, 111, 3 }, res);
            uint actual = new Deserialization(bytes).DeserializeU32();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Python SDK Code:
        /// <code>
        /// in_value = 9432012321182
        /// ser = Serializer()
        /// ser.u64(in_value)
        /// out = ser.output()
        /// print([x for x in ser.output()])
        /// </code>
        /// </summary>
        [Test]
        public void U64Serialize()
        {
            byte[] res = new Serialization().SerializeU64(9432012321182).GetBytes();
            Assert.AreEqual(new byte[] { 158, 113, 190, 15, 148, 8, 0, 0 }, res);
        }

        [Test]
        public void U64Deserialize()
        {
            ulong expected = 9432012321182;
            byte[] bytes = new Serialization().SerializeU64(expected).GetBytes();
            ulong actual = new Deserialization(bytes).DeserializeU64();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Python SDK Code:
        /// <code>
        /// in_value = 10
        /// ser = Serializer()
        /// ser.128(in_value)
        /// out = ser.output()
        /// print([x for x in ser.output()])
        /// 
        /// in_value = 749382032131231323910498053
        /// ser = Serializer()
        /// ser.128(in_value)
        /// out = ser.output()
        /// print([x for x in ser.output()])
        /// </code>
        /// </summary>
        [Test]
        public void U128Serialize()
        {
            byte[] res = new Serialization().SerializeU128(BigInteger.Parse("10")).GetBytes();
            Assert.AreEqual(new byte[] { 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, res);

            res = new Serialization().SerializeU128(BigInteger.Parse("749382032131231323910498053")).GetBytes();
            Assert.AreEqual(new byte[] { 5, 231, 86, 201, 40, 241, 231, 92, 209, 223, 107, 2, 0, 0, 0, 0 }, res);
        }

        [Test]
        public void U128Deserialize()
        {
            BigInteger expected = BigInteger.Parse("10");
            byte[] bytes = new Serialization().SerializeU128(expected).GetBytes();
            //Assert.AreEqual(new byte[] { 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, res);
            BigInteger actual = new Deserialization(bytes).DeserializeU128();
            Assert.AreEqual(expected, actual);

            expected = BigInteger.Parse("749382032131231323910498053");
            bytes = new Serialization().SerializeU128(expected).GetBytes();
            //Assert.AreEqual(new byte[] { 5, 231, 86, 201, 40, 241, 231, 92, 209, 223, 107, 2, 0, 0, 0, 0 }, res);
            actual = new Deserialization(bytes).DeserializeU128();
            Assert.AreEqual(expected, actual);
        }


        /// <summary>
        /// Python SDK Code:
        /// <code>
        /// in_value = 1160
        /// ser = Serializer()
        /// ser.uleb128(in_value)
        /// out = ser.output()
        /// print([x for x in ser.output()])
        /// </code>
        /// </summary>
        [Test]
        public void U32AsUleb128Serialize()
        {
            byte[] res = new Serialization().SerializeU32AsUleb128(1160).GetBytes();
            Assert.AreEqual(new byte[] { 136, 9 }, res);

            //byte[] input = Encoding.UTF8.GetBytes("1234567890");
            //byte[] actual = new Serialization().SerializeBytes(input).GetBytes();
            //byte[] expected = { 4, 49, 49, 54, 48 };
            //Assert.AreEqual(expected, actual, ToReadableByteArray(actual));
        }

        [Test]
        public void U32AsUleb128Deserialize()
        {
            uint expected = 1160;
            byte[] bytes = new Serialization().SerializeU32AsUleb128(expected).GetBytes();
            //Assert.AreEqual(new byte[] { 136, 9 }, res);
            int actual = new Deserialization(bytes).DeserializeUleb128();
            Assert.AreEqual(expected, actual, ToReadableByteArray(bytes));
        }

        /// <summary>
        /// Python SDK Code:
        /// <code>
        /// in_value = "potato UTF8: 🥔"
        /// ser = Serializer()
        /// ser.str(in_value)
        /// out = ser.output()
        /// print([x for x in ser.output()])
        /// </code>
        /// </summary>
        [Test]
        public void StringSerialize()
        {
            byte[] res = new Serialization().SerializeString("potato UTF8: 🥔").GetBytes();
            byte[] exp = new byte[] { 17, 112, 111, 116, 97, 116, 111, 32, 85, 84, 70, 56, 58, 32, 240, 159, 165, 148 };
            Assert.AreEqual(exp, res);
        }

        [Test]
        public void StringDeserialize()
        {
            string expected = "potato UTF8: 🥔";
            byte[] bytes = new Serialization().SerializeString(expected).GetBytes();
            string actual = new Deserialization(bytes).DeserializeString();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Python SDK Code:
        /// <code>
        /// in_value = "potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔"
        /// ser = Serializer()
        /// ser.str(in_value)
        /// out = ser.output()
        /// print([x for x in ser.output()])
        /// </code>
        /// </summary>
        [Test]
        public void StringLongSerialize()
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
        public void StringLongDeserialize()
        {
            string expected = "potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔 potato UTF8: 🥔";
            byte[] bytes = new Serialization().SerializeString(expected).GetBytes();
            string actual = new Deserialization(bytes).DeserializeString();
            Assert.AreEqual(expected, actual, actual);
        }

        /// <summary>
        /// Python SDK Code:
        /// <code>
        /// in_value = {"a": 12345, "b": 99234, "c": 23829}
        /// ser = Serializer()
        /// ser.map(in_value, Serializer.str, Serializer.u32)
        /// output = ser.output()
        /// print([x for x in ser.output()])
        /// </code>
        /// </summary>
        [Test]
        public void Map_BString_U32_Serialize()
        {
            Dictionary<BString, ISerializable> map = new Dictionary<BString, ISerializable>();
            map.Add(new BString("a"), new U32(12345));
            map.Add(new BString("b"), new U32(99234));
            map.Add(new BString("c"), new U32(23829));

            Serialization ser = new Serialization();
            BCSMap bcsMap = new BCSMap(map);
            bcsMap.Serialize(ser);
            
            byte[] res = ser.GetBytes();

            byte[] exp = new byte[] { 3, 1, 97, 57, 48, 0, 0, 1, 98, 162, 131, 1, 0, 1, 99, 21, 93, 0, 0 };

            Assert.AreEqual(exp, res);
        }

        [Test]
        public void Map_BString_U32_Deserialize()
        {
            Dictionary<BString, ISerializable> expectedMap = new Dictionary<BString, ISerializable>();
            expectedMap.Add(new BString("x"), new U32(12345));
            expectedMap.Add(new BString("b"), new U32(99234));
            expectedMap.Add(new BString("c"), new U32(23829));

            BCSMap expectedBcsMap = new BCSMap(expectedMap);

            Serialization ser = new Serialization();
            expectedBcsMap.Serialize(ser);

            byte[] bytes = ser.GetBytes();

            Deserialization deser = new Deserialization(bytes);
            BCSMap actualBcsMap = deser.DeserializeMap(typeof(BString), typeof(U32));

            Dictionary<BString, ISerializable> res = actualBcsMap.values;
            var lines = res.Select(kvp => kvp.Key.value + ": " + kvp.Value.ToString());
            string keysStr = string.Join(Environment.NewLine, lines);

            // NOTE: After deserialization the Map / Dictionary will be sorted
            List<BString> expectedKeyList = new List<BString>() { 
                new BString("b"), 
                new BString("c"), 
                new BString("x") 
            };

            List<ISerializable> expectedValueList = new List<ISerializable>() { 
                new U32(99234), 
                new U32(23829), 
                new U32(12345) 
            };

            List<BString> actualKeyList = new List<BString>(res.Keys);
            List<ISerializable> actualValueList = new List<ISerializable>(res.Values);

            Assert.AreEqual(expectedKeyList, actualKeyList, keysStr);
            Assert.AreEqual(expectedValueList, actualValueList);
        }

        /// <summary>
        /// Python SDK Code:
        /// <code>
        /// in_value = {"x": 12345, "b": 99234, "c": 23829}
        /// ser = Serializer()
        /// ser.map(in_value, Serializer.str, Serializer.u32)
        /// output = ser.output()
        /// print([x for x in ser.output()])
        /// </code>
        /// </summary>
        [Test]
        public void Map_BString_U32_Serialize_1()
        {
            Dictionary<BString, ISerializable> map = new Dictionary<BString, ISerializable>();
            map.Add(new BString("x"), new U32(12345));
            map.Add(new BString("b"), new U32(99234));
            map.Add(new BString("c"), new U32(23829));

            Serialization ser = new Serialization();
            BCSMap bcsMap = new BCSMap(map);
            bcsMap.Serialize(ser);

            byte[] res = ser.GetBytes();

            byte[] exp = new byte[] { 3, 1, 98, 162, 131, 1, 0, 1, 99, 21, 93, 0, 0, 1, 120, 57, 48, 0, 0 };

            Assert.AreEqual(exp, res);
        }

        /// <summary>
        /// Python SDK Code:
        /// <code>
        /// in_value = {"b": 12345, "x": 99234, "c": 23829}
        /// ser = Serializer()
        /// ser.map(in_value, Serializer.str, Serializer.u32)
        /// output = ser.output()
        /// print([x for x in ser.output()])
        /// </code>
        /// </summary>
        [Test]
        public void Map_BString_U32_Serialize_2()
        {
            Dictionary<BString, ISerializable> map = new Dictionary<BString, ISerializable>();
            map.Add(new BString("b"), new U32(12345));
            map.Add(new BString("x"), new U32(99234));
            map.Add(new BString("c"), new U32(23829));

            Serialization ser = new Serialization();
            BCSMap bcsMap = new BCSMap(map);
            bcsMap.Serialize(ser);

            byte[] res = ser.GetBytes();

            byte[] exp = new byte[] { 3, 1, 98, 57, 48, 0, 0, 1, 99, 21, 93, 0, 0, 1, 120, 162, 131, 1, 0 };

            Assert.AreEqual(exp, res);
        }

        /// <summary>
        /// Python SDK Code:
        /// <code>
        /// in_value = {"b": 99234, "x": 12345, "c": 23829}
        /// ser = Serializer()
        /// ser.map(in_value, Serializer.str, Serializer.u32)
        /// output = ser.output()
        /// print([x for x in ser.output()])
        /// </code>
        /// </summary>
        [Test]
        public void Map_BString_U32_Serialize_3()
        {
            Dictionary<BString, ISerializable> map = new Dictionary<BString, ISerializable>();
            map.Add(new BString("b"), new U32(99234));
            map.Add(new BString("x"), new U32(12345));
            map.Add(new BString("c"), new U32(23829));

            Serialization ser = new Serialization();
            BCSMap bcsMap = new BCSMap(map);
            bcsMap.Serialize(ser);

            byte[] res = ser.GetBytes();

            byte[] exp = new byte[] { 3, 1, 98, 162, 131, 1, 0, 1, 99, 21, 93, 0, 0, 1, 120, 57, 48, 0, 0 };

            Assert.AreEqual(exp, res);
        }

        /// <summary>
        /// Python SDK Code:
        /// <code>
        /// in_value = {"a": "a", "b": "b", "c": "c"}
        /// ser = Serializer()
        /// ser.map(in_value, Serializer.str, Serializer.str)
        /// output = ser.output()
        /// print([x for x in ser.output()])
        /// </code>
        /// </summary>
        [Test]
        public void Map_BString_BString_Serialize()
        {
            Dictionary<BString, ISerializable> map = new Dictionary<BString, ISerializable>();
            map.Add(new BString("a"), new BString("a"));
            map.Add(new BString("b"), new BString("b"));
            map.Add(new BString("c"), new BString("c"));

            Serialization ser = new Serialization();
            BCSMap bcsMap = new BCSMap(map);
            bcsMap.Serialize(ser);

            byte[] res = ser.GetBytes();

            byte[] exp = new byte[] { 3, 1, 97, 1, 97, 1, 98, 1, 98, 1, 99, 1, 99 };

            Assert.AreEqual(exp, res);
        }

        /// <summary>
        /// Python SDK Code:
        /// <code>
        /// s = Serializer()
        /// s.to_bytes(b"\0"*1160)
        /// out = s.output()
        /// print([x for x in s.output()])
        /// </code>
        /// </summary>
        [Test]
        public void StringBytesEmptySerialize()
        {
            byte[] value = new byte[1160]; // empty byte string of size 1160

            Serialization serializer = new Serialization();
            serializer.SerializeBytes(value);
            byte[] res = serializer.GetBytes();

            // 1160 for byte array + 2 for length
            byte[] exp = new byte[1162];
            exp[0] = 136;
            exp[1] = 9;
            Assert.AreEqual(exp, res);
        }

        /// <summary>
        /// Python SDK Code:
        /// <code>
        /// ser = Serializer()
        /// ser.u32(123)
        /// ser.bool (True)
        /// ser.u32(456)
        /// output = ser.output()
        /// print([x for x in output])
        /// </code>
        /// </summary>
        [Test]
        public void MultipleValues_Serialize()
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

        /// <summary>
        /// Python SDK Code:
        /// <code>
        /// in_value = [""]
        /// ser = Serializer()
        /// ser.sequence(in_value, Serializer.str)
        /// der = Deserializer(ser.output())
        /// out_value = der.sequence(Deserializer.str)
        /// self.assertEqual(in_value, out_value)
        /// output = ser.output()
        /// print([x for x in output])
        /// </code>
        /// </summary>
        [Test]
        public void SequenceBStringEmptySerialize()
        {
            Serialization ser = new Serialization();
            BString[] strArr = { new BString("") };
            Sequence seq = new Sequence(strArr);

            seq.Serialize(ser);

            byte[] actual = ser.GetBytes();
            byte[] expected = { 1, 1, 0 };
            Assert.AreEqual(expected, actual, ToReadableByteArray(actual));
        }

        [Test]
        public void SequenceBStringLongDeserialize()
        {
            Serialization ser = new Serialization();
            BString[] expectedStrArr = { new BString("a"), new BString("abc"), new BString("def"), new BString("ghi") };
            ser.Serialize(expectedStrArr);

            Sequence expectedSeq = new Sequence(expectedStrArr);

            byte[] actual = ser.GetBytes();
            byte[] exp = new byte[] { 4, 1, 97, 3, 97, 98, 99, 3, 100, 101, 102, 3, 103, 104, 105 };
            Assert.AreEqual(exp, actual, ToReadableByteArray(actual));

            Deserialization deser = new Deserialization(actual);
            BString[] actualSequenceArr = deser.DeserializeSequence(typeof(BString)).Cast<BString>().ToArray();

            Assert.AreEqual(expectedStrArr, actualSequenceArr);
        }

        /// <summary>
        /// Python SDK Code:
        /// <code>
        /// in_value = [""]
        /// ser = Serializer()
        /// ser.sequence(in_value, Serializer.str)
        /// der = Deserializer(ser.output())
        /// out_value = der.sequence(Deserializer.str)
        /// self.assertEqual(in_value, out_value)
        /// </code>
        /// </summary>
        [Test]
        public void SequenceBStringSerialize()
        {
            BString[] inValue = { new BString("a"), new BString("abc"), new BString("def"), new BString("ghi") };
            Serialization ser = new Serialization();
            ser.Serialize(inValue);

            byte[] res = ser.GetBytes();
            byte[] exp = new byte[] { 4, 1, 97, 3, 97, 98, 99, 3, 100, 101, 102, 3, 103, 104, 105 };
            Assert.AreEqual(exp, res, ToReadableByteArray(res));
        }

        // This test is invalid.
        // To serialize sequences, use the Serializer
        //[Test]
        //public void SerializeStringSequenceTWO()
        //{
        //    Serialization ser = new Serialization();
        //    Sequence seq = new Sequence(new[] { new BString("a"), new BString("abc"), new BString("def"), new BString("ghi") });
        //    //ser.Serialize(seq);
        //    seq.Serialize(ser);
        //    byte[] res = ser.GetBytes();
        //    byte[] exp = new byte[] { 4, 1, 97, 3, 97, 98, 99, 3, 100, 101, 102, 3, 103, 104, 105 };
        //    // 4,    1, 97,    3, 97, 98, 99,    3, 100, 101, 102, 3   , 103, 104, 105
        //    // 4, 2, 1, 97, 4, 3, 97, 98, 99, 4, 3, 100, 101, 102, 4, 3, 103, 104, 105
        //    //       1, 97, 3, 97, 98, 99, 3, 100, 101, 102, 3, 103, 104, 105
        //    Assert.AreEqual(exp, res, ToReadableByteArray(res));
        //}

        /// <summary>
        /// Python SDK Code:
        /// <code>
        /// in_value = [False, True, False]
        /// ser = Serializer()
        /// seq_ser = Serializer.sequence_serializer(Serializer.bool)
        /// seq_ser(ser, in_value)
        /// der = Deserializer(ser.output())
        /// out_value = der.sequence(Deserializer.bool)
        /// self.assertEqual(in_value, out_value)
        /// </code>
        /// </summary>
        [Test]
        public void SequenceBoolSerialize()
        {
            Bool[] inValue = { new Bool(false), new Bool(true), new Bool(false) };
            Serialization ser = new Serialization();
            ser.Serialize(inValue);

            byte[] expected = new byte[] { 3, 0, 1, 0 };
            byte[] actual = ser.GetBytes();

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void SequenceBoolDeserializer()
        {
            Bool[] expectedBoolArr = { new Bool(false), new Bool(true), new Bool(false) };
            Serialization ser = new Serialization();
            ser.Serialize(expectedBoolArr);

            byte[] expectedByteArr = new byte[] { 3, 0, 1, 0 };
            byte[] actualByteArr = ser.GetBytes();

            Assert.AreEqual(expectedByteArr, actualByteArr);

            Deserialization deser = new Deserialization(actualByteArr);
            Bool[] actualBoolArr = deser.DeserializeSequence(typeof(Bool)).Cast<Bool>().ToArray();

            Assert.AreEqual(expectedBoolArr, actualBoolArr);
        }

        static public string ToReadableByteArray(byte[] bytes)
        {
            return string.Join(", ", bytes);
        }

        //public string ToHexadecimalRepresentation(byte[] bytes)
        //{
        //    StringBuilder sb = new StringBuilder(bytes.Length << 1);
        //    foreach (byte b in bytes)
        //    {
        //        sb.AppendFormat("{0:X2}", b);
        //    }
        //    return sb.ToString();
        //}
    }
}