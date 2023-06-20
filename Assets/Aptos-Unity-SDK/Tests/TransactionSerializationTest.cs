using NUnit.Framework;
using Aptos.BCS;
using Aptos.Accounts;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using Aptos.HdWallet.Utils;

namespace Aptos.Unity.Test
{
    public class TransactionSerializationTest
    {
        internal Accounts.AccountAddress TestAddress()
        {
            return Accounts.AccountAddress.FromHex("0x01");
        }

        internal ModuleId TestModuleId()
        {
            return new ModuleId(TestAddress(), "my_module");
        }

        internal EntryFunction TestEntryFunction(ISerializableTag[] typeTags, ISerializable[] args)
        {
            return EntryFunction.Natural(TestModuleId(), "some_function", new TagSequence(typeTags), new Sequence(args));
        }

        [Test]
        public void AddressSerialize()
        {
            Serialization s = new Serialization();
            TestAddress().Serialize(s);
            byte[] res = s.GetBytes();
            Assert.AreEqual(
                new byte[]
                {
                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1
                }, res);
        }

        [Test]
        public void ModuleIdSerialize()
        {
            Serialization s = new Serialization();
            TestModuleId().Serialize(s);
            byte[] res = s.GetBytes();
            Assert.AreEqual(
                new byte[]
                {
                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 9, 109,
                    121, 95, 109, 111, 100, 117, 108, 101
                }, res);
        }

        /// <summary>
        /// Tests a transaction where the arguments sequence is empty
        /// 
        /// Python Example:
        /// txn = EntryFunction(mod, "some_function", [], [])
        /// </summary>
        [Test]
        public void Transaction_Simple_Serialize()
        {
            Serialization s = new Serialization();
            ISerializableTag[] tags = new ISerializableTag[] { };
            ISerializable[] args = new ISerializable[] { };

            TestEntryFunction(tags, args).Serialize(s);
            byte[] res = s.GetBytes();
            Assert.AreEqual(
                new byte[]
                {
                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 9, 109,
                    121, 95, 109, 111, 100, 117, 108, 101, 13, 115, 111, 109, 101, 95, 102, 117, 110, 99, 116, 105, 111,
                    110, 0, 0
                }, res, ToReadableByteArray(res));
        }

        [Test]
        public void Transaction_EmptyArgSequence_Serialize()
        {
            Serialization s = new Serialization();
            TestEntryFunction(new ISerializableTag[0], new ISerializable[0]).Serialize(s);
            byte[] res = s.GetBytes();
            Assert.AreEqual(new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 9, 109, 121, 95, 109, 111, 100, 117, 108, 101, 13, 115, 111, 109, 101, 95, 102, 117, 110, 99, 116, 105, 111, 110, 0, 0
            }, res, ToReadableByteArray(res));
        }

        [Test]
        public void SequenceBStringSerialize()
        {
            byte[] res;
            Serialization s;

            s = new Serialization();
            (new Sequence(new ISerializable[] { new BString("") })).Serialize(s);
            res = s.GetBytes();
            Assert.AreEqual(new byte[] { 1, 1, 0 }, res, ToReadableByteArray(res));

            s = new Serialization();
            (new Sequence(new ISerializable[] { new BString("A") })).Serialize(s);
            res = s.GetBytes();
            Assert.AreEqual(new byte[] { 1, 2, 1, 65 }, res);

            s = new Serialization();
            (new Sequence(new ISerializable[] { new BString("AA") })).Serialize(s);
            res = s.GetBytes();
            Assert.AreEqual(new byte[] { 1, 3, 2, 65, 65 }, res);
        }

        [Test]
        public void SequenceOfSequenceSerialize()
        {
            Serialization s;
            Sequence args;

            s = new Serialization();
            args = new Sequence( new ISerializable[] {
                    new Sequence(new ISerializable[0] ) 
            });
            args.Serialize(s);

            byte[] actual = s.GetBytes();
            Assert.AreEqual(new byte[] { 1, 1, 0 }, actual, ToReadableByteArray(actual));


            //s = new Serialization();
            //s.SerializeU32AsUleb128()
            //BytesSequence args2 = new BytesSequence(new[]
            //{

            //   Serialization.SerializeOne(Serialization.SerializeOne("A")),
            //});

            //args2.Serialize(s);
            //Assert.AreEqual(new byte[] { 1, 3, 1, 1, 65 }, s.GetBytes());
        }

        /// <summary>
        /// Python SDK Example:
        /// <code>
        /// addr = AccountAddress.from_hex("0x01")
        /// mid = ModuleId(addr, "my_module")
        /// txn = EntryFunction(mid, "some_function", [], [
        ///     TransactionArgument("wow", Serializer.str).encode(),
        /// ])
        ///     
        /// s = Serializer()
        /// txn.serialize(s)
        /// out = s.output()
        /// print([x for x in out])
        /// </code>
        /// </summary>
        [Test]
        public void Transaction_SingleStringArg_Serialize()
        {
            Serialization s = new Serialization();
            ISerializable[] args =
            {
                new BString("wow"),
            };
            TestEntryFunction(new ISerializableTag[0], args).Serialize(s);
            byte[] actual = s.GetBytes();
            byte[] expected = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 9, 109, 121, 95, 109, 111, 100, 117, 108, 101, 13, 115, 111, 109, 101, 95, 102, 117, 110, 99, 116, 105, 111, 110, 0, 1, 4, 3, 119, 111, 119 };
            Assert.AreEqual(expected, actual, ToReadableByteArray(actual));
        }

        /// <summary>
        /// Python SDK Example:
        /// <code>
        /// addr = AccountAddress.from_hex("0x01")
        /// mid = ModuleId(addr, "my_module")
        /// txn = EntryFunction(mid, "some_function", [], [
        ///     TransactionArgument(555555, Serializer.u64).encode(),
        /// ])
        ///     
        /// s = Serializer()
        /// txn.serialize(s)
        /// out = s.output()
        /// print([x for x in out])
        /// </code>
        /// </summary>
        [Test]
        public void Transaction_SingleU64Arg_Serialize()
        {
            Serialization s = new Serialization();
            ISerializable[] args =
            {
                new U64(555555),
            };
            TestEntryFunction(new ISerializableTag[0], args).Serialize(s);
            byte[] actual = s.GetBytes();
            byte[] expected = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 9, 109, 121, 95, 109, 111, 100, 117, 108, 101, 13, 115, 111, 109, 101, 95, 102, 117, 110, 99, 116, 105, 111, 110, 0, 1, 8, 35, 122, 8, 0, 0, 0, 0, 0 };
            Assert.AreEqual(expected, actual, ToReadableByteArray(actual));
        }

        /// <summary>
        /// Python SDK Example:
        /// <code>
        /// addr = AccountAddress.from_hex("0x01")
        /// mid = ModuleId(addr, "my_module")
        /// txn = EntryFunction(mid, "some_function", [], [
        ///     TransactionArgument([], Serializer.sequence_serializer(Serializer.bool)).encode()
        /// ])
        ///     
        /// s = Serializer()
        /// txn.serialize(s)
        /// out = s.output()
        /// print([x for x in out])
        /// </code>
        /// </summary>
        [Test]
        public void Transaction_WithEmptyBoolArgSequence_Serialize()
        {
            Bool[] boolSequence = new Bool[] { };
            Serialization s = new Serialization();
            ISerializable[] args =
            {
                new Sequence(boolSequence),
            };
            TestEntryFunction(new ISerializableTag[0], args).Serialize(s);
            byte[] actual = s.GetBytes();
            byte[] expected = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 9, 109, 121, 95, 109, 111, 100, 117, 108, 101, 13, 115, 111, 109, 101, 95, 102, 117, 110, 99, 116, 105, 111, 110, 0, 1, 1, 0 };
            Assert.AreEqual(expected, actual, ToReadableByteArray(actual));
        }

        /// <summary>
        /// Python SDK Example:
        /// <code>
        /// addr = AccountAddress.from_hex("0x01")
        /// mid = ModuleId(addr, "my_module")
        /// txn = EntryFunction(mid, "some_function", [], [
        ///     TransactionArgument([False], Serializer.sequence_serializer(Serializer.bool)).encode()
        /// ])
        ///     
        /// s = Serializer()
        /// txn.serialize(s)
        /// out = s.output()
        /// print([x for x in out])
        /// </code>
        /// </summary>
        [Test]
        public void Transaction_hOneBoolArgSequence_Serialize()
        {
            Serialization s = new Serialization();
            ISerializable[] args =
            {
                new Sequence(new[] { new Bool(false) }),
            };
            TestEntryFunction(new ISerializableTag[0], args).Serialize(s);
            byte[] actual = s.GetBytes();
            byte[] expected = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 9, 109, 121, 95, 109, 111, 100, 117, 108, 101, 13, 115, 111, 109, 101, 95, 102, 117, 110, 99, 116, 105, 111, 110, 0, 1, 2, 1, 0 };
            Assert.AreEqual(expected, actual, ToReadableByteArray(actual) + "\n" + ToReadableByteArray(expected));
        }

        //[Test]
        //public void SerializeTransactionWithOneBoolArgSequenceTWO()
        //{
        //    Serialization s = new Serialization();

        //    Bool[] arg1 = { new Bool(false) };
        //    ISerializable[] args =
        //    {
        //        arg1
        //    };
        //    TestEntryFunction(new ISerializableTag[0], args).Serialize(s);
        //    byte[] res = s.GetBytes();
        //    Assert.AreEqual(new byte[]
        //    {
        //        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 9, 109, 121, 95, 109, 111, 100, 117, 108, 101, 13, 115, 111, 109, 101, 95, 102, 117, 110, 99, 116, 105, 111, 110, 0, 1, 2, 1, 0
        //    }, res, ToReadableByteArray(res));
        //}


        /// <summary>
        /// Python SDK Example:
        /// <code>
        /// addr = AccountAddress.from_hex("0x01")
        /// mid = ModuleId(addr, "my_module")
        /// txn = EntryFunction(mid, "some_function", [], [
        ///     TransactionArgument([False, True], Serializer.sequence_serializer(Serializer.bool)).encode()
        /// ])
        ///     
        /// s = Serializer()
        /// txn.serialize(s)
        /// out = s.output()
        /// print([x for x in out])
        /// </code>
        /// </summary>
        [Test]
        public void Transaction_TwoBoolSequence_Serialize()
        {
            Serialization s = new Serialization();
            ISerializable[] args =
            {
                new Sequence(new[] { new Bool(false), new Bool(true)}),
            };
            TestEntryFunction(new ISerializableTag[0], args).Serialize(s);
            byte[] actual = s.GetBytes();

            byte[] expected = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 9, 109, 121, 95, 109, 111, 100, 117, 108, 101, 13, 115, 111, 109, 101, 95, 102, 117, 110, 99, 116, 105, 111, 110, 0, 1, 3, 2, 0, 1 };
            Assert.AreEqual(expected, actual, ToReadableByteArray(actual) + "\n" + ToReadableByteArray(expected));
        }

        /// <summary>
        /// Python SDK Example:
        /// <code>
        /// addr = AccountAddress.from_hex("0x01")
        /// mid = ModuleId(addr, "my_module")
        /// txn = EntryFunction(mid, "some_function", [], [
        ///     TransactionArgument([False, True, False], Serializer.sequence_serializer(Serializer.bool)).encode()
        /// ])
        ///     
        /// s = Serializer()
        /// txn.serialize(s)
        /// out = s.output()
        /// print([x for x in out])
        /// </code>
        /// </summary>
        [Test]
        public void Transaction_ThreeBoolArgsSequence_Serialize()
        {
            Serialization s = new Serialization();
            ISerializable[] args =
            {
                new Sequence(new[] { new Bool(false), new Bool(true), new Bool(false) }),
            };
            TestEntryFunction(new ISerializableTag[0], args).Serialize(s);
            byte[] actual = s.GetBytes();
            byte[] expected = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 9, 109, 121, 95, 109, 111, 100, 117, 108, 101, 13, 115, 111, 109, 101, 95, 102, 117, 110, 99, 116, 105, 111, 110, 0, 1, 4, 3, 0, 1, 0 };
            Assert.AreEqual(expected, actual, ToReadableByteArray(actual) + "\n" + ToReadableByteArray(expected));
        }

        /// <summary>
        /// Python SDK Example:
        /// <code>
        /// addr = AccountAddress.from_hex("0x01")
        /// mid = ModuleId(addr, "my_module")
        /// txn = EntryFunction(mid, "some_function", [], [
        ///     TransactionArgument(["A"], Serializer.sequence_serializer(Serializer.str)).encode(),
        /// ])
        ///     
        /// s = Serializer()
        /// txn.serialize(s)
        /// out = s.output()
        /// print([x for x in out])
        /// </code>
        /// </summary>
        [Test]
        public void Transaction_WithOneStringArgSequence_Serialize()
        {
            Serialization s = new Serialization();
            ISerializable[] args =
            {
                new Sequence(new[] { new BString("A") }),
            };
            TestEntryFunction(new ISerializableTag[0], args).Serialize(s);
            byte[] actual = s.GetBytes();
            byte[] expected = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 9, 109, 121, 95, 109, 111, 100, 117, 108, 101, 13, 115, 111, 109, 101, 95, 102, 117, 110, 99, 116, 105, 111, 110, 0, 1, 3, 1, 1, 65 };
            Assert.AreEqual(expected, actual, ToReadableByteArray(actual) + "\n" + ToReadableByteArray(expected));
        }


        /// <summary>
        /// Python SDK Example:
        /// <code>
        /// addr = AccountAddress.from_hex("0x01")
        /// mid = ModuleId(addr, "my_module")
        /// txn = EntryFunction(mid, "some_function", [], [
        ///     TransactionArgument(["A", "B"], Serializer.sequence_serializer(Serializer.str)).encode(),
        /// ])
        ///     
        /// s = Serializer()
        /// txn.serialize(s)
        /// out = s.output()
        /// print([x for x in out])
        /// </code>
        /// </summary>
        [Test]
        public void Transaction_WithTwoStringArgSequence_Serialize()
        {
            Serialization s = new Serialization();
            ISerializable[] args =
            {
                new Sequence(new[] { new BString("A"), new BString("B") }),
            };
            TestEntryFunction(new ISerializableTag[0], args).Serialize(s);
            byte[] actual = s.GetBytes();
            byte[] expected = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 9, 109, 121, 95, 109, 111, 100, 117, 108, 101, 13, 115, 111, 109, 101, 95, 102, 117, 110, 99, 116, 105, 111, 110, 0, 1, 5, 2, 1, 65, 1, 66 };
            Assert.AreEqual(expected, actual, ToReadableByteArray(actual) + "\n" + ToReadableByteArray(expected));
        }

        /// <summary>
        /// Python SDK Example:
        /// <code>
        /// addr = AccountAddress.from_hex("0x01")
        /// mid = ModuleId(addr, "my_module")
        /// txn = EntryFunction(mid, "some_function", [], [
        ///     TransactionArgument(["A", "B", "C"], Serializer.sequence_serializer(Serializer.str)).encode(),
        /// ])
        ///     
        /// s = Serializer()
        /// txn.serialize(s)
        /// out = s.output()
        /// print([x for x in out])
        /// </code>
        /// </summary>
        [Test]
        public void Transaction_ThreeStringArgSequence_Serialize()
        {
            Serialization s = new Serialization();
            ISerializable[] args =
            {
                new Sequence(new[] { new BString("A"), new BString("B"), new BString("C") }),
            };
            TestEntryFunction(new ISerializableTag[0], args).Serialize(s);
            byte[] actual = s.GetBytes();
            byte[] expected = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 9, 109, 121, 95, 109, 111, 100, 117, 108, 101, 13, 115, 111, 109, 101, 95, 102, 117, 110, 99, 116, 105, 111, 110, 0, 1, 7, 3, 1, 65, 1, 66, 1, 67 };
            Assert.AreEqual(expected, actual, ToReadableByteArray(actual) + "\n" + ToReadableByteArray(expected));
        }

        /// <summary>
        /// Python SDK Example:
        /// <code>
        /// addr = AccountAddress.from_hex("0x01")
        /// mid = ModuleId(addr, "my_module")
        /// txn = EntryFunction(mid, "some_function", [], [
        ///     TransactionArgument("A", Serializer.str).encode(),
        ///     TransactionArgument( [False], Serializer.sequence_serializer(Serializer.bool)).encode()
        /// ])
        ///     
        /// s = Serializer()
        /// txn.serialize(s)
        /// out = s.output()
        /// print([x for x in out])
        /// </code>
        /// </summary>
        [Test]
        public void Transactio_OneStringOneBoolArgSequence_Serialize()
        {
            Serialization s = new Serialization();
            ISerializable[] args =
            {
                new BString("A"),
                new Sequence(new[] { new Bool(false) }),
            };
            TestEntryFunction(new ISerializableTag[0], args).Serialize(s);
            byte[] actual = s.GetBytes();
            byte[] expected = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 9, 109, 121, 95, 109, 111, 100, 117, 108, 101, 13, 115, 111, 109, 101, 95, 102, 117, 110, 99, 116, 105, 111, 110, 0, 2, 2, 1, 65, 2, 1, 0 };
            Assert.AreEqual(expected, actual, ToReadableByteArray(actual) + "\n" + ToReadableByteArray(expected));
        }


        /// <summary>
        /// Python SDK Example:
        /// <code>
        /// addr = AccountAddress.from_hex("0x01")
        /// mid = ModuleId(addr, "my_module")
        /// txn = EntryFunction(mid, "some_function", [], [
        ///     TransactionArgument("A", Serializer.str).encode(),
        ///     TransactionArgument(1, Serializer.u64).encode(),
        ///     TransactionArgument( [False], Serializer.sequence_serializer(Serializer.bool)).encode()
        /// ])
        ///     
        /// s = Serializer()
        /// txn.serialize(s)
        /// out = s.output()
        /// print([x for x in out])
        /// </code>
        /// </summary>
        [Test]
        public void Transaction_OneStringOneIntOneBoolArgSequence_Serialize()
        {
            Serialization s = new Serialization();
            ISerializable[] args =
            {
                new BString("A"),
                new U64(1),
                new Sequence(new[] { new Bool(false) }),
            };
            TestEntryFunction(new ISerializableTag[0], args).Serialize(s);
            byte[] actual = s.GetBytes();
            byte[] expected = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 9, 109, 121, 95, 109, 111, 100, 117, 108, 101, 13, 115, 111, 109, 101, 95, 102, 117, 110, 99, 116, 105, 111, 110, 0, 3, 2, 1, 65, 8, 1, 0, 0, 0, 0, 0, 0, 0, 2, 1, 0 };
            Assert.AreEqual(expected, actual, ToReadableByteArray(actual) + "\n" + ToReadableByteArray(expected));
        }

        /// <summary>
        /// Python SDK Example:
        /// <code>
        /// addr = AccountAddress.from_hex("0x01")
        /// mid = ModuleId(addr, "my_module")
        /// txn = EntryFunction(mid, "some_function", [], [
        ///     TransactionArgument("A", Serializer.str).encode(),
        ///     TransactionArgument(1, Serializer.u64).encode(),
        ///     TransactionArgument(addr, Serializer.struct).encode(),
        ///     TransactionArgument( [False], Serializer.sequence_serializer(Serializer.bool)).encode()
        /// ])
        ///     
        /// s = Serializer()
        /// txn.serialize(s)
        /// out = s.output()
        /// print([x for x in out])
        /// </code>
        /// </summary>
        [Test]
        public void Transaction_OneStringOneIntOneAddressOneBoolArgSequence_Serialize()
        {
            Serialization s = new Serialization();
            ISerializable[] args =
            {
                new BString("A"),
                new U64(1),
                TestAddress(),
                new Sequence(new[] { new Bool(false) }),
            };
            TestEntryFunction(new ISerializableTag[0], args).Serialize(s);
            byte[] actual = s.GetBytes();
            byte[] expected = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 9, 109, 121, 95, 109, 111, 100, 117, 108, 101, 13, 115, 111, 109, 101, 95, 102, 117, 110, 99, 116, 105, 111, 110, 0, 4, 2, 1, 65, 8, 1, 0, 0, 0, 0, 0, 0, 0, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 1, 0 };
            Assert.AreEqual(expected, actual, ToReadableByteArray(actual) + "\n" + ToReadableByteArray(expected));
        }

        /// <summary>
        /// Python SDK Example:
        /// 
        /// addr = AccountAddress.from_hex("0x01")
        /// mid = ModuleId(addr, "my_module")
        /// txn = EntryFunction(mid, "some_function", [], [
        ///     TransactionArgument("A", Serializer.str).encode(),
        ///     TransactionArgument(1, Serializer.u64).encode(),
        ///     TransactionArgument(addr, Serializer.struct).encode(),
        ///     TransactionArgument([False], Serializer.sequence_serializer(Serializer.bool)).encode(),
        ///     TransactionArgument([False, True], Serializer.sequence_serializer(Serializer.bool)).encode(),
        ///     TransactionArgument(["A", "B", "C"], Serializer.sequence_serializer(Serializer.str)).encode()
        /// ])
        /// s = Serializer()
        /// txn.serialize(s)
        /// out = s.output()
        /// print([x for x in out])
        /// </summary>
        [Test]
        public void Transaction_OneStringOneIntOneAddressMultipleArgSequences_Serialize()
        {
            Serialization s = new Serialization();
            ISerializable[] args =
            {
                new BString("A"),
                new U64(1),
                TestAddress(),
                new Sequence(new[] { new Bool(false) }),
                new Sequence(new[] { new Bool(false), new Bool(true) }),
                new Sequence(new[] { new BString("A"), new BString("B"), new BString("C") }),
            };
            TestEntryFunction(new ISerializableTag[0], args).Serialize(s);

            byte[] actual = s.GetBytes();
            byte[] expected = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 9, 109, 121, 95, 109, 111, 100, 117, 108, 101, 13, 115, 111, 109, 101, 95, 102, 117, 110, 99, 116, 105, 111, 110, 0, 6, 2, 1, 65, 8, 1, 0, 0, 0, 0, 0, 0, 0, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 1, 0, 3, 2, 0, 1, 7, 3, 1, 65, 1, 66, 1, 67 };
            Assert.AreEqual(expected, actual, ToReadableByteArray(actual) + "\n" + ToReadableByteArray(expected));
        }

        [Test]
        public void ModuleFromString()
        {
            AccountAddress accountAddress = AccountAddress.FromHex("0x1");
            string name = "coin";

            ModuleId expectedModuleId = new ModuleId(accountAddress, name);

            ModuleId actualModuleId = ModuleId.FromStr("0x1::coin");

            Assert.AreEqual(expectedModuleId, actualModuleId);
        }

        [Test]
        public void StructTagFromString()
        {
            StructTag expectedStructTag = new StructTag(AccountAddress.FromHex("0x1"), "aptos_coin", "AptosCoin", new ISerializableTag[0]);
            StructTag actualStructTag = StructTag.FromStr("0x1::aptos_coin::AptosCoin");
            Assert.AreEqual(expectedStructTag, actualStructTag);
        }

        [Test]
        public void TagSequenceSerialize()
        {
            TagSequence typeTags = new TagSequence(
                new ISerializableTag[] {
                    new StructTag(AccountAddress.FromHex("0x1"), "aptos_coin", "AptosCoin", new ISerializableTag[0])
                }
            );

            Serialization ser = new Serialization();
            typeTags.Serialize(ser);
            byte[] typeTagsBytes = ser.GetBytes();

            Deserialization deser = new Deserialization(typeTagsBytes);
            TagSequence actualTagSeq = TagSequence.Deserialize(deser);
            Assert.AreEqual(typeTags, actualTagSeq);
        }

        /// <summary>
        /// addr = AccountAddress.from_hex("0x1")
        /// mid = ModuleId(addr, "coin")
        /// transaction_arguments = [
        ///     TransactionArgument(addr, Serializer.struct),
        ///     TransactionArgument(1000, Serializer.u64),
        /// ]
        /// 
        /// payload = EntryFunction.natural(
        ///     "0x1::coin",
        ///     "transfer",
        ///     [TypeTag(StructTag.from_str("0x1::aptos_coin::AptosCoin"))],
        ///     transaction_arguments,
        /// )
        /// 
        /// ser = Serializer()
        /// payload.serialize(ser)
        /// out = ser.output()
        /// 
        /// print([x for x in out])
        /// </summary>
        [Test]
        public void EntryFunction_PayloadForTransferCoin_Serialize()
        {
            TagSequence typeTags = new TagSequence(
                new ISerializableTag[] { 
                    new StructTag(AccountAddress.FromHex("0x1"), "aptos_coin", "AptosCoin", new ISerializableTag[0])
                }
            );

            ISerializable[] args =
            {
                AccountAddress.FromHex("0x1"),
                new U64(1000),
            };

            Sequence txnArgs = new Sequence(args);

            EntryFunction payload = EntryFunction.Natural(
                new ModuleId(AccountAddress.FromHex("0x1"), "coin"),
                "transfer",
                typeTags,
                txnArgs
            );

            Serialization s = new Serialization();
            payload.Serialize(s);

            byte[] actual = s.GetBytes();
            byte[] expected = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 4, 99, 111, 105, 110, 8, 116, 114, 97, 110, 115, 102, 101, 114, 1, 7, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 10, 97, 112, 116, 111, 115, 95, 99, 111, 105, 110, 9, 65, 112, 116, 111, 115, 67, 111, 105, 110, 0, 2, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 8, 232, 3, 0, 0, 0, 0, 0, 0 };
            Assert.AreEqual(expected, actual, ToReadableByteArray(actual));
        }

        /// <summary>
        /// 
        /// Python SDK Example:
        /// <code>
        /// carol_addr = AccountAddress.from_hex("0x01")
        /// david_addr = AccountAddress.from_hex("0x01")
        /// 
        /// script_arguments = [
        ///     ScriptArgument(ScriptArgument.U64, 100),
        ///     ScriptArgument(ScriptArgument.U64, 200),
        ///     ScriptArgument(ScriptArgument.ADDRESS, carol_addr),
        ///     ScriptArgument(ScriptArgument.ADDRESS, david_addr),
        ///     ScriptArgument(ScriptArgument.U64, 50),
        /// ]
        /// 
        /// script = Script(code, [], script_arguments)
        /// ser = Serializer()
        /// script.serialize(ser)
        /// out = ser.output()
        /// print([x for x in out])
        /// </code>
        /// </summary>
        [Test]
        public void ScriptSerialize()
        {
            string dp = Application.dataPath;
           
            string[] s = dp.Split("/"[0]);
            Debug.Log("project = " + s[s.Length - 2]);

            string path = Application.dataPath + "/Aptos-Unity-SDK/Tests" + "/two_by_two_transfer.mv";
            byte[] code = File.ReadAllBytes(path);

            AccountAddress caroldAddress = AccountAddress.FromHex("0x1");
            AccountAddress davidAddress = AccountAddress.FromHex("0x1");

            TagSequence typeArgs = new TagSequence(new ISerializableTag[0]);

            ISerializable[] args =
            {
                new ScriptArgument(ScriptArgument.TypeTag.U64, new U64(100)),
                new ScriptArgument(ScriptArgument.TypeTag.U64, new U64(200)),
                new ScriptArgument(ScriptArgument.TypeTag.ADDRESS, caroldAddress),
                new ScriptArgument(ScriptArgument.TypeTag.ADDRESS, davidAddress),
                new ScriptArgument(ScriptArgument.TypeTag.U64, new U64(50))
            };
            Sequence scriptArgs = new Sequence(args);

            Script script = new Script(code, typeArgs, scriptArgs);

            Serialization ser = new Serialization();
            script.Serialize(ser);
            byte[] actual = ser.GetBytes();
            byte[] expected = { 187, 2, 161, 28, 235, 11, 5, 0, 0, 0, 8, 1, 0, 4, 2, 4, 10, 3, 14, 24, 4, 38, 8, 5, 46, 67, 7, 113, 62, 8, 175, 1, 32, 6, 207, 1, 20, 0, 0, 0, 1, 1, 2, 4, 1, 0, 1, 0, 3, 8, 0, 1, 4, 3, 4, 1, 0, 1, 5, 5, 6, 1, 0, 1, 6, 7, 4, 1, 0, 1, 7, 8, 6, 1, 0, 0, 2, 1, 2, 2, 2, 3, 2, 7, 6, 12, 6, 12, 3, 3, 5, 5, 3, 3, 11, 0, 1, 8, 1, 11, 0, 1, 8, 1, 11, 0, 1, 8, 1, 1, 8, 1, 2, 6, 12, 3, 1, 11, 0, 1, 9, 0, 2, 7, 11, 0, 1, 9, 0, 11, 0, 1, 9, 0, 0, 2, 7, 11, 0, 1, 9, 0, 3, 2, 5, 11, 0, 1, 9, 0, 10, 97, 112, 116, 111, 115, 95, 99, 111, 105, 110, 4, 99, 111, 105, 110, 4, 67, 111, 105, 110, 9, 65, 112, 116, 111, 115, 67, 111, 105, 110, 8, 119, 105, 116, 104, 100, 114, 97, 119, 5, 109, 101, 114, 103, 101, 7, 101, 120, 116, 114, 97, 99, 116, 7, 100, 101, 112, 111, 115, 105, 116, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 3, 8, 160, 134, 1, 0, 0, 0, 0, 0, 3, 8, 255, 255, 255, 255, 255, 255, 255, 255, 0, 0, 1, 26, 11, 0, 10, 2, 56, 0, 12, 7, 11, 1, 10, 3, 56, 0, 12, 8, 13, 7, 11, 8, 56, 1, 13, 7, 11, 2, 11, 3, 22, 11, 6, 23, 56, 2, 12, 9, 11, 4, 11, 7, 56, 3, 11, 5, 11, 9, 56, 3, 2, 0, 5, 1, 100, 0, 0, 0, 0, 0, 0, 0, 1, 200, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 50, 0, 0, 0, 0, 0, 0, 0 };
            Assert.AreEqual(expected, actual, ToReadableByteArray(actual));

            // Test Script Deseiralization
            Deserialization deser = new Deserialization(actual);
            Script actualScript = Script.Deserialize(deser);
            Assert.AreEqual(script, actualScript);
        }

        /// <summary>
        /// Python SDK Example:
        /// <code>
        /// addr = AccountAddress.from_hex("0x1")
        /// mid = ModuleId(addr, "coin")
        /// transaction_arguments = [
        ///     TransactionArgument(addr, Serializer.struct),
        ///     TransactionArgument(1000, Serializer.u64),
        /// ]
        /// 
        /// payload = EntryFunction.natural(
        ///     "0x1::coin",
        ///     "transfer",
        ///     [TypeTag(StructTag.from_str("0x1::aptos_coin::AptosCoin"))],
        ///     transaction_arguments,
        /// )
        /// 
        /// txnPayload = TransactionPayload(payload)
        /// 
        /// ser = Serializer()
        /// txnPayload.serialize(ser)
        /// out = ser.output()
        /// 
        /// print([x for x in out])
        /// </code>
        /// </summary>
        [Test]
        public void Transaction_PayloadForTransferCoin_Serialize()
        {
            //Account alice = Account.LoadKey("0x64f57603b58af16907c18a866123286e1cbce89790613558dc1775abb3fc5c8c");
            //string acctAddressAlice = alice.AccountAddress.ToString();

            //Account bob = Account.LoadKey("0xb10d4b38bef8a0d3e8747a84cfdc764b89a528af98e055e0237b11863afa9825");
            //string acctAddressBob = bob.AccountAddress.ToString();

            //TestEntryFunction(new ISerializableTag[0], args).Serialize(s);

            TagSequence typeTags = new TagSequence(
                new ISerializableTag[] { 
                    new StructTag(AccountAddress.FromHex("0x1"), "aptos_coin", "AptosCoin", new ISerializableTag[0])
                }
            );

            ISerializable[] args =
            {
                AccountAddress.FromHex("0x1"),
                new U64(1000),
            };

            Sequence txnArgs = new Sequence(args);

            EntryFunction payload = EntryFunction.Natural(
                new ModuleId(AccountAddress.FromHex("0x1"), "coin"),
                "transfer",
                typeTags,
                txnArgs
            );

            TransactionPayload txnPayload = new TransactionPayload(payload);

            Serialization s = new Serialization();
            txnPayload.Serialize(s);

            byte[] actual = s.GetBytes();
            byte[] expected = { 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 4, 99, 111, 105, 110, 8, 116, 114, 97, 110, 115, 102, 101, 114, 1, 7, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 10, 97, 112, 116, 111, 115, 95, 99, 111, 105, 110, 9, 65, 112, 116, 111, 115, 67, 111, 105, 110, 0, 2, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 8, 232, 3, 0, 0, 0, 0, 0, 0 };
            Assert.AreEqual(expected, actual, ToReadableByteArray(actual));
        }


        /// <summary>
        /// Python SDK Example:
        /// <code>
        /// addr = AccountAddress.from_hex("0x1")
        /// mid = ModuleId(addr, "coin")
        /// transaction_arguments = [
        ///     TransactionArgument(addr, Serializer.struct),
        ///     TransactionArgument(1000, Serializer.u64),
        /// ]
        /// 
        /// payload = EntryFunction.natural(
        ///     "0x1::coin",
        ///     "transfer",
        ///     [TypeTag(StructTag.from_str("0x1::aptos_coin::AptosCoin"))],
        ///     transaction_arguments,
        /// )
        /// 
        /// txnPayload = TransactionPayload(payload)
        /// 
        /// raw_transaction = RawTransaction(
        ///     addr,
        ///     0,
        ///     TransactionPayload(payload),
        ///     2000,
        ///     0,
        ///     18446744073709551615,
        ///     4,
        ///     )
        ///     
        /// s = Serializer()
        /// raw_transaction.serialize(s)
        /// out = s.output()
        /// 
        /// print([x for x in out])
        /// </code>
        /// </summary>
        [Test]
        public void RawTransaction_TransferCoin_Serialize()
        {
            TagSequence typeTags = new TagSequence(
                new ISerializableTag[] {
                    new StructTag(AccountAddress.FromHex("0x1"), "aptos_coin", "AptosCoin", new ISerializableTag[0])
                }
            );

            ISerializable[] args =
            {
                AccountAddress.FromHex("0x1"),
                new U64(1000),
            };

            Sequence txnArgs = new Sequence(args);

            EntryFunction payload = EntryFunction.Natural(
                new ModuleId(AccountAddress.FromHex("0x1"), "coin"),
                "transfer",
                typeTags,
                txnArgs
            );

            TransactionPayload txnPayload = new TransactionPayload(payload);

            AccountAddress accountAddress = AccountAddress.FromHex("0x1");

            RawTransaction rawTransaction = new RawTransaction(
                accountAddress,
                0,
                txnPayload,
                2000,
                0,
                18446744073709551615,
                4
            );

            Serialization s = new Serialization();
            rawTransaction.Serialize(s);

            byte[] actual = s.GetBytes();
            byte[] expected = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 4, 99, 111, 105, 110, 8, 116, 114, 97, 110, 115, 102, 101, 114, 1, 7, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 10, 97, 112, 116, 111, 115, 95, 99, 111, 105, 110, 9, 65, 112, 116, 111, 115, 67, 111, 105, 110, 0, 2, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 8, 232, 3, 0, 0, 0, 0, 0, 0, 208, 7, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 255, 255, 255, 255, 255, 255, 4 };
            Assert.AreEqual(expected, actual, ToReadableByteArray(actual));

            Deserialization deser = new Deserialization(actual);
            RawTransaction actualRawTxn = RawTransaction.Deserialize(deser);

            Assert.AreEqual(rawTransaction, actualRawTxn, actualRawTxn.ToString());
        }

        [Test]
        public void RawTransaction_TransferCoin_Prehash()
        {
            TagSequence typeTags = new TagSequence(
                new ISerializableTag[] {
                    new StructTag(AccountAddress.FromHex("0x1"), "aptos_coin", "AptosCoin", new ISerializableTag[0])
                }
            );

            ISerializable[] args =
            {
                AccountAddress.FromHex("0x1"),
                new U64(1000),
            };

            Sequence txnArgs = new Sequence(args);

            EntryFunction payload = EntryFunction.Natural(
                new ModuleId(AccountAddress.FromHex("0x1"), "coin"),
                "transfer",
                typeTags,
                txnArgs
            );

            TransactionPayload txnPayload = new TransactionPayload(payload);

            AccountAddress accountAddress = AccountAddress.FromHex("0x1");

            RawTransaction rawTransaction = new RawTransaction(
                accountAddress,
                0,
                txnPayload,
                2000,
                0,
                18446744073709551615,
                4
            );


            byte[] actual = rawTransaction.Prehash();
            byte[] expected = { 181, 233, 125, 176, 127, 160, 189, 14, 85, 152, 170, 54, 67, 169, 188, 111, 102, 147, 189, 220, 26, 159, 236, 158, 103, 74, 70, 30, 170, 0, 177, 147 };
            Assert.AreEqual(expected, actual, ToReadableByteArray(actual));
        }

        [Test]
        public void RawTransaction_TransferCoin_Keyed()
        {
            TagSequence typeTags = new TagSequence(
                new ISerializableTag[] {
                    new StructTag(AccountAddress.FromHex("0x1"), "aptos_coin", "AptosCoin", new ISerializableTag[0])
                }
            );

            ISerializable[] args =
            {
                AccountAddress.FromHex("0x1"),
                new U64(1000),
            };

            Sequence txnArgs = new Sequence(args);

            EntryFunction payload = EntryFunction.Natural(
                new ModuleId(AccountAddress.FromHex("0x1"), "coin"),
                "transfer",
                typeTags,
                txnArgs
            );

            TransactionPayload txnPayload = new TransactionPayload(payload);

            AccountAddress accountAddress = AccountAddress.FromHex("0x1");

            RawTransaction rawTransaction = new RawTransaction(
                accountAddress,
                0,
                txnPayload,
                2000,
                0,
                18446744073709551615,
                4
            );

            byte[] actual = rawTransaction.Keyed();
            byte[] expected = { 181, 233, 125, 176, 127, 160, 189, 14, 85, 152, 170, 54, 67, 169, 188, 111, 102, 147, 189, 220, 26, 159, 236, 158, 103, 74, 70, 30, 170, 0, 177, 147, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 4, 99, 111, 105, 110, 8, 116, 114, 97, 110, 115, 102, 101, 114, 1, 7, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 10, 97, 112, 116, 111, 115, 95, 99, 111, 105, 110, 9, 65, 112, 116, 111, 115, 67, 111, 105, 110, 0, 2, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 8, 232, 3, 0, 0, 0, 0, 0, 0, 208, 7, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 255, 255, 255, 255, 255, 255, 4 };

            Assert.AreEqual(expected, actual, ToReadableByteArray(actual));
        }

        [Test]
        public void EntryFunction_WithCorpus_Serialize()
        {
            string senderKeyInput = "9bf49a6a0755f953811fce125f2683d50429c3bb49e074147e0089a52eae155f";
            string receiverKeyInput = "0564f879d27ae3c02ce82834acfa8c793a629f2ca0de6919610be82f411326be";

            int sequenceNumberInput = 11;
            int gasUnitPriceInput = 1;
            int maxGasAmountInput = 2000;
            ulong expirationTimestampsSecsInput = 1234567890;
            int chainIdInput = 4;
            int amountInput = 5000;

            // Accounts and crypto
            PrivateKey senderPrivateKey = PrivateKey.FromHex(senderKeyInput);
            PublicKey senderPublicKey = senderPrivateKey.PublicKey();
            AccountAddress senderAccountAddress = AccountAddress.FromKey(senderPublicKey);

            PrivateKey receiverPrivateKey = PrivateKey.FromHex(receiverKeyInput);
            PublicKey receiverPublicKey = receiverPrivateKey.PublicKey();
            AccountAddress receiverAccountAddress = AccountAddress.FromKey(receiverPublicKey);

            // Transaction arguments
            ISerializable[] txnArgs = 
            {
                receiverAccountAddress,
                new U64(Convert.ToUInt64(amountInput))
            };

            Sequence txnArgsSeq = new Sequence(txnArgs);

            TagSequence typeTags = new TagSequence(
                new ISerializableTag[] { 
                    new StructTag(AccountAddress.FromHex("0x1"), "aptos_coin", "AptosCoin", new ISerializableTag[0])
                }
            );

            EntryFunction payload = EntryFunction.Natural(
                new ModuleId(AccountAddress.FromHex("0x1"), "coin"),
                "transfer",
                typeTags,
                txnArgsSeq
            );

            RawTransaction rawTransactionGenerated = new RawTransaction(
                senderAccountAddress,
                sequenceNumberInput,
                new TransactionPayload(payload),
                maxGasAmountInput,
                gasUnitPriceInput,
                expirationTimestampsSecsInput,
                chainIdInput
            );

            Signature senderSignature = rawTransactionGenerated.Sign(senderPrivateKey);
            bool verifySenderSignature = rawTransactionGenerated.Verify(senderPublicKey, senderSignature);
            Assert.IsTrue(verifySenderSignature);

            Authenticator authenticator = new Authenticator(
                new Ed25519Authenticator(
                    senderPublicKey, senderSignature
                )
            );

            Serialization ser = new Serialization();
            authenticator.Serialize(ser);

            byte[] actual = ser.GetBytes();
            byte[] expected = { 0, 32, 185, 198, 238, 22, 48, 239, 62, 113, 17, 68, 166, 72, 219, 6, 187, 178, 40, 79, 114, 116, 207, 190, 229, 63, 252, 238, 80, 60, 193, 164, 146, 0, 64, 242, 91, 116, 236, 96, 163, 138, 30, 215, 128, 253, 43, 239, 109, 219, 110, 180, 53, 110, 58, 179, 146, 118, 201, 23, 108, 223, 15, 202, 226, 171, 55, 215, 155, 98, 106, 187, 67, 217, 38, 233, 21, 149, 182, 101, 3, 164, 163, 201, 10, 203, 174, 54, 162, 141, 64, 94, 48, 143, 53, 55, 175, 114, 11 };

            Assert.AreEqual(expected, actual, ToReadableByteArray(actual));

            Deserialization deser = new Deserialization(actual);
            Authenticator authenticatorDeserialized = (Authenticator)Authenticator.Deserialize(deser);

            Assert.AreEqual(authenticator.GetVariant(), authenticatorDeserialized.GetVariant());

            string internals = authenticator.ToString() + " ::: " + authenticatorDeserialized.ToString();
            Assert.AreEqual(authenticator, authenticatorDeserialized, internals);

            SignedTransaction signedTransactionGenerated = new SignedTransaction(
                rawTransactionGenerated, authenticator
            );

            Assert.IsTrue(signedTransactionGenerated.Verify());
        }

        /// <summary>
        /// Python SDK Example:
        /// <code>
        /// # Define common inputs
        /// sender_key_input = (
        ///     "9bf49a6a0755f953811fce125f2683d50429c3bb49e074147e0089a52eae155f"
        /// )
        /// 
        /// receiver_key_input = (
        ///     "0564f879d27ae3c02ce82834acfa8c793a629f2ca0de6919610be82f411326be"
        /// )
        /// 
        /// sequence_number_input = 11
        /// gas_unit_price_input = 1
        /// max_gas_amount_input = 2000
        /// expiration_timestamps_secs_input = 1234567890
        /// chain_id_input = 4
        /// 
        /// # Accounts and crypto
        /// sender_private_key = ed25519.PrivateKey.from_hex(sender_key_input)
        /// sender_public_key = sender_private_key.public_key()
        /// sender_account_address = AccountAddress.from_key(sender_public_key)
        /// 
        /// receiver_private_key = ed25519.PrivateKey.from_hex(receiver_key_input)
        /// receiver_public_key = receiver_private_key.public_key()
        /// receiver_account_address = AccountAddress.from_key(receiver_public_key)
        /// 
        /// # Generate the transaction locally
        /// transaction_arguments = [
        ///     TransactionArgument(receiver_account_address, Serializer.struct),
        ///     TransactionArgument("collection_name", Serializer.str),
        ///     TransactionArgument("token_name", Serializer.str),
        ///     TransactionArgument(1, Serializer.u64),
        /// ]
        /// 
        /// payload = EntryFunction.natural(
        ///     "0x3::token",
        ///     "direct_transfer_script",
        ///     [],
        ///     transaction_arguments,
        /// )
        /// 
        /// raw_transaction_generated = MultiAgentRawTransaction(
        ///     RawTransaction(
        ///         sender_account_address, 
        ///         sequence_number_input,
        ///         TransactionPayload(payload),
        ///         max_gas_amount_input,
        ///         gas_unit_price_input,
        ///         expiration_timestamps_secs_input,
        ///         chain_id_input,
        ///     ),
        ///     [receiver_account_address],
        /// )
        /// 
        /// sender_signature = raw_transaction_generated.sign(sender_private_key)
        /// receiver_signature = raw_transaction_generated.sign(receiver_private_key)
        /// self.assertTrue(
        ///     raw_transaction_generated.verify(sender_public_key, sender_signature)
        /// )
        /// self.assertTrue(
        ///     raw_transaction_generated.verify(receiver_public_key, receiver_signature)
        /// )
        /// 
        /// authenticator = Authenticator(
        ///     MultiAgentAuthenticator(
        ///         Authenticator(
        ///             Ed25519Authenticator(sender_public_key, sender_signature)
        ///         ),
        ///         [
        ///             (
        ///                 receiver_account_address,
        ///                 Authenticator(
        ///                     Ed25519Authenticator(
        ///                         receiver_public_key, receiver_signature
        ///                     )
        ///                 ),
        ///             )
        ///         ],
        ///     )
        /// )
        /// 
        /// signed_transaction_generated = SignedTransaction(
        ///     raw_transaction_generated.inner(), authenticator
        /// )
        /// self.assertTrue(signed_transaction_generated.verify())
        /// </code>
        /// </summary>
        [Test]
        public void EntryFunction_MultiAgentWithCorpus_Serialize()
        {
            string senderKeyInput = "9bf49a6a0755f953811fce125f2683d50429c3bb49e074147e0089a52eae155f";
            string receiverKeyInput = "0564f879d27ae3c02ce82834acfa8c793a629f2ca0de6919610be82f411326be";

            int sequenceNumberInput = 11;
            int gasUnitPriceInput = 1;
            int maxGasAmountInput = 2000;
            ulong expirationTimestampsSecsInput = 1234567890;
            int chainIdInput = 4;

            // Accounts and crypto
            PrivateKey senderPrivateKey = PrivateKey.FromHex(senderKeyInput);
            PublicKey senderPublicKey = senderPrivateKey.PublicKey();
            AccountAddress senderAccountAddress = AccountAddress.FromKey(senderPublicKey);

            PrivateKey receiverPrivateKey = PrivateKey.FromHex(receiverKeyInput);
            PublicKey receiverPublicKey = receiverPrivateKey.PublicKey();
            AccountAddress receiverAccountAddress = AccountAddress.FromKey(receiverPublicKey);

            // Transaction arguments
            ISerializable[] txnArgs =
            {
                receiverAccountAddress,
                new BString("collection_name"),
                new BString("token_name"),
                new U64(1)
            };

            Sequence txnArgsSeq = new Sequence(txnArgs);

            // Type tags
            TagSequence typeTags = new TagSequence(
                new ISerializableTag[0]
            );

            EntryFunction payload = EntryFunction.Natural(
                new ModuleId(AccountAddress.FromHex("0x3"), "token"),
                "direct_transfer_script",
                typeTags,
                txnArgsSeq
            );

            RawTransaction rawTxn = new RawTransaction(
                senderAccountAddress,
                sequenceNumberInput,
                new TransactionPayload(payload),
                maxGasAmountInput,
                gasUnitPriceInput,
                expirationTimestampsSecsInput,
                chainIdInput
            );

            MultiAgentRawTransaction rawTransactionGenerated = new MultiAgentRawTransaction(
                rawTxn,
                new Sequence(new ISerializable[] { receiverAccountAddress })
            );

            // Test MultiAgentRawTransaction Keyed
            byte[] keyedActual = rawTransactionGenerated.Keyed();
            byte[] keyedExpected = { 94, 250, 60, 79, 2, 248, 58, 15, 75, 45, 105, 252, 149, 198, 7, 204, 2, 130, 92, 196, 231, 190, 83, 110, 240, 153, 45, 240, 80, 217, 230, 124, 0, 125, 238, 204, 177, 8, 8, 84, 244, 153, 236, 139, 76, 27, 33, 59, 130, 197, 227, 75, 146, 92, 246, 135, 95, 236, 2, 212, 183, 122, 219, 210, 214, 11, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 5, 116, 111, 107, 101, 110, 22, 100, 105, 114, 101, 99, 116, 95, 116, 114, 97, 110, 115, 102, 101, 114, 95, 115, 99, 114, 105, 112, 116, 0, 4, 32, 45, 19, 61, 221, 40, 27, 182, 32, 85, 88, 53, 124, 198, 172, 117, 102, 24, 23, 233, 170, 234, 195, 175, 235, 195, 40, 66, 117, 156, 191, 127, 169, 16, 15, 99, 111, 108, 108, 101, 99, 116, 105, 111, 110, 95, 110, 97, 109, 101, 11, 10, 116, 111, 107, 101, 110, 95, 110, 97, 109, 101, 8, 1, 0, 0, 0, 0, 0, 0, 0, 208, 7, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 210, 2, 150, 73, 0, 0, 0, 0, 4, 1, 45, 19, 61, 221, 40, 27, 182, 32, 85, 88, 53, 124, 198, 172, 117, 102, 24, 23, 233, 170, 234, 195, 175, 235, 195, 40, 66, 117, 156, 191, 127, 169 };
            Assert.AreEqual(keyedExpected, keyedActual, ToReadableByteArray(keyedActual));

            Signature senderSignature = rawTransactionGenerated.Sign(senderPrivateKey);
            Signature receiverSignature = rawTransactionGenerated.Sign(receiverPrivateKey);

            bool verifySenderSignature = rawTransactionGenerated.Verify(senderPublicKey, senderSignature);
            Assert.IsTrue(verifySenderSignature);

            bool verifyRecieverSignature = rawTransactionGenerated.Verify(receiverPublicKey, receiverSignature);
            Assert.IsTrue(verifyRecieverSignature);

            // Test ED25519 Authenticator for sender
            Authenticator ed25519AuthSender =
                new Authenticator(
                        new Ed25519Authenticator(
                            senderPublicKey, senderSignature
                        )
                );

            #region Test ED25519 Authenticator serialization for sender
            Serialization ser = new Serialization();
            ed25519AuthSender.Serialize(ser);
            byte[] actualEd25519Sender = ser.GetBytes();
            byte[] expectedEd25519Sender = { 0, 32, 185, 198, 238, 22, 48, 239, 62, 113, 17, 68, 166, 72, 219, 6, 187, 178, 40, 79, 114, 116, 207, 190, 229, 63, 252, 238, 80, 60, 193, 164, 146, 0, 64,   52, 62, 123, 16, 170, 50, 60, 72, 3, 145, 165, 215, 205, 45, 12, 247, 8, 213, 21, 41, 185, 107, 90, 43, 224, 140, 187, 54, 94, 79, 17, 220, 194, 207, 6, 85, 118, 108, 247, 13, 64, 133, 59, 156, 57, 91, 98, 218, 215, 169, 245, 142, 217, 152, 128, 61, 139, 241, 144, 27, 167, 167, 164, 1 };
            Assert.AreEqual(expectedEd25519Sender, actualEd25519Sender, ToReadableByteArray(actualEd25519Sender));
            #endregion

            // Test ED25519 Authenticator for receiver          
            Authenticator ed25519AuthReceiver = 
                new Authenticator(
                    new Ed25519Authenticator(
                        receiverPublicKey, receiverSignature
                    )
            );

            #region Test ED25519 Authenticator serialization for receiver
            ser = new Serialization();
            ed25519AuthReceiver.Serialize(ser);
            byte[] actualEd25519AuthReceiver = ser.GetBytes();
            byte[] expectedEd25519AuthReceiver = { 0, 32, 174, 243, 244, 164, 184, 236, 161, 223, 195, 67, 54, 27, 248, 228, 54, 189, 66, 222, 146, 89, 192, 75, 131, 20, 235, 142, 32, 84, 221, 110, 130, 171, 64, 138, 127, 6, 228, 4, 174, 141, 149, 53, 176, 203, 190, 175, 183, 201, 227, 78, 149, 254, 20, 37, 228, 82, 151, 88, 21, 10, 79, 124, 231, 166, 131, 53, 65, 72, 173, 92, 49, 62, 195, 101, 73, 227, 251, 41, 230, 105, 217, 0, 16, 249, 116, 103, 201, 7, 79, 240, 174, 195, 237, 135, 247, 102, 8 };
            Assert.AreEqual(expectedEd25519AuthReceiver, actualEd25519AuthReceiver, ToReadableByteArray(actualEd25519AuthReceiver));
            #endregion

            List<Tuple<AccountAddress, Authenticator>> secondarySignersTup = new List<Tuple<AccountAddress, Authenticator>>();
            secondarySignersTup.Add(
                new Tuple<AccountAddress, Authenticator>(
                    receiverAccountAddress,
                    ed25519AuthReceiver
                )
            );

            MultiAgentAuthenticator multiAgentAuthenticator = 
                new MultiAgentAuthenticator(
                    ed25519AuthSender,
                    secondarySignersTup
                );

            #region Test MultiAgentAuthenticator serialization
            ser = new Serialization();
            multiAgentAuthenticator.Serialize(ser);
            byte[] actualMultiAgentAuthenticator = ser.GetBytes();
            byte[] expectedMultiAgentAuthenticator = { 0, 32, 185, 198, 238, 22, 48, 239, 62, 113, 17, 68, 166, 72, 219, 6, 187, 178, 40, 79, 114, 116, 207, 190, 229, 63, 252, 238, 80, 60, 193, 164, 146, 0, 64, 52, 62, 123, 16, 170, 50, 60, 72, 3, 145, 165, 215, 205, 45, 12, 247, 8, 213, 21, 41, 185, 107, 90, 43, 224, 140, 187, 54, 94, 79, 17, 220, 194, 207, 6, 85, 118, 108, 247, 13, 64, 133, 59, 156, 57, 91, 98, 218, 215, 169, 245, 142, 217, 152, 128, 61, 139, 241, 144, 27, 167, 167, 164, 1, 1,     45, 19, 61, 221, 40, 27, 182, 32, 85, 88, 53, 124, 198, 172, 117, 102, 24, 23, 233, 170, 234, 195, 175, 235, 195, 40, 66, 117, 156, 191, 127, 169, 1,     0, 32, 174, 243, 244, 164, 184, 236, 161, 223, 195, 67, 54, 27, 248, 228, 54, 189, 66, 222, 146, 89, 192, 75, 131, 20, 235, 142, 32, 84, 221, 110, 130, 171, 64, 138, 127, 6, 228, 4, 174, 141, 149, 53, 176, 203, 190, 175, 183, 201, 227, 78, 149, 254, 20, 37, 228, 82, 151, 88, 21, 10, 79, 124, 231, 166, 131, 53, 65, 72, 173, 92, 49, 62, 195, 101, 73, 227, 251, 41, 230, 105, 217, 0, 16, 249, 116, 103, 201, 7, 79, 240, 174, 195, 237, 135, 247, 102, 8 };
            Assert.AreEqual(expectedMultiAgentAuthenticator, actualMultiAgentAuthenticator, ToReadableByteArray(actualMultiAgentAuthenticator));
            #endregion

            // Test full MultiAgentAuthenticator serialization
            Authenticator authenticator = new Authenticator(
                multiAgentAuthenticator
            );

            #region Test serialization of nested Authenticator containing MultiAgentAuthenticator
            ser = new Serialization();
            authenticator.Serialize(ser);
            byte[] actualAuthenticator = ser.GetBytes();
            byte[] expectedAuthenticator = { 2, 0, 32, 185, 198, 238, 22, 48, 239, 62, 113, 17, 68, 166, 72, 219, 6, 187, 178, 40, 79, 114, 116, 207, 190, 229, 63, 252, 238, 80, 60, 193, 164, 146, 0, 64, 52, 62, 123, 16, 170, 50, 60, 72, 3, 145, 165, 215, 205, 45, 12, 247, 8, 213, 21, 41, 185, 107, 90, 43, 224, 140, 187, 54, 94, 79, 17, 220, 194, 207, 6, 85, 118, 108, 247, 13, 64, 133, 59, 156, 57, 91, 98, 218, 215, 169, 245, 142, 217, 152, 128, 61, 139, 241, 144, 27, 167, 167, 164, 1, 1, 45, 19, 61, 221, 40, 27, 182, 32, 85, 88, 53, 124, 198, 172, 117, 102, 24, 23, 233, 170, 234, 195, 175, 235, 195, 40, 66, 117, 156, 191, 127, 169, 1, 0, 32, 174, 243, 244, 164, 184, 236, 161, 223, 195, 67, 54, 27, 248, 228, 54, 189, 66, 222, 146, 89, 192, 75, 131, 20, 235, 142, 32, 84, 221, 110, 130, 171, 64, 138, 127, 6, 228, 4, 174, 141, 149, 53, 176, 203, 190, 175, 183, 201, 227, 78, 149, 254, 20, 37, 228, 82, 151, 88, 21, 10, 79, 124, 231, 166, 131, 53, 65, 72, 173, 92, 49, 62, 195, 101, 73, 227, 251, 41, 230, 105, 217, 0, 16, 249, 116, 103, 201, 7, 79, 240, 174, 195, 237, 135, 247, 102, 8 };
            Assert.AreEqual(expectedAuthenticator, actualAuthenticator, ToReadableByteArray(actualAuthenticator));

            Deserialization deser = new Deserialization(actualAuthenticator);
            Authenticator authenticatorDeserialized = (Authenticator)Authenticator.Deserialize(deser);
            Assert.AreEqual(authenticator.GetVariant(), authenticatorDeserialized.GetVariant());

            //string internals = ((Authenticator.MultiAgentAuthenticator) authenticator.GetAuthenticator()).sender.ToString() + " ::: " + ((Authenticator.MultiAgentAuthenticator) authenticatorDeserialized.GetAuthenticator()).sender.ToString();
            //List<Tuple<AccountAddress, Authenticator.Authenticator>> list1 = ((Authenticator.MultiAgentAuthenticator)authenticator.GetAuthenticator()).secondarySigners;
            //string result1 = string.Join(",", list1.Select(t => string.Format("[ '{0}', '{1}']", t.Item1, t.Item2)));

            //List<Tuple<AccountAddress, Authenticator.Authenticator>> list2 = ((Authenticator.MultiAgentAuthenticator)authenticatorDeserialized.GetAuthenticator()).secondarySigners;
            //string result2 = string.Join(",", list1.Select(t => string.Format("[ '{0}', '{1}']", t.Item1, t.Item2)));

            //Assert.AreEqual(authenticator, authenticatorDeserialized, result1 + "\n" + result2);

            Assert.AreEqual(authenticator, authenticatorDeserialized, authenticatorDeserialized.ToString());

            #endregion

            SignedTransaction signedTransactionGenerated = new SignedTransaction(
                rawTransactionGenerated.Inner(), authenticator
            );

            #region Test SignedTransaction serialization
            ser = new Serialization();
            signedTransactionGenerated.Serialize(ser);
            byte[] signedTxnActual = ser.GetBytes();
            byte[] signedTxnExpected = { 125, 238, 204, 177, 8, 8, 84, 244, 153, 236, 139, 76, 27, 33, 59, 130, 197, 227, 75, 146, 92, 246, 135, 95, 236, 2, 212, 183, 122, 219, 210, 214, 11, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 5, 116, 111, 107, 101, 110, 22, 100, 105, 114, 101, 99, 116, 95, 116, 114, 97, 110, 115, 102, 101, 114, 95, 115, 99, 114, 105, 112, 116, 0, 4, 32, 45, 19, 61, 221, 40, 27, 182, 32, 85, 88, 53, 124, 198, 172, 117, 102, 24, 23, 233, 170, 234, 195, 175, 235, 195, 40, 66, 117, 156, 191, 127, 169, 16, 15, 99, 111, 108, 108, 101, 99, 116, 105, 111, 110, 95, 110, 97, 109, 101, 11, 10, 116, 111, 107, 101, 110, 95, 110, 97, 109, 101, 8, 1, 0, 0, 0, 0, 0, 0, 0, 208, 7, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 210, 2, 150, 73, 0, 0, 0, 0, 4, 2, 0, 32, 185, 198, 238, 22, 48, 239, 62, 113, 17, 68, 166, 72, 219, 6, 187, 178, 40, 79, 114, 116, 207, 190, 229, 63, 252, 238, 80, 60, 193, 164, 146, 0, 64, 52, 62, 123, 16, 170, 50, 60, 72, 3, 145, 165, 215, 205, 45, 12, 247, 8, 213, 21, 41, 185, 107, 90, 43, 224, 140, 187, 54, 94, 79, 17, 220, 194, 207, 6, 85, 118, 108, 247, 13, 64, 133, 59, 156, 57, 91, 98, 218, 215, 169, 245, 142, 217, 152, 128, 61, 139, 241, 144, 27, 167, 167, 164, 1, 1, 45, 19, 61, 221, 40, 27, 182, 32, 85, 88, 53, 124, 198, 172, 117, 102, 24, 23, 233, 170, 234, 195, 175, 235, 195, 40, 66, 117, 156, 191, 127, 169, 1, 0, 32, 174, 243, 244, 164, 184, 236, 161, 223, 195, 67, 54, 27, 248, 228, 54, 189, 66, 222, 146, 89, 192, 75, 131, 20, 235, 142, 32, 84, 221, 110, 130, 171, 64, 138, 127, 6, 228, 4, 174, 141, 149, 53, 176, 203, 190, 175, 183, 201, 227, 78, 149, 254, 20, 37, 228, 82, 151, 88, 21, 10, 79, 124, 231, 166, 131, 53, 65, 72, 173, 92, 49, 62, 195, 101, 73, 227, 251, 41, 230, 105, 217, 0, 16, 249, 116, 103, 201, 7, 79, 240, 174, 195, 237, 135, 247, 102, 8 };
            Assert.AreEqual(signedTxnExpected, signedTxnActual, ToReadableByteArray(signedTxnActual));
            #endregion

            Assert.IsTrue(signedTransactionGenerated.Verify());

            string rawTransactionInput = "7deeccb1080854f499ec8b4c1b213b82c5e34b925cf6875fec02d4b77adbd2d60b0000000000000002000000000000000000000000000000000000000000000000000000000000000305746f6b656e166469726563745f7472616e736665725f7363726970740004202d133ddd281bb6205558357cc6ac75661817e9aaeac3afebc32842759cbf7fa9100f636f6c6c656374696f6e5f6e616d650b0a746f6b656e5f6e616d65080100000000000000d0070000000000000100000000000000d20296490000000004";
            string signedTransactionInput = "7deeccb1080854f499ec8b4c1b213b82c5e34b925cf6875fec02d4b77adbd2d60b0000000000000002000000000000000000000000000000000000000000000000000000000000000305746f6b656e166469726563745f7472616e736665725f7363726970740004202d133ddd281bb6205558357cc6ac75661817e9aaeac3afebc32842759cbf7fa9100f636f6c6c656374696f6e5f6e616d650b0a746f6b656e5f6e616d65080100000000000000d0070000000000000100000000000000d20296490000000004020020b9c6ee1630ef3e711144a648db06bbb2284f7274cfbee53ffcee503cc1a4920040343e7b10aa323c480391a5d7cd2d0cf708d51529b96b5a2be08cbb365e4f11dcc2cf0655766cf70d40853b9c395b62dad7a9f58ed998803d8bf1901ba7a7a401012d133ddd281bb6205558357cc6ac75661817e9aaeac3afebc32842759cbf7fa9010020aef3f4a4b8eca1dfc343361bf8e436bd42de9259c04b8314eb8e2054dd6e82ab408a7f06e404ae8d9535b0cbbeafb7c9e34e95fe1425e4529758150a4f7ce7a683354148ad5c313ec36549e3fb29e669d90010f97467c9074ff0aec3ed87f76608";

            VerifyTransactions(
                rawTransactionInput,
                rawTransactionGenerated.Inner(),
                signedTransactionInput,
                signedTransactionGenerated
            );
        }

        /// <summary>
        /// Verifies serialization works for RawTransaction and SignedTransaction
        /// </summary>
        /// <param name="rawTransactionInput"></param>
        /// <param name="rawTransactionGenerated"></param>
        /// <param name="signedTransactionInput"></param>
        /// <param name="signedTransactionGenerated"></param>
        public static void VerifyTransactions(
            string rawTransactionInput, 
            RawTransaction rawTransactionGenerated, 
            string signedTransactionInput,
            SignedTransaction signedTransactionGenerated
        )
        {
            // Produce serialized generated transactions
            Serialization ser = new Serialization();
            ser.Serialize(rawTransactionGenerated);
            string rawTransactionGeneratedBytes = ser.GetBytes().HexString();

            ser = new Serialization();
            ser.Serialize(signedTransactionGenerated);
            string signedTransactionGeneratedBytes = ser.GetBytes().HexString();

            // Verify the RawTransaction
            Assert.AreEqual(rawTransactionInput, rawTransactionGeneratedBytes, rawTransactionGeneratedBytes);
            RawTransaction rawTransaction = RawTransaction.Deserialize(
                new Deserialization(rawTransactionInput.ByteArrayFromHexString())
            );
            Assert.AreEqual(rawTransactionGenerated, rawTransaction);

            // Verify the SignedTransaction
            Assert.AreEqual(signedTransactionInput, signedTransactionGeneratedBytes, signedTransactionGeneratedBytes);
            SignedTransaction signedTransaction = SignedTransaction.Deserialize(
                new Deserialization(signedTransactionInput.ByteArrayFromHexString())
            );

            Assert.AreEqual(signedTransaction.transaction, rawTransaction);
            Assert.AreEqual(signedTransaction, signedTransactionGenerated);
            Assert.IsTrue(signedTransaction.Verify());
        }

        /// <summary>
        /// Utility function to print out byte array
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        static public string ToReadableByteArray(byte[] bytes)
        {
            return string.Join(", ", bytes);
        }
    }
}