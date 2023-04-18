using NUnit.Framework;
using Aptos.Utilities.BCS;
using Aptos.Accounts;

namespace Aptos.Unity.Test
{
    public class TransactionSerializationTest
    {
        //internal Utilities.BCS.AccountAddress TestAddress()
        //{
        //    return new Utilities.BCS.AccountAddress("0x01");
        //}

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
            return new EntryFunction(TestModuleId(), "some_function", new TagSequence(typeTags), new Sequence(args));
        }

        [Test]
        public void SerializeAddress()
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
        public void SerializeModuleId()
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
        public void SerializeSimpleTransaction()
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
        public void SerializeTransactionWithEmptyArgSequence()
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
        public void SerializeSequenceString()
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
        public void SerializeSequenceSequenceBool()
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
        public void SerializeTransactionWithSingleStringArg()
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
        public void SerializeTransactionWithSingleU64Arg()
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
        public void SerializeTransactionWithEmptyBoolArgSequence()
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
        public void SerializeTransactionWithOneBoolArgSequence()
        {
            Serialization s = new Serialization();
            ISerializable[] args =
            {
                new Sequence(new[] { new Bool(false) }),
            };
            TestEntryFunction(new ISerializableTag[0], args).Serialize(s);
            byte[] actual = s.GetBytes();
            byte[] expected = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 9, 109, 121, 95, 109, 111, 100, 117, 108, 101, 13, 115, 111, 109, 101, 95, 102, 117, 110, 99, 116, 105, 111, 110, 0, 1, 2, 1, 0 };
            Assert.AreEqual(expected, actual, ToReadableByteArray(actual));
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
        public void SerializeTransactionWithTwoBoolSequence()
        {
            Serialization s = new Serialization();
            ISerializable[] args =
            {
                new Sequence(new[] { new Bool(false), new Bool(true)}),
            };
            TestEntryFunction(new ISerializableTag[0], args).Serialize(s);
            byte[] actual = s.GetBytes();
            byte[] expected = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 9, 109, 121, 95, 109, 111, 100, 117, 108, 101, 13, 115, 111, 109, 101, 95, 102, 117, 110, 99, 116, 105, 111, 110, 0, 1, 3, 2, 0, 1 };

            Assert.AreEqual(expected, actual, ToReadableByteArray(actual));
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
        public void SerializeTransactionWithThreeBoolArgsSequence()
        {
            Serialization s = new Serialization();
            ISerializable[] args =
            {
                new Sequence(new[] { new Bool(false), new Bool(true), new Bool(false) }),
            };
            TestEntryFunction(new ISerializableTag[0], args).Serialize(s);
            byte[] actual = s.GetBytes();
            byte[] expected = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 9, 109, 121, 95, 109, 111, 100, 117, 108, 101, 13, 115, 111, 109, 101, 95, 102, 117, 110, 99, 116, 105, 111, 110, 0, 1, 4, 3, 0, 1, 0 };
            Assert.AreEqual(expected, actual, ToReadableByteArray(actual));
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
        public void SerializeTransactionWithOneStringArgSequence()
        {
            Serialization s = new Serialization();
            ISerializable[] args =
            {
                new Sequence(new[] { new BString("A") }),
            };
            TestEntryFunction(new ISerializableTag[0], args).Serialize(s);
            byte[] actual = s.GetBytes();
            byte[] expected = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 9, 109, 121, 95, 109, 111, 100, 117, 108, 101, 13, 115, 111, 109, 101, 95, 102, 117, 110, 99, 116, 105, 111, 110, 0, 1, 3, 1, 1, 65 };
            Assert.AreEqual(expected, actual, ToReadableByteArray(actual));
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
        public void SerializeTransactionWithTwoStringArgSequence()
        {
            Serialization s = new Serialization();
            ISerializable[] args =
            {
                new Sequence(new[] { new BString("A"), new BString("B") }),
            };
            TestEntryFunction(new ISerializableTag[0], args).Serialize(s);
            byte[] actual = s.GetBytes();
            byte[] expected = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 9, 109, 121, 95, 109, 111, 100, 117, 108, 101, 13, 115, 111, 109, 101, 95, 102, 117, 110, 99, 116, 105, 111, 110, 0, 1, 5, 2, 1, 65, 1, 66 };

