using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Aptos.Utilities.BCS;
using System.Numerics;

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

    [Test]
    public void SerializeSimpleTransaction()
    {
        Serialization s = new Serialization();
        TestEntryFunction(new ISerializableTag[0], new ISerializable[0]).Serialize(s);
        byte[] res = s.GetBytes();
        Assert.AreEqual(
            new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 9, 109,
                121, 95, 109, 111, 100, 117, 108, 101, 13, 115, 111, 109, 101, 95, 102, 117, 110, 99, 116, 105, 111,
                110, 0, 0
            }, res);
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

        /*
        s = new Serialization();
        args = new Sequence(new ISerializable[]
        {
            new Sequence(new ISerializable[0] )
        });
        args.Serialize(s);
        Assert.AreEqual(new byte[] { 1, 1, 0 }, s.GetBytes());
        

        s = new Serialization();
        s.SerializeU32AsUleb128()
        BytesSequence args2 = new BytesSequence(new[]
        {

           Serialization.SerializeOne(Serialization.SerializeOne("A")),
        });
       
        args2.Serialize(s);
        Assert.AreEqual(new byte[] { 1, 3, 1, 1, 65 }, s.GetBytes());
        */
    }

    [Test]
    public void SerializeTransactionWithArgs()
    {
        Serialization s = new Serialization();
        ISerializable[] args =
        {
            //new BString("wow"),
            //new U64(555555),
            //TestAddress(),
            new Sequence(new[] { new Bool(false), new Bool(true), new Bool(false) }),
        };
        TestEntryFunction(new ISerializableTag[0], args).Serialize(s);
        byte[] res = s.GetBytes();
        Assert.AreEqual(new byte[]
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 9, 109, 121,
            95, 109, 111, 100, 117, 108, 101, 13, 115, 111, 109, 101, 95, 102, 117, 110, 99, 116, 105, 111, 110, 0, 1,
            4, 3, 0, 1, 0
        }, res);
    }
}