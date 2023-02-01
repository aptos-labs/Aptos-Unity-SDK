using System;
using Aptos.Utilities.BCS;


namespace Aptos.Utilities.BCS
{

    public class ModuleId : ISerializable
    {
        public AccountAddress address;
        public String name;

        public ModuleId(AccountAddress address, String name)
        {
            // TODO: assert on length of an address- and make it it's own type
            this.address = address;
            this.name = name;
        }

        public void Serialize(Serialization serializer)
        {
            this.address.Serialize(serializer);
            serializer.SerializeString(this.name);
        }
    }

    public class EntryFunction : ISerializable
    {
        ModuleId module;
        String function;
        TagSequence typeArgs;
        Sequence args;

        public EntryFunction(ModuleId module, String function, TagSequence typeArgs, Sequence args)
        {
            this.module = module;
            this.function = function;
            this.typeArgs = typeArgs;
            this.args = args;
        }

        public void Serialize(Serialization serializer)
        {
            this.module.Serialize(serializer);
            serializer.SerializeString(this.function);
            this.typeArgs.Serialize(serializer);
            this.args.Serialize(serializer);
            //serializer.SerializeU32AsUleb128((uint)this.args.Length);
            //for (int i = 0; i < this.args.Length; i++)
            //{
            //    serializer.Serialize(this.args[i]);
            //}
        }
    }
}