            Assert.AreEqual(expected, actual, ToReadableByteArray(actual));
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
        public void SerializeTransactionWithThreeStringArgSequence()
        {
            Serialization s = new Serialization();
            ISerializable[] args =
            {
                new Sequence(new[] { new BString("A"), new BString("B"), new BString("C") }),
            };
            TestEntryFunction(new ISerializableTag[0], args).Serialize(s);
            byte[] actual = s.GetBytes();
            byte[] expected = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 9, 109, 121, 95, 109, 111, 100, 117, 108, 101, 13, 115, 111, 109, 101, 95, 102, 117, 110, 99, 116, 105, 111, 110, 0, 1, 7, 3, 1, 65, 1, 66, 1, 67 };

            Assert.AreEqual(expected, actual, ToReadableByteArray(actual));
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
        public void SerializeTransactionWithOneStringOneBoolArgSequence()
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

            Assert.AreEqual(expected, actual, ToReadableByteArray(actual));
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
        public void SerializeTransactionWithOneStringOneIntOneBoolArgSequence()
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
            Assert.AreEqual(expected, actual, ToReadableByteArray(actual));
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
        public void SerializeTransactionWithOneStringOneIntOneAddressOneBoolArgSequence()
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

            Assert.AreEqual(expected, actual, ToReadableByteArray(actual));
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
        public void SerializeTransactionWithOneStringOneIntOneAddressMultipleArgSequences()
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
            Assert.AreEqual(expected, actual, ToReadableByteArray(actual));
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
        public void SerializeEntryFunctionPayloadForTransferCoin()
        {
            //Account alice = Account.LoadKey("0x64f57603b58af16907c18a866123286e1cbce89790613558dc1775abb3fc5c8c");
            //string acctAddressAlice = alice.AccountAddress.ToString();

            //Account bob = Account.LoadKey("0xb10d4b38bef8a0d3e8747a84cfdc764b89a528af98e055e0237b11863afa9825");
            //string acctAddressBob = bob.AccountAddress.ToString();

            //TestEntryFunction(new ISerializableTag[0], args).Serialize(s);

            TagSequence typeTags = new TagSequence(
                new ISerializableTag[] { 
                    //new StructTag(new Utilities.BCS.AccountAddress("0x1"), "aptos_coin", "AptosCoin", new ISerializableTag[0])
                    new StructTag(AccountAddress.FromHex("0x1"), "aptos_coin", "AptosCoin", new ISerializableTag[0])
                }
            );

            ISerializable[] args =
            {
                //new Utilities.BCS.AccountAddress(acctAddressBob),
                //AccountAddress.FromHex(acctAddressBob),
                AccountAddress.FromHex("0x1"),
                new U64(1000),
            };

            Sequence txnArgs = new Sequence(args);

            //EntryFunction payload =  new EntryFunction(
            //    //new ModuleId(new Utilities.BCS.AccountAddress("0x1"), "coin"),
            //    new ModuleId(AccountAddress.FromHex("0x1"), "coin"),
            //    "tranfer",
            //    typeTags,
            //    txnArgs
            //);

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
            //byte[] expected = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 4, 99, 111, 105, 110, 8, 116, 114, 97, 110, 115, 102, 101, 114, 1, 7, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 10, 97, 112, 116, 111, 115, 95, 99, 111, 105, 110, 9, 65, 112, 116, 111, 115, 67, 111, 105, 110, 0, 2, 32, 216, 159, 215, 62, 247, 192, 88, 204, 247, 159, 228, 193, 195, 133, 7, 213, 128, 53, 66, 6, 163, 106, 224, 62, 234, 1, 221, 253, 58, 254, 239, 7, 8, 232, 3, 0, 0, 0, 0, 0, 0 };
            Assert.AreEqual(expected, actual, ToReadableByteArray(actual));
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
        /// txnPayload = TransactionPayload(payload)
        /// 
        /// ser = Serializer()
        /// txnPayload.serialize(ser)
        /// out = ser.output()
        /// 
        /// print([x for x in out])
        /// </summary>
        [Test]
        public void SerializeTransactionPayloadForTransferCoin()
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
        /// </summary>
        [Test]
        public void SerializeRawTransactionForTransferCoin()
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
        }

        [Test]
        public void PrehashRawTransactionForTransferCoin()
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
        public void KeyedRawTransactionForTransferCoin()
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