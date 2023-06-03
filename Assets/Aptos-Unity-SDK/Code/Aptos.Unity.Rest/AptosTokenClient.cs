using Aptos.Accounts;
using Aptos.BCS;
using Aptos.Unity.Rest.Model;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Aptos.Unity.Rest
{
    /// <summary>
    /// Interface implemented by all resource types.
    /// Necessary to create Dictionaries or Lists of different resources.
    /// </summary>
    public interface IResource
    {
        public string GetStructTag();
        public static IResource Parse(string resourceJson) => throw new NotImplementedException();
    }

    #region Object Class
    /// <summary>
    /// Represents an Object resource.
    /// </summary>
    class Object : IResource
    {
        bool AllowUngatedTransfer;
        AccountAddress Owner;

        public string StructTag = "0x1::object::ObjectCore";

        public Object(bool AllowUngatedTransfer, AccountAddress Owner)
        {
            this.AllowUngatedTransfer = AllowUngatedTransfer;
            this.Owner = Owner;
        }

        /// <summary>
        /// Parses a JSON string representation of a resource into an Object.
        /// </summary>
        /// <param name="resourceJson"></param>
        /// <returns></returns>
        // TODO: Implement Parse for Object class
        public IResource Parse(string resourceJson)
        {
            //ObjectResource resourceObj = JsonConvert.DeserializeObject<ObjectResource>(resourceJson, new ObjectResourceConverter());

            //return new Object(
            //    resourceObj.AllowUngatedTransfer,
            //    AccountAddress.FromHex(resourceObj.Owner)
            //);
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return string.Format("Object[allow_ungated_transfer: {0}, owner: {1}]", this.AllowUngatedTransfer, this.Owner);
        }

        public string GetStructTag()
        {
            return StructTag;
        }
    }
    #endregion

    #region Collection class
    /// <summary>
    /// Represents a Collection resource.
    /// </summary>
    public class Collection : IResource
    {
        AccountAddress Creator;
        string Description;
        string Name;
        string Uri;

        public static string StructTag = "0x4::collection::Collection";

        public Collection(AccountAddress Creator, string Description, string Name, string Uri)
        {
            this.Creator = Creator;
            this.Description = Description;
            this.Name = Name;
            this.Uri = Uri;
        }

        public override string ToString()
        {
            return string.Format("AccountAddress[creator: {0}, description: {1}, name: {2}, uri: {3}]", this.Creator, this.Description, this.Name, this.Uri);
        }

        // TODO: Check the resource data type and if it matches Python implementation
        public static IResource Parse(string resourceJson)
        {
            //CollectionResource resourceObj = JsonConvert.DeserializeObject<CollectionResource>(resourceJson, new CollectionResourceConverter());
            //return new Collection(
            //    AccountAddress.FromHex(resourceObj.Creator),
            //    resourceObj.Description,
            //    resourceObj.Name,
            //    resourceObj.Uri
            //);

            throw new NotImplementedException();
        }

        public string GetStructTag()
        {
            return StructTag;
        }
    }
    #endregion

    #region Royalty Class
    public class Royalty : IResource
    {
        int Numerator;
        int Denominator;
        AccountAddress PayeeAddress;

        string StructTag = "0x4::royalty::Royalty";

        public Royalty(int Numerator, int Denominator, AccountAddress PayeeAddress)
        {
            this.Numerator = Numerator;
            this.Denominator = Denominator;
            this.PayeeAddress = PayeeAddress;
        }

        public override string ToString()
        {
            return string.Format("Royalty[numerator: {0}, denominator: {1}, payee_address: {2}]", this.Numerator, this.Denominator, this.PayeeAddress);
        }

        // TODO: Check the resource data type and if it matches Python implementation
        public IResource Parse(string resourceJson)
        {
            //RoyaltyResource resourceObj = JsonConvert.DeserializeObject<RoyaltyResource>(resourceJson, new RoyaltyResourceConverter());
            //return new Royalty(
            //    resourceObj.Numerator,
            //    resourceObj.Denominator,
            //    resourceObj.Payee_address
            //);
            throw new NotImplementedException();
        }

        public string GetStructTag()
        {
            return StructTag;
        }
    }
    #endregion

    #region Token Class
    /// <summary>
    /// Represent a Token resource.
    /// </summary>
    public class Token : IResource
    {
        AccountAddress Collection;
        int Index;
        string Description;
        string Name;
        string Uri;

        string StructTag = "0x4::token::Token";

        public Token(AccountAddress Collection, int Index, string Description, string Name, string Uri)
        {
            this.Collection = Collection;
            this.Index = Index;
            this.Description = Description;
            this.Name = Name;
            this.Uri = Uri;
        }

        public override string ToString()
        {
            return string.Format("Token[collection: {0}, index: {1}, description: {2}, name: {3}, uri: {4}]", this.Collection, this.Index, this.Description, this.Name, this.Uri);
        }

        // TODO: Check the resource data type and if it matches Python implementation
        public static IResource Parse(string resourceJson)
        {
            //TokenResource resourceObj = JsonConvert.DeserializeObject<TokenResource>(resourceJson, new TokenResourceConverter());
            //return new Token(
            //    AccountAddress.FromHex(resourceObj.Collection.Inner),
            //    int.Parse(resourceObj.Index),
            //    resourceObj.Description,
            //    resourceObj.Name,
            //    resourceObj.Uri
            //);

            throw new NotImplementedException();
        }

        public string GetStructTag()
        {
            return StructTag;
        }
    }
    #endregion

    #region
    // TODO: Implement InvalidPropertyType
    public class InvalidPropertyType
    {

    }
    #endregion

    #region Property Class
    public class Property
    {
        public BString Name;
        public BString PropertyType;
        public ISerializable Value; // TODO: Define static type

        public static int BOOL = 0;
        public static int U8 = 1;
        public static int U16 = 2;
        public static int U32 = 3;
        public static int U64 = 4;
        public static int U128 = 5;
        public static int U256 = 6;
        public static int ADDRESS = 7;
        public static int BYTE_VECTOR = 8;
        public static int STRING = 9;

        public Property(string Name, string PropertyType, ISerializable Value)
        {
            this.Name = new BString(Name);
            this.PropertyType = new BString(PropertyType);
            this.Value = Value;
        }

        public Property(BString Name, BString PropertyType, ISerializable Value)
        {
            this.Name = Name;
            this.PropertyType = PropertyType;
            this.Value = Value;
        }

        public override string ToString()
        {
            return string.Format("Property[{0}, {1}, {2}]", this.Name, this.PropertyType, this.Value);
        }

        public byte[] SerializeValue()
        {
            Serialization ser = new Serialization();
            if (this.PropertyType.Equals("bool"))
            {
                Bool boolValue = (Bool)Value;
                boolValue.Serialize(ser);
            }
            else if (this.PropertyType.Equals("u8"))
            {
                U8 u8Value = (U8)Value;
                u8Value.Serialize(ser);
            }
            //else if (this.PropertyType.Equals("u16"))
            //{
            //    U16 u16Value = (U16)Value;
            //    u16Value.Serialize(ser);
            //}
            else if (this.PropertyType.Equals("u32"))
            {
                U32 u32Value = (U32)Value;
                u32Value.Serialize(ser);
            }
            else if (this.PropertyType.Equals("u64"))
            {
                U64 u64Value = (U64)Value;
                u64Value.Serialize(ser);
            }
            else if (this.PropertyType.Equals("u128"))
            {
                U128 u128Value = (U128)Value;
                u128Value.Serialize(ser);
            }
            //else if (this.PropertyType.Equals("u256"))
            //{
            //    U256 u256Value = (U256)Value;
            //    u256Value.Serialize(ser);
            //}
            else if (this.PropertyType.Equals("address"))
            {
                AccountAddress accountAddress = (AccountAddress)Value;
                accountAddress.Serialize(ser);
            }
            else if (this.PropertyType.Equals("0x1::string::String"))
            {
                BString bStringValue = (BString)Value;
                bStringValue.Serialize(ser);
            }
            else if (this.PropertyType.Equals("vector<u8>"))
            {
                Bytes sequenceVectorValue = (Bytes)Value;
                sequenceVectorValue.Serialize(ser);
            }
            else
            {
                throw new Exception("Invalid property type");
            }

            return ser.GetBytes();
        }

        // TODO: Review TransactionArgument implementation,
        // otherwise remove TranactionArgument class since ISerializable represents a TransactionArgument
        // public List<TransactionArgument> ToTransactionArguments()
        // {
        //     List<TransactionArgument> args = new List<TransactionArgument>();
        //     args.Add(new TransactionArgument(this.Name, typeof(BString)));
        //     args.Add(new TransactionArgument(this.PropertyType, typeof(BString)));
        //     args.Add(new TransactionArgument(new Bytes(this.SerializeValue()), typeof(Bytes)));
        //     return args;
        // }

        public List<ISerializable> ToTransactionArguments()
        {
            List<ISerializable> args = new List<ISerializable>();
            args.Add(this.Name);
            args.Add(this.PropertyType);
            args.Add(new Bytes(this.SerializeValue()));

            return args;
        }

        public static Property Parse(BString Name, int PropertyType, Bytes Value)
        {
            Deserialization deser = new Deserialization(Value.GetValue());

            if (PropertyType.Equals(Property.BOOL))
            {
                return new Property(Name, new BString("bool"), new Bool(deser.DeserializeBool()));
            }
            else if (PropertyType.Equals(Property.U8))
            {
                return new Property(Name, new BString("u8"), new U8(deser.DeserializeU8()));
            }
            //else if (PropertyType.Equals(Property.U16))
            //{
            //  return new Property(Name, new BString("u16"), new U16(deser.DeserializeU16()));
            //}
            else if (PropertyType.Equals(Property.U32))
            {
                return new Property(Name, new BString("u32"), new U32(deser.DeserializeU32()));
            }
            else if (PropertyType.Equals(Property.U64))
            {
                return new Property(Name, new BString("u64"), new U64(deser.DeserializeU64()));
            }
            else if (PropertyType.Equals(Property.U128))
            {
                return new Property(Name, new BString("u128"), new U128(deser.DeserializeU128()));
            }
            //else if (PropertyType.Equals(Property.U256))
            //{
            //  return new Property(Name, new BString("u256"), new U256(deser.DeserializeU256()));
            //}
            else if (PropertyType.Equals(Property.ADDRESS))
            {
                return new Property(Name, new BString("address"), AccountAddress.Deserialize(deser));
            }
            else if (PropertyType.Equals(Property.STRING))
            {
                return new Property(Name, new BString("0x1::string::String"), new BString(deser.DeserializeString()));
            }
            else if (PropertyType.Equals(Property.BYTE_VECTOR))
            {
                return new Property(Name, new BString("vector<u8>"), new Bytes(deser.ToBytes()));
            }
            else
            {
                throw new Exception("Invalid property type");
            }
        }

        public static Property Parse(string Name, int PropertyType, Bytes Value)
        {
            Deserialization deser = new Deserialization(Value.GetValue());

            if (PropertyType.Equals(Property.BOOL))
            {
                return new Property(Name, "bool", new Bool(deser.DeserializeBool()));
            }
            else if (PropertyType.Equals(Property.U8))
            {
                return new Property(Name, "u8", new U8(deser.DeserializeU8()));
            }
            //else if (PropertyType.Equals(Property.U16))
            //{
            //  return new Property(Name, "u16", new U16(deser.DeserializeU16()));
            //}
            else if (PropertyType.Equals(Property.U32))
            {
                return new Property(Name, "u32", new U32(deser.DeserializeU32()));
            }
            else if (PropertyType.Equals(Property.U64))
            {
                return new Property(Name, "u64", new U64(deser.DeserializeU64()));
            }
            else if (PropertyType.Equals(Property.U128))
            {
                return new Property(Name, "u128", new U128(deser.DeserializeU128()));
            }
            //else if (PropertyType.Equals(Property.U256))
            //{
            //  return new Property(Name, "u256", new U256(deser.DeserializeU256()));
            //}
            else if (PropertyType.Equals(Property.ADDRESS))
            {
                return new Property(Name, "address", AccountAddress.Deserialize(deser));
            }
            else if (PropertyType.Equals(Property.STRING))
            {
                return new Property(Name, "0x1::string::String", new BString(deser.DeserializeString()));
            }
            else if (PropertyType.Equals(Property.BYTE_VECTOR))
            {
                return new Property(Name, "vector<u8>", new Bytes(deser.ToBytes()));
            }
            else
            {
                throw new Exception("Invalid property type");
            }
        }

        public static Property BoolProp(string Name, bool Value)
        {
            return new Property(Name, "bool", new Bool(Value));
        }

        public static Property U8Prop(string Name, byte Value)
        {
            return new Property(Name, "u8", new U8(Value));
        }

        //public static Property U16Prop()

        public static Property U32Prop(string Name, uint Value)
        {
            return new Property(Name, "u32", new U32(Value));
        }

        public static Property U64Prop(string Name, uint Value)
        {
            return new Property(Name, "u64", new U64(Value));
        }

        public static Property U128Prop(string Name, uint Value)
        {
            return new Property(Name, "u128", new U128(Value));
        }

        //public static Property U256Prop(string Name, uint Value)
        //{
        //    return new Property(Name, "u256", new U256(Value));
        //}

        public static Property StringProp(string Name, string Value)
        {
            return new Property(Name, "0x1::string::String", new BString(Value));
        }

        public static Property BytesProp(string Name, byte[] Value)
        {
            return new Property(Name, "vector<u8>", new Bytes(Value));
        }
    }
    #endregion

    #region PropertyMap
    public class PropertyMap : IResource
    {
        List<Property> Properties;

        string StructTag = "0x4::property_map::PropertyMap";

        public PropertyMap(List<Property> Properties)
        {
            this.Properties = Properties;
        }

        public override string ToString()
        {
            string response = "PropertyMap[";
            foreach (Property prop in this.Properties)
                response += string.Format("{0}", prop);

            if (this.Properties.Count > 0)
                response = response.Remove(response.Length - 2, 2);
            response += "]";
            return response;
        }

        public Tuple<List<BString>, List<BString>, List<byte[]>> ToTuple()
        {
            List<BString> names = new List<BString>();
            List<BString> types = new List<BString>();
            List<byte[]> values = new List<byte[]>();

            foreach (Property prop in this.Properties)
            {
                names.Add(prop.Name);
                types.Add(prop.PropertyType);
                values.Add(prop.SerializeValue());
            }

            return Tuple.Create(names, types, values);
        }

        // TODO: Look into implementation and or resource object model
        public static IResource Parse(string resourceJson)
        {
            //PropertyMapResource resourceObj = JsonConvert.DeserializeObject<PropertyMapResource>(resourceJson, new PropertyMapResourceConverter());
            //PropertyList props = resourceObj.Inner.Data;
            //List<Property> properties = new List<Property>();
            //foreach(Property prop in props)
            //{
            //    properties.Add(
            //        Property.Parse(
            //            new BString(prop.Key),
            //            prop.Value.Type,
            //            prop.Value.Value.Substring(2).BytesFromHex()
            //        )
            //    );
            //}
            //return new PropertyMap(properties);

            throw new NotImplementedException();
        }

        public string GetStructTag()
        {
            return StructTag;
        }
    }
    #endregion

    #region ReadObject Class
    public class ReadObject
    {
        // TODO: Look into how to implement the following Python code:
        /* 
        resource_map: dict[str, Any] = {
            Collection.struct_tag: Collection,
            Object.struct_tag: Object,
            PropertyMap.struct_tag: PropertyMap,
            Royalty.struct_tag: Royalty,
            Token.struct_tag: Token,
        }

        */
        // TODO: Look into instantiating a Dictionary
        //Dictionary<string, Type> ResourceMap = {
        //    new KeyValuePair<string, Type>("", null)
        //};

        Dictionary<IResource, IResource> Resources;


        public ReadObject(Dictionary<IResource, IResource> Resources)
        {
            this.Resources = Resources;
        }

        public override string ToString()
        {
            string response = "ReadObject";
            foreach (KeyValuePair<IResource, IResource> resourceObjValue in this.Resources)
            {
                response += string.Format("\n\t{0}: {1}", resourceObjValue.Key.GetStructTag(), resourceObjValue.Value);
            }
            return response;
        }
    }
    #endregion

    /// <summary>
    /// A wrapper around reading and mutating AptosTokens also known as Token Objects
    /// </summary>
    public class AptosTokenClient : MonoBehaviour
    {
        RestClient Client;

        public AptosTokenClient(RestClient Client)
        {
            this.Client = Client;
        }

        // TODO: Look into missing function from Python code:
        // read_resources = await self.client.account_resources(address)
        public IEnumerator ReadObject(Action<AccountData, ResponseInfo> callback, AccountAddress address)
        {
            bool success = false;
            long responseCode = 0;
            string resourcesResp = "";
            Coroutine getAccountResourceCor = StartCoroutine(Client.GetAccountResource((_success, _responseCode, returnResult) =>
            {
                success = _success;
                responseCode = _responseCode;
                resourcesResp = returnResult;
            }, address, ""));
            yield return getAccountResourceCor;

            ResponseInfo responseInfo = new ResponseInfo();

            if (!success && responseCode == 404)
            {
                responseInfo.status = ResponseInfo.Status.Failed;
                responseInfo.message = resourcesResp;
                callback(null, responseInfo);
                yield break;
            }

            // TODO: Implement ResourcesResponse data model
            //Model.Resources resources;

            //ResourcesResponse resourcesResponseObj = JsonConvert.DeserializeObject<ResourcesResponse>(resourcesResp);
            //List<Resource> resourcesList = resourcesResponseObj.responses;

            // TODO: Implement
            //for resource in read_resources:
            //if resource["type"] in ReadObject.resource_map:
            //    resource_obj = ReadObject.resource_map[resource["type"]]
            //    resources[resource_obj] = resource_obj.parse(resource["data"])
            //return ReadObject(resources)

            //foreach (Resource resource in resourcesList)
            //{
            //    if (ReadObject.ResourceMap.ContainsKey(resource.Type))
            //    {
            //        Resource resourceObj = ReadObject.ResourceMap[resource.Type];
            //        resources[]
            //    }
            //}
        }

        public IEnumerator CreateCollection(
            Action<string, ResponseInfo> Callback,
            Account Creator,
            string Description,
            int MaxSupply,
            string Name,
            string Uri,
            bool MutableDescription,
            bool MutableRoyalty,
            bool MutableUri,
            bool MutableTokenDescription,
            bool MutableTokenName,
            bool MutableTokenProperties,
            bool MutableTokenUri,
            bool TokensBurnableByCreator,
            bool TokensFreezableByCreator,
            int RoyaltyNumerator,
            int RoyaltyDenominator
            )
        {
            ISerializable[] transactionArguments =
            {
                new BString(Description),
                new U64((ulong)MaxSupply),
                new BString(Name),
                new BString(Uri),
                new Bool(MutableDescription),
                new Bool(MutableRoyalty),
                new Bool(MutableUri),
                new Bool(MutableTokenDescription),
                new Bool(MutableTokenName),
                new Bool(MutableTokenProperties),
                new Bool(MutableTokenUri),
                new Bool(TokensBurnableByCreator),
                new Bool(TokensFreezableByCreator),
                new U64((ulong)RoyaltyNumerator),
                new U64((ulong)RoyaltyDenominator)
            };

            EntryFunction payload = EntryFunction.Natural(
                new ModuleId(AccountAddress.FromHex("0x4"), "aptos_token"),
                "create_collection",
                new TagSequence(new ISerializableTag[0]),
                new Sequence(transactionArguments)
            );

            // TODO: Add create_bcs_signed_transaction REST call
            // signed_transaction = await self.client.create_bcs_signed_transaction(
            //     creator, TransactionPayload(payload)
            // )
            // return await self.client.submit_bcs_transaction(signed_transaction)

            SignedTransaction signedTransaction = null;
            Coroutine cor_createBcsSIgnedTransaction = StartCoroutine(Client.CreateBCSSignedTransaction((_signedTransaction) => {
                signedTransaction = _signedTransaction;
            }, Creator, new BCS.TransactionPayload(payload)));
            yield return cor_createBcsSIgnedTransaction;


            string submitBcsTxnJsonResponse = "";
            ResponseInfo responseInfo = new ResponseInfo();

            Coroutine cor_submitBcsTransaction = StartCoroutine(Client.SubmitBCSTransaction((_responseJson, _responseInfo) => {
                submitBcsTxnJsonResponse = _responseJson;
                responseInfo = _responseInfo;
            }, signedTransaction));
            yield return cor_submitBcsTransaction;

            Callback(submitBcsTxnJsonResponse, responseInfo);

            yield return null;
        }

        public IEnumerator IMintToken(
            Action<string, ResponseInfo> Callback,
            Account Creator,
            string Collection,
            string Description,
            string Name,
            string Uri,
            PropertyMap Properties
        )
        {
            Tuple<List<BString>, List<BString>, List<byte[]>> propertiesTuple = Properties.ToTuple();

            ISerializable[] transactionArguments =
            {
                new BString(Collection),
                new BString(Description),
                new BString(Name),
                new BString(Uri),
                new Sequence(propertiesTuple.Item1.ToArray()),
                new Sequence(propertiesTuple.Item2.ToArray()),
                new BytesSequence(propertiesTuple.Item3.ToArray())
            };

            EntryFunction payload = EntryFunction.Natural(
                new ModuleId(AccountAddress.FromHex("0x4"), "aptos_token"),
                "mint",
                new TagSequence(new ISerializableTag[0]),
                new Sequence(transactionArguments)
            );

            // TODO: Add create_bcs_signed_transaction REST call
            // signed_transaction = await self.client.create_bcs_signed_transaction(
            //     creator, TransactionPayload(payload)
            //)
            // return await self.client.submit_bcs_transaction(signed_transaction)
            SignedTransaction signedTransaction = null;
            Coroutine cor_createBcsSIgnedTransaction = StartCoroutine(Client.CreateBCSSignedTransaction((_signedTransaction) => {
                signedTransaction = _signedTransaction;
            }, Creator, new BCS.TransactionPayload(payload)));
            yield return cor_createBcsSIgnedTransaction;


            string submitBcsTxnJsonResponse = "";
            ResponseInfo responseInfo = new ResponseInfo();

            Coroutine cor_submitBcsTransaction = StartCoroutine(Client.SubmitBCSTransaction((_responseJson, _responseInfo) => {
                submitBcsTxnJsonResponse = _responseJson;
                responseInfo = _responseInfo;
            }, signedTransaction));
            yield return cor_submitBcsTransaction;

            Callback(submitBcsTxnJsonResponse, responseInfo);

            yield return null;
        }

        public IEnumerator IMintSoulBoundToken(
            Action<string, ResponseInfo> Callback,
            Account Creator,
            string Collection,
            string Description,
            string Name,
            string Uri,
            PropertyMap Properties,
            AccountAddress SoulBoundTo
        )
        {
            Tuple<List<BString>, List<BString>, List<byte[]>> propertiesTuple = Properties.ToTuple();

            ISerializable[] transactionArguments =
            {
                new BString(Collection),
                new BString(Description),
                new BString(Name),
                new BString(Uri),
                new Sequence(propertiesTuple.Item1.ToArray()),
                new Sequence(propertiesTuple.Item2.ToArray()),
                new BytesSequence(propertiesTuple.Item3.ToArray()),
                SoulBoundTo
            };

            EntryFunction payload = EntryFunction.Natural(
                new ModuleId(AccountAddress.FromHex("0x4"), "aptos_token"),
                "mint_soul_bound",
                new TagSequence(new ISerializableTag[0]),
                new Sequence(transactionArguments)
            );

            // TODO: Add create_bcs_signed_transaction REST call
            // signed_transaction = await self.client.create_bcs_signed_transaction(
            //     creator, TransactionPayload(payload)
            // )
            // return await self.client.submit_bcs_transaction(signed_transaction)

            SignedTransaction signedTransaction = null;
            Coroutine cor_createBcsSIgnedTransaction = StartCoroutine(Client.CreateBCSSignedTransaction((_signedTransaction) => {
                signedTransaction = _signedTransaction;
            }, Creator, new BCS.TransactionPayload(payload)));
            yield return cor_createBcsSIgnedTransaction;


            string submitBcsTxnJsonResponse = "";
            ResponseInfo responseInfo = new ResponseInfo();

            Coroutine cor_submitBcsTransaction = StartCoroutine(Client.SubmitBCSTransaction((_responseJson, _responseInfo) => {
                submitBcsTxnJsonResponse = _responseJson;
                responseInfo = _responseInfo;
            }, signedTransaction));
            yield return cor_submitBcsTransaction;

            Callback(submitBcsTxnJsonResponse, responseInfo);

            yield return null;
        }

        public IEnumerator BurnToken(
            Action<string, ResponseInfo> Callback,
            Account Creator,
            AccountAddress Token
        )
        {
            EntryFunction payload = EntryFunction.Natural(
                 new ModuleId(AccountAddress.FromHex("0x4"), "aptos_token"),
                 "burn",
                 new TagSequence(new ISerializableTag[] { new StructTag(AccountAddress.FromHex("0x4"), "token", "Token", new ISerializableTag[0]) }),
                 new Sequence(new ISerializable[] { Token })
             );

            // TODO: Add create_bcs_signed_transaction REST call
            // signed_transaction = await self.client.create_bcs_signed_transaction(
            //     creator, TransactionPayload(payload)
            // )
            // return await self.client.submit_bcs_transaction(signed_transaction)

            SignedTransaction signedTransaction = null;
            Coroutine cor_createBcsSIgnedTransaction = StartCoroutine(Client.CreateBCSSignedTransaction((_signedTransaction) => {
                signedTransaction = _signedTransaction;
            }, Creator, new BCS.TransactionPayload(payload)));
            yield return cor_createBcsSIgnedTransaction;


            string submitBcsTxnJsonResponse = "";
            ResponseInfo responseInfo = new ResponseInfo();

            Coroutine cor_submitBcsTransaction = StartCoroutine(Client.SubmitBCSTransaction((_responseJson, _responseInfo) => {
                submitBcsTxnJsonResponse = _responseJson;
                responseInfo = _responseInfo;
            }, signedTransaction));
            yield return cor_submitBcsTransaction;

            Callback(submitBcsTxnJsonResponse, responseInfo);

            yield return null;
        }

        public IEnumerator FreezeToken(
            Action<string, ResponseInfo> Callback,
            Account Creator,
            AccountAddress Token
        )
        {
            EntryFunction payload = EntryFunction.Natural(
                 new ModuleId(AccountAddress.FromHex("0x4"), "aptos_token"),
                 "freeze_transfer",
                 new TagSequence(new ISerializableTag[] { new StructTag(AccountAddress.FromHex("0x4"), "token", "Token", new ISerializableTag[0]) }),
                 new Sequence(new ISerializable[] { Token })
             );

            // TODO: Add create_bcs_signed_transaction REST call
            // signed_transaction = await self.client.create_bcs_signed_transaction(
            //     creator, TransactionPayload(payload)
            // )
            // return await self.client.submit_bcs_transaction(signed_transaction)

            SignedTransaction signedTransaction = null;
            Coroutine cor_createBcsSIgnedTransaction = StartCoroutine(Client.CreateBCSSignedTransaction((_signedTransaction) => {
                signedTransaction = _signedTransaction;
            }, Creator, new BCS.TransactionPayload(payload)));
            yield return cor_createBcsSIgnedTransaction;


            string submitBcsTxnJsonResponse = "";
            ResponseInfo responseInfo = new ResponseInfo();

            Coroutine cor_submitBcsTransaction = StartCoroutine(Client.SubmitBCSTransaction((_responseJson, _responseInfo) => {
                submitBcsTxnJsonResponse = _responseJson;
                responseInfo = _responseInfo;
            }, signedTransaction));
            yield return cor_submitBcsTransaction;

            Callback(submitBcsTxnJsonResponse, responseInfo);

            yield return null;
        }

        public IEnumerator UnfreezeToken(
            Action<string, ResponseInfo> Callback,
            Account Creator,
            AccountAddress Token
        )
        {
            EntryFunction payload = EntryFunction.Natural(
                 new ModuleId(AccountAddress.FromHex("0x4"), "aptos_token"),
                 "unfreeze_transfer",
                 new TagSequence(new ISerializableTag[] { new StructTag(AccountAddress.FromHex("0x4"), "token", "Token", new ISerializableTag[0]) }),
                 new Sequence(new ISerializable[] { Token })
             );

            // TODO: Add create_bcs_signed_transaction REST call
            // signed_transaction = await self.client.create_bcs_signed_transaction(
            //     creator, TransactionPayload(payload)
            // )
            // return await self.client.submit_bcs_transaction(signed_transaction)

            SignedTransaction signedTransaction = null;
            Coroutine cor_createBcsSIgnedTransaction = StartCoroutine(Client.CreateBCSSignedTransaction((_signedTransaction) => {
                signedTransaction = _signedTransaction;
            }, Creator, new BCS.TransactionPayload(payload)));
            yield return cor_createBcsSIgnedTransaction;


            string submitBcsTxnJsonResponse = "";
            ResponseInfo responseInfo = new ResponseInfo();

            Coroutine cor_submitBcsTransaction = StartCoroutine(Client.SubmitBCSTransaction((_responseJson, _responseInfo) => {
                submitBcsTxnJsonResponse = _responseJson;
                responseInfo = _responseInfo;
            }, signedTransaction));
            yield return cor_submitBcsTransaction;

            Callback(submitBcsTxnJsonResponse, responseInfo);

            yield return null;
        }

        public IEnumerator AddTokenProperty(
            Action<string, ResponseInfo> Callback,
            Account Creator,
            AccountAddress Token,
            Property Prop)
        {
            // TODO: Double check this
            List<ISerializable> txnArgumentsList = Prop.ToTransactionArguments();
            txnArgumentsList.Insert(0, Token);

            ISerializable[] transactionArguments = txnArgumentsList.ToArray();

            EntryFunction payload = EntryFunction.Natural(
                new ModuleId(AccountAddress.FromHex("0x4"), "aptos_token"),
                "add_property",
                new TagSequence(new ISerializableTag[] { new StructTag(AccountAddress.FromHex("0x4"), "token", "Token", new ISerializableTag[0]) }),
                new Sequence(transactionArguments)
            );

            // TODO: Add create_bcs_signed_transaction REST call
            // signed_transaction = await self.client.create_bcs_signed_transaction(
            //     creator, TransactionPayload(payload)
            // )
            // return await self.client.submit_bcs_transaction(signed_transaction)

            SignedTransaction signedTransaction = null;
            Coroutine cor_createBcsSIgnedTransaction = StartCoroutine(Client.CreateBCSSignedTransaction((_signedTransaction) => {
                signedTransaction = _signedTransaction;
            }, Creator, new BCS.TransactionPayload(payload)));
            yield return cor_createBcsSIgnedTransaction;


            string submitBcsTxnJsonResponse = "";
            ResponseInfo responseInfo = new ResponseInfo();

            Coroutine cor_submitBcsTransaction = StartCoroutine(Client.SubmitBCSTransaction((_responseJson, _responseInfo) => {
                submitBcsTxnJsonResponse = _responseJson;
                responseInfo = _responseInfo;
            }, signedTransaction));
            yield return cor_submitBcsTransaction;

            Callback(submitBcsTxnJsonResponse, responseInfo);

            yield return null;
        }

        public IEnumerator RemoveTokenProperty(
            Action<string, ResponseInfo> Callback,
            Account Creator,
            AccountAddress Token,
            string Name)
        {
            ISerializable[] transactionArguments = {
                Token,
                new BString(Name)
            };

            EntryFunction payload = EntryFunction.Natural(
                new ModuleId(AccountAddress.FromHex("0x4"), "aptos_token"),
                "remove_property",
                new TagSequence(new ISerializableTag[] { new StructTag(AccountAddress.FromHex("0x4"), "token", "Token", new ISerializableTag[0]) }),
                new Sequence(transactionArguments)
            );

            // TODO: Add create_bcs_signed_transaction REST call
            // signed_transaction = await self.client.create_bcs_signed_transaction(
            //     creator, TransactionPayload(payload)
            // )
            // return await self.client.submit_bcs_transaction(signed_transaction)
            SignedTransaction signedTransaction = null;
            Coroutine cor_createBcsSIgnedTransaction = StartCoroutine(Client.CreateBCSSignedTransaction((_signedTransaction) => {
                signedTransaction = _signedTransaction;
            }, Creator, new BCS.TransactionPayload(payload)));
            yield return cor_createBcsSIgnedTransaction;


            string submitBcsTxnJsonResponse = "";
            ResponseInfo responseInfo = new ResponseInfo();

            Coroutine cor_submitBcsTransaction = StartCoroutine(Client.SubmitBCSTransaction((_responseJson, _responseInfo) => {
                submitBcsTxnJsonResponse = _responseJson;
                responseInfo = _responseInfo;
            }, signedTransaction));
            yield return cor_submitBcsTransaction;

            Callback(submitBcsTxnJsonResponse, responseInfo);

            yield return null;
        }

        public IEnumerator UpdateTokenProperty(
            Action<string, ResponseInfo> Callback,
            Account Creator,
            AccountAddress Token,
            Property Prop)
        {
            // TODO: Double check this
            List<ISerializable> txnArgumentsList = Prop.ToTransactionArguments();
            txnArgumentsList.Insert(0, Token);

            ISerializable[] transactionArguments = txnArgumentsList.ToArray();


            EntryFunction payload = EntryFunction.Natural(
                new ModuleId(AccountAddress.FromHex("0x4"), "aptos_token"),
                "update_property",
                new TagSequence(new ISerializableTag[] { new StructTag(AccountAddress.FromHex("0x4"), "token", "Token", new ISerializableTag[0]) }),
                new Sequence(transactionArguments)
            );

            // TODO: Add create_bcs_signed_transaction REST call
            // signed_transaction = await self.client.create_bcs_signed_transaction(
            //     creator, TransactionPayload(payload)
            // )
            // return await self.client.submit_bcs_transaction(signed_transaction)
            SignedTransaction signedTransaction = null;
            Coroutine cor_createBcsSIgnedTransaction = StartCoroutine(Client.CreateBCSSignedTransaction((_signedTransaction) => {
                signedTransaction = _signedTransaction;
            }, Creator, new BCS.TransactionPayload(payload)));
            yield return cor_createBcsSIgnedTransaction;


            string submitBcsTxnJsonResponse = "";
            ResponseInfo responseInfo = new ResponseInfo();

            Coroutine cor_submitBcsTransaction = StartCoroutine(Client.SubmitBCSTransaction((_responseJson, _responseInfo) => {
                submitBcsTxnJsonResponse = _responseJson;
                responseInfo = _responseInfo;
            }, signedTransaction));
            yield return cor_submitBcsTransaction;

            Callback(submitBcsTxnJsonResponse, responseInfo);

            yield return null;
        }
    }
}