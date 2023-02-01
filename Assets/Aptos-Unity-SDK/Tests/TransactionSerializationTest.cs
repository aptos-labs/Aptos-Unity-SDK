using NUnit.Framework;
using Aptos.Utilities.BCS;

namespace Aptos.Unity.Test
{
    public class TransactionSerializationTest
    {
        internal AccountAddress TestAddress()
        {
            return new AccountAddress("0x01");
        }

        internal ModuleId TestModuleId()
        {
            return new ModuleId(TestAddress(), "my_module");
        }

        internal EntryFunction TestEntryFunction(ISerializableTag[] typeTags, ISerializable[] args)
        {
            return new EntryFunction(TestModuleId(), "some_function", new TagSequence(typeTags), new Sequence(args));
        }

        #region General transaction tests
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
            Assert.AreEqual(new byte[] { 1, 1, 0 }, res);

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
            args = new Sequence(new ISerializable[]
            {
                new Sequence(new ISerializable[0] )
            });
            args.Serialize(s);
            Assert.AreEqual(new byte[] { 1, 1, 0 }, s.GetBytes());


            //s = new Serialization();
            //s.SerializeU32AsUleb128()
            //BytesSequence args2 = new BytesSequence(new[]
            //{

            //   Serialization.SerializeOne(Serialization.SerializeOne("A")),
            //});

            //args2.Serialize(s);
            //Assert.AreEqual(new byte[] { 1, 3, 1, 1, 65 }, s.GetBytes());
        }

        [Test]
        public void SerializeTransactionWithSingleStringArg()
        {
            Serialization s = new Serialization();
            ISerializable[] args =
            {
                new BString("wow"),
            };
            TestEntryFunction(new ISerializableTag[0], args).Serialize(s);
            byte[] res = s.GetBytes();
            Assert.AreEqual(new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 9, 109, 121, 95, 109, 111, 100, 117, 108, 101, 13, 115, 111, 109, 101, 95, 102, 117, 110, 99, 116, 105, 111, 110, 0, 1, 4, 3, 119, 111, 119
            }, res, ToReadableByteArray(res));
        }

        [Test]
        public void SerializeTransactionWithSingleU64Arg()
        {
            Serialization s = new Serialization();
            ISerializable[] args =
            {
                new U64(555555),
            };
            TestEntryFunction(new ISerializableTag[0], args).Serialize(s);
            byte[] res = s.GetBytes();
            Assert.AreEqual(new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1,
                9, 109, 121, 95, 109, 111, 100, 117, 108, 101, 13, 115, 111, 109, 101, 95,
                102, 117, 110, 99, 116, 105, 111, 110, 0, 1, 8, 35, 122, 8, 0, 0, 0, 0, 0
            }, res, ToReadableByteArray(res));
        }

        #endregion

        #region Transaction with bool sequence

        /// <summary>
        /// Tests a transaction where the arguments sequence contains an empty boolean sequence
        /// 
        /// Python Example:
        /// txnBoolArg = TransactionArgument( [], Serializer.sequence_serializer(Serializer.bool) ).encode()
        /// txn = EntryFunction(mod, "some_function", [], [txnBoolArg])
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
            byte[] res = s.GetBytes();
            Assert.AreEqual(new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 9, 109, 121, 95, 109, 111, 100, 117, 108, 101, 13, 115, 111, 109, 101, 95, 102, 117, 110, 99, 116, 105, 111, 110, 0, 1, 1, 0
            }, res, ToReadableByteArray(res));
        }

        [Test]
        public void SerializeTransactionWithOneBoolArgSequence()
        {
            Serialization s = new Serialization();
            ISerializable[] args =
            {
                new Sequence(new[] { new Bool(false) }),
            };
            TestEntryFunction(new ISerializableTag[0], args).Serialize(s);
            byte[] res = s.GetBytes();
            Assert.AreEqual(new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 9, 109, 121, 95, 109, 111, 100, 117, 108, 101, 13, 115, 111, 109, 101, 95, 102, 117, 110, 99, 116, 105, 111, 110, 0, 1, 2, 1, 0
            }, res, ToReadableByteArray(res));
        }

        [Test]
        public void SerializeTransactionWithTwoBoolSequence()
        {
            Serialization s = new Serialization();
            ISerializable[] args =
            {
                new Sequence(new[] { new Bool(false), new Bool(true)}),
            };
            TestEntryFunction(new ISerializableTag[0], args).Serialize(s);
            byte[] res = s.GetBytes();
            Assert.AreEqual(new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 9, 109, 121, 95, 109, 111, 100, 117, 108, 101, 13, 115, 111, 109, 101, 95, 102, 117, 110, 99, 116, 105, 111, 110, 0, 1, 3, 2, 0, 1
            }, res, ToReadableByteArray(res));
        }

        [Test]
        public void SerializeTransactionWithThreeBoolArgsSequence()
        {
            Serialization s = new Serialization();
            ISerializable[] args =
            {
                new Sequence(new[] { new Bool(false), new Bool(true), new Bool(false) }),
            };
            TestEntryFunction(new ISerializableTag[0], args).Serialize(s);
            byte[] res = s.GetBytes();
            Assert.AreEqual(new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 9, 109, 121, 95, 109, 111, 100, 117, 108, 101, 13, 115, 111, 109, 101, 95, 102, 117, 110, 99, 116, 105, 111, 110, 0, 1, 4, 3, 0, 1, 0
            }, res, ToReadableByteArray(res));
        }

        #endregion

        #region Transaction with string sequence
        [Test]
        public void SerializeTransactionWithOneStringArgSequence()
        {
            Serialization s = new Serialization();
            ISerializable[] args =
            {
                new Sequence(new[] { new BString("A") }),
            };
            TestEntryFunction(new ISerializableTag[0], args).Serialize(s);
            byte[] res = s.GetBytes();
            Assert.AreEqual(new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 9, 109, 121, 95, 109, 111, 100, 117, 108, 101, 13, 115, 111, 109, 101, 95, 102, 117, 110, 99, 116, 105, 111, 110, 0, 1, 3, 1, 1, 65
            }, res, ToReadableByteArray(res));
        }

        [Test]
        public void SerializeTransactionWithTwoStringArgSequence()
        {
            Serialization s = new Serialization();
            ISerializable[] args =
            {
                new Sequence(new[] { new BString("A"), new BString("B") }),
            };
            TestEntryFunction(new ISerializableTag[0], args).Serialize(s);
            byte[] res = s.GetBytes();
            Assert.AreEqual(new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 9, 109, 121, 95, 109, 111, 100, 117, 108, 101, 13, 115, 111, 109, 101, 95, 102, 117, 110, 99, 116, 105, 111, 110, 0, 1, 5, 2, 1, 65, 1, 66
            }, res, ToReadableByteArray(res));
        }

        [Test]
        public void SerializeTransactionWithThreeStringArgSequence()
        {
            Serialization s = new Serialization();
            ISerializable[] args =
            {
                new Sequence(new[] { new BString("A"), new BString("B"), new BString("C") }),
            };
            TestEntryFunction(new ISerializableTag[0], args).Serialize(s);
            byte[] res = s.GetBytes();
            Assert.AreEqual(new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 9, 109, 121, 95, 109, 111, 100, 117, 108, 101, 13, 115, 111, 109, 101, 95, 102, 117, 110, 99, 116, 105, 111, 110, 0, 1, 7, 3, 1, 65, 1, 66, 1, 67
            }, res, ToReadableByteArray(res));
        }

        #endregion

        #region Transaction with multiple single-type args and sequence args
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
            byte[] res = s.GetBytes();
            Assert.AreEqual(new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 9, 109, 121, 95, 109, 111, 100, 117, 108, 101, 13, 115, 111, 109, 101, 95, 102, 117, 110, 99, 116, 105, 111, 110, 0, 2, 2, 1, 65, 2, 1, 0
            }, res, ToReadableByteArray(res));
        }

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
            byte[] res = s.GetBytes();
            Assert.AreEqual(new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 9, 109, 121, 95, 109, 111, 100, 117, 108, 101, 13, 115, 111, 109, 101, 95, 102, 117, 110, 99, 116, 105, 111, 110, 0, 3, 2, 1, 65, 8, 1, 0, 0, 0, 0, 0, 0, 0, 2, 1, 0
            }, res, ToReadableByteArray(res));
        }

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
            byte[] res = s.GetBytes();
            Assert.AreEqual(new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 9, 109, 121, 95, 109, 111, 100, 117, 108, 101, 13, 115, 111, 109, 101, 95, 102, 117, 110, 99, 116, 105, 111, 110, 0, 4, 2, 1, 65, 8, 1, 0, 0, 0, 0, 0, 0, 0, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 1, 0
            }, res, ToReadableByteArray(res));
        }

        /// <summary>
        /// Python Example:
        /// 
        /// addr = AccountAddress.from_hex("0x01")
        /// txn = EntryFunction(mid, "some_function", [], [
        ///     TransactionArgument("A", Serializer.str).encode(),
        ///     TransactionArgument(1, Serializer.u64).encode(),
        ///     TransactionArgument(addr, Serializer.struct).encode(),
        ///     TransactionArgument([False], Serializer.sequence_serializer(Serializer.bool)).encode(),
        ///     TransactionArgument([False, True], Serializer.sequence_serializer(Serializer.bool)).encode(),
        ///     TransactionArgument(["A", "B", "C"], Serializer.sequence_serializer(Serializer.str)).encode()
        /// ])
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
            byte[] res = s.GetBytes();
            Assert.AreEqual(new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 9, 109, 121, 95, 109, 111, 100, 117, 108, 101, 13, 115, 111, 109, 101, 95, 102, 117, 110, 99, 116, 105, 111, 110, 0, 6, 2, 1, 65, 8, 1, 0, 0, 0, 0, 0, 0, 0, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 1, 0, 3, 2, 0, 1, 7, 3, 1, 65, 1, 66, 1, 67
            }, res, ToReadableByteArray(res));
        }

        #endregion

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