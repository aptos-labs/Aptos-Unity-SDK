using NUnit.Framework;
using Aptos.Utilities.BCS;
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
        public void SerializeTrue()
        {
            byte[] res = new Serialization().SerializeBool(true).GetBytes();
            Assert.AreEqual(new byte[] { 1 }, res);
        }

        [Test]
        public void DeserializeBoolTrue()
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
        public void SerializeFalse()
        {
            byte[] res = new Serialization().SerializeBool(false).GetBytes();
            Assert.AreEqual(new byte[] { 0 }, res);
        }

        [Test]
        public void DeserializeBoolFalse()
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
        public void SerializeBytes()
        {
            byte[] value = Encoding.UTF8.GetBytes("1234567890");
            byte[] actual = new Serialization().SerializeBytes(value).GetBytes();
            byte[] expected = { 10, 49, 50, 51, 52, 53, 54, 55, 56, 57, 48 };
            Assert.AreEqual(expected, actual, ToReadableByteArray(actual));
        }

        [Test]
        public void DeserializeBytes()
        {
            byte[] value = Encoding.UTF8.GetBytes("1234567890");
            byte[] expected = new Serialization().SerializeBytes(value).GetBytes();

            byte[] actual = new Deserialization(expected).ToBytes();
            Assert.AreEqual(expected, actual, ToReadableByteArray(actual));
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
        public void SerializeU8()
        {
            byte[] res = new Serialization().SerializeU8(123).GetBytes();
            Assert.AreEqual(new byte[] { 123 }, res);
        }

        [Test]
        public void DeserializeU8()
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
        public void SerializeU32()
        {
            byte[] res = new Serialization().SerializeU32(57615782).GetBytes();
            Assert.AreEqual(new byte[] { 166, 37, 111, 3 }, res);
        }

        [Test]
        public void DeserializeU32()
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
        public void SerializeU64()
        {
            byte[] res = new Serialization().SerializeU64(9432012321182).GetBytes();
            Assert.AreEqual(new byte[] { 158, 113, 190, 15, 148, 8, 0, 0 }, res);
        }

        [Test]
        public void DeserializeU64()
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
        public void SerializeU128()
        {
            byte[] res = new Serialization().SerializeU128(BigInteger.Parse("10")).GetBytes();
            Assert.AreEqual(new byte[] { 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, res);

            res = new Serialization().SerializeU128(BigInteger.Parse("749382032131231323910498053")).GetBytes();
            Assert.AreEqual(new byte[] { 5, 231, 86, 201, 40, 241, 231, 92, 209, 223, 107, 2, 0, 0, 0, 0 }, res);
        }

        [Test]
        public void DeserializeU128()
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
        public void SerializeU32AsUleb128()
        {
            byte[] res = new Serialization().SerializeU32AsUleb128(1160).GetBytes();
            Assert.AreEqual(new byte[] { 136, 9 }, res);

            //byte[] input = Encoding.UTF8.GetBytes("1234567890");
            //byte[] actual = new Serialization().SerializeBytes(input).GetBytes();
            //byte[] expected = { 4, 49, 49, 54, 48 };
            //Assert.AreEqual(expected, actual, ToReadableByteArray(actual));
        }

        [Test]
        public void DeserializeU32AsUleb128()
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
        public void SerializeString()
        {
            byte[] res = new Serialization().SerializeString("potato UTF8: 🥔").GetBytes();
            byte[] exp = new byte[] { 17, 112, 111, 116, 97, 116, 111, 32, 85, 84, 70, 56, 58, 32, 240, 159, 165, 148 };
            Assert.AreEqual(exp, res);
        }

        [Test]
        public void DeserializeString()
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
        public void DeserializeStringLong()
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
        public void SerializeMap()
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
        public void DeserializeMap()
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

            Dictionary<BString, ISerializable> res = actualBcsMap.value;
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
        public void SerializeMapOfStringToU32_One()
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
        public void SerializeMapStringToU32_Two()
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
        public void SerializeMapStringToU32_Three()
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
        public void SerializeMapStringToString()
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
        public void SerializeEmptyStringBytes()
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
        public void SerializeMultipleValues()
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
        public void SerializeEmptyStringSequence()
        {
            Serialization ser = new Serialization();
            BString[] strArr = { new BString("") };
            Sequence seq = new Sequence(strArr);

            seq.Serialize(ser);

            byte[] actual = ser.GetBytes();
            byte[] expected = { 1, 1, 0 };
            Assert.AreEqual(expected, actual, ToReadableByteArray(actual));
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
        public void SerializeStringSequence()
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
        public void SerializerBoolSequence()
        {
            Bool[] inValue = { new Bool(false), new Bool(true), new Bool(false) };
            Serialization ser = new Serialization();
            ser.Serialize(inValue);

            byte[] expected = new byte[] { 3, 0, 1, 0 };
            byte[] actual = ser.GetBytes();

            Assert.AreEqual(expected, actual);
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