using Aptos.Accounts;
using Aptos.BCS;
using Aptos.HdWallet.Utils;
using Aptos.Unity.Rest.Model;
using Aptos.Unity.Rest.Model.Resources;
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
        public static IResource Parse(ResourceDataBase resource)
        {
            throw new NotImplementedException();
        }
    }

    #region Object Class
    /// <summary>
    /// Represents an Object resource.
    /// </summary>
    class Object : IResource
    {
        bool AllowUngatedTransfer;
        AccountAddress Owner;

        public static string StructTag = "0x1::object::ObjectCore";

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
        public static IResource Parse(ResourceDataBase resourceData)
        {
            ObjectResourceData objcResData = (ObjectResourceData)resourceData;
            return new Object(
                objcResData.AllowUngatedTransfer,
                AccountAddress.FromHex(objcResData.Owner)
            );
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

        public static IResource Parse(ResourceDataBase resource)
        {
            CollectionResourceData collectionData = (CollectionResourceData)resource;
            return new Collection(
                AccountAddress.FromHex(collectionData.Creator),
                collectionData.Description,
                collectionData.Name,
                collectionData.Uri
            );
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

        public static string StructTag = "0x4::royalty::Royalty";

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

        public static IResource Parse(ResourceDataBase resource)
        {
            RoyaltyResourceData royaltyResData = (RoyaltyResourceData)resource;
            return new Royalty(
                int.Parse(royaltyResData.Numerator),
                int.Parse(royaltyResData.Denominator),
                AccountAddress.FromHex(royaltyResData.PayeeAddress)
            );
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

        public static string StructTag = "0x4::token::Token";

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

        public static IResource Parse(ResourceDataBase resource)
        {
            TokenResourceData royaltyResData = (TokenResourceData)resource;
            return new Token(
                AccountAddress.FromHex(royaltyResData.Collection.Inner),
                int.Parse(royaltyResData.Index),
                royaltyResData.Description,
                royaltyResData.Name,
                royaltyResData.Uri
            );
        }

        public string GetStructTag()
        {
            return StructTag;
        }
    }
    #endregion

    #region Property Class
    public class Property
    {
        public BString Name;
        public BString PropertyType;
        public ISerializable Value;

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

        //public static Property U16Prop() {}

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

        public static string StructTag = "0x4::property_map::PropertyMap";

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

        public static IResource Parse(ResourceDataBase resource)
        {
            PropertyMapResourceData propMapResData = (PropertyMapResourceData)resource;
            List<Property> properties = new List<Property>();
            List<PropertyResource> props = propMapResData.Inner.Data;

            foreach(PropertyResource prop in props)
            {
                properties.Add(
                    Property.Parse(
                        new BString(prop.Key),
                        int.Parse(prop.Value.Type),
                        new Bytes(prop.Value.Value.ByteArrayFromHexString())
                    )
                );
            }

            return new PropertyMap(properties);
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
        public static Dictionary<string, Func<ResourceDataBase, IResource>> ResourceMap
            = new Dictionary<string, Func<ResourceDataBase, IResource>>
        {
            { Collection.StructTag, new Func<ResourceDataBase, IResource>(Collection.Parse) },
            { Object.StructTag, new Func<ResourceDataBase, IResource>(Object.Parse) },
            { PropertyMap.StructTag, new Func<ResourceDataBase, IResource>(PropertyMap.Parse) },
            { Royalty.StructTag, new Func<ResourceDataBase, IResource>(Royalty.Parse) },
            { Token.StructTag, new Func<ResourceDataBase, IResource>(Token.Parse) }
        };

        // <0x1::object::ObjectCore, ResourceDataBase>
        Dictionary<string, IResource> Resources;

        public ReadObject(Dictionary<string, IResource> Resources)
        {
            this.Resources = Resources;
        }

        public override string ToString()
        {
            string response = "ReadObject";
            foreach (KeyValuePair<string, IResource> resourceObjValue in this.Resources)
            {
                //response += string.Format("\n\t{0}: {1}", resourceObjValue.Key.GetStructTag(), resourceObjValue.Value);
                response += string.Format("\n\t{0}: {1}", resourceObjValue.Key, resourceObjValue.Value);
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

        public static AptosTokenClient Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }

        public AptosTokenClient SetUp(RestClient Client)
        {
            this.Client = Client;
            return Instance;
        }

        public IEnumerator ReadObject(Action<ReadObject, ResponseInfo> callback, AccountAddress address)
        {
            bool success = false;
            long responseCode = 0;
            string resourcesResp = "";
            Coroutine getAccountResourceCor = StartCoroutine(Client.GetAccountResources((_success, _responseCode, returnResult) =>
            {
                success = _success;
                responseCode = _responseCode;
                resourcesResp = returnResult;
            }, address));
            yield return getAccountResourceCor;

            ResponseInfo responseInfo = new ResponseInfo();

            if (!success && responseCode == 404)
            {
                responseInfo.status = ResponseInfo.Status.Failed;
                responseInfo.message = resourcesResp;
                callback(null, responseInfo);
                yield break;
            }

            Dictionary<string, IResource> resources = new Dictionary<string, IResource>();
            List<IResourceBase> readResources = JsonConvert.DeserializeObject<List<IResourceBase>>(resourcesResp, new ResourceBaseListConverter<IResourceBase>());
            foreach (IResourceBase resource in readResources)
            {
                string type = resource.Type;
                if(Rest.ReadObject.ResourceMap.ContainsKey(type))
                {
                    string resourceObj = type; // NOTE: In Python it resources the entire class

                    if(resourceObj.Equals(Collection.StructTag))
                    {
                        CollectionResource collectionRes = (CollectionResource)resource;
                        CollectionResourceData data = collectionRes.Data;
                        Collection collection = (Collection) Collection.Parse(data);
                        resources.Add(resourceObj, collection);
                    }
                    else if(resourceObj.Equals(Object.StructTag))
                    {
                        ObjectResource objectRes = (ObjectResource) resource;
                        ObjectResourceData data = objectRes.Data;
                        Object obj = (Object)Object.Parse(data);
                        resources.Add(resourceObj, obj);
                    }
                    else if (resourceObj.Equals(PropertyMap.StructTag))
                    {
                        PropertyMapResource PropMapRes = (PropertyMapResource)resource;
                        PropertyMapResourceData data = PropMapRes.Data;
                        PropertyMap propMap = (PropertyMap)PropertyMap.Parse(data);
                        resources.Add(resourceObj, propMap);
                    }
                    else if (resourceObj.Equals(Royalty.StructTag))
                    {
                        RoyaltyResource RoyaltyRes = (RoyaltyResource)resource;
                        RoyaltyResourceData data = RoyaltyRes.Data;
                        Royalty royalty = (Royalty)Royalty.Parse(data);
                        resources.Add(resourceObj, royalty);
                    }
                    else // Token
                    {
                        TokenResource RoyaltyRes = (TokenResource)resource;
                        TokenResourceData data = RoyaltyRes.Data;
                        Token token = (Token)Token.Parse(data);
                        resources.Add(resourceObj, token);
                    }
                }
            }

            responseInfo.status = ResponseInfo.Status.Success;
            responseInfo.message = resourcesResp;
            callback(new ReadObject(resources), responseInfo);
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

        public IEnumerator MintToken(
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

        public IEnumerator TransferToken(
            Action<string, ResponseInfo> Callback,
            Account Owner,
            AccountAddress Token,
            AccountAddress To
        )
        {
            return Client.TransferObject(Callback, Owner, Token, To);
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
            List<ISerializable> txnArgumentsList = Prop.ToTransactionArguments();
            txnArgumentsList.Insert(0, Token);
            ISerializable[] transactionArguments = txnArgumentsList.ToArray();

            EntryFunction payload = EntryFunction.Natural(
                new ModuleId(AccountAddress.FromHex("0x4"), "aptos_token"),
                "add_property",
                new TagSequence(new ISerializableTag[] { new StructTag(AccountAddress.FromHex("0x4"), "token", "Token", new ISerializableTag[0]) }),
                new Sequence(transactionArguments)
            );

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
            List<ISerializable> txnArgumentsList = Prop.ToTransactionArguments();
            txnArgumentsList.Insert(0, Token);
            ISerializable[] transactionArguments = txnArgumentsList.ToArray();

            EntryFunction payload = EntryFunction.Natural(
                new ModuleId(AccountAddress.FromHex("0x4"), "aptos_token"),
                "update_property",
                new TagSequence(new ISerializableTag[] { new StructTag(AccountAddress.FromHex("0x4"), "token", "Token", new ISerializableTag[0]) }),
                new Sequence(transactionArguments)
            );

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

        public IEnumerator TokensMintedFromTransaction(Action<List<AccountAddress>, ResponseInfo> Callback, string TransactionHash)
        {
            ResponseInfo responseInfo = new ResponseInfo();
            Transaction responseTx = new Transaction();
            Coroutine transactionByHashCor = StartCoroutine(Client.TransactionByHash((_responseTx, _responseInfo) => {
                responseTx = _responseTx;
                responseInfo = _responseInfo;
            }, TransactionHash));

            yield return transactionByHashCor;

            List<AccountAddress> mints = new List<AccountAddress>();

            foreach (TransactionEvent txEvent in responseTx.Events)
            {
                if (txEvent.Type.Equals(Constants.APTOS_MINT_EVENT))
                {
                    mints.Add(AccountAddress.FromHex(txEvent.Data.Token));
                }
            }

            Callback(mints, responseInfo);
            yield return null;
        }
    }
}