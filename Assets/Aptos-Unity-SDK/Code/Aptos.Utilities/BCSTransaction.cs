using Aptos.Accounts;

namespace Aptos.Utilities.BCS
{
    /// <summary>
    /// Representation of a Module ID.
    /// </summary>
    public class ModuleId : ISerializable
    {
        public AccountAddress address;
        public string name;

        public ModuleId(AccountAddress address, string name)
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

    /// <summary>
    /// Representation of EntryFunction.
    /// </summary>
    public class EntryFunction : ISerializable
    {
        readonly ModuleId module;
        readonly string function;
        readonly TagSequence typeArgs;
        readonly Sequence args;
        //readonly ISerializable args;

        public EntryFunction(ModuleId module, string function, TagSequence typeArgs, Sequence args)
        //public EntryFunction(ModuleId module, string function, TagSequence typeArgs, ISerializable args)
        {
            this.module = module;
            this.function = function;
            this.typeArgs = typeArgs;
            this.args = args;
        }

        /// TODO: Complete EntryFunction Natural
        /// TODO: Get Sample Python ouutput for serialization of Natural
        //public static EntryFunction Natural(string module, string function, TagSequence typeArgs, Sequence args)
        //{
        //    string moduleId = module;

        //    Serialization ser = new Serialization();
        //    ISerializable[] argsValues = args.GetValues();
        //    ser.Serialize(argsValues);

        //    return null;
        //}

        public static EntryFunction Natural(ModuleId module, string function, TagSequence typeArgs, Sequence args)
        {
            //Serialization ser = new Serialization();

            //byte[,] bytes = new byte[1, args.Length];
            //ISerializable[] argsArr = args.GetValues();

            //for (int i = 0; i < args.Length; i++)
            //{
            //    //argsArr[i].Serialize(ser);
            //    //bytes[i][0] =
            //    ser.Serialize(argsArr[i]);
            //}

            //BytesSingleSequence bytesArgsSeq = new BytesSingleSequence(ser.GetBytes());

            //return new EntryFunction(module, function, typeArgs, bytesArgsSeq);

            return new EntryFunction(module, function, typeArgs, args);
        }

        public void Serialize(Serialization serializer)
        {
            //this.module.Serialize(serializer);
            //serializer.SerializeString(this.function);

            serializer.Serialize(this.module);
            serializer.SerializeString(this.function);
            //serializer.SerializeBytes(System.Text.Encoding.UTF8.GetBytes(this.function));

            //this.typeArgs.Serialize(serializer);
            //this.args.Serialize(serializer);
            ////serializer.SerializeU32AsUleb128((uint)this.args.Length);
            ////for (int i = 0; i < this.args.Length; i++)
            ////{
            ////    serializer.Serialize(this.args[i]);
            ////}
            ///
            serializer.Serialize(this.typeArgs);
            ////serializer.Serialize(this.args);

            //serializer.SerializeU32AsUleb128((uint)this.args.Length);
            //ISerializable[] values = this.args.GetValues();
            //for (int i = 0; i < values.Length; i++)
            //{
            //    serializer.Serialize(values[i]);
            //}

            args.Serialize(serializer);
        }
    }
}
