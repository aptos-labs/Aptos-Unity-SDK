using Aptos.Accounts;
using Aptos.BCS;
using Aptos.Unity.Rest.Model;
using Chaos.NaCl;
using NBitcoin;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Transaction = Aptos.Unity.Rest.Model.Transaction;

namespace Aptos.Unity.Rest
{
    /// <summary>
    /// Common configuration for clients,
    /// particularly for submitting transactions
    /// </summary>
    public static class ClientConfig
    {
        public const int EXPIRATION_TTL = 600;
        public const int GAS_UNIT_PRICE = 100;
        public const int MAX_GAS_AMOUNT = 100000;
        public const int TRANSACTION_WAIT_IN_SECONDS = 20; // Amount of seconds to each during each polling cycle.
    }

    /// <summary>
    /// The Aptos REST Client contains a set of [standalone] Coroutines
    /// that can started within any Unity script.
    ///
    /// Consideration must be placed into the wait time required
    /// for a transaction to be committed into the blockchain.
    /// </summary>
    public class RestClient : MonoBehaviour
    {
        /// Static instance of the REST client.
        public static RestClient Instance { get; private set; }

        /// Based enpoint for REST API.
        public Uri Endpoint { get; private set; }
        public int? ChainId { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(this);
            else
                Instance = this;
        }

        #region Setup
        public IEnumerator SetUp()
        {
            LedgerInfo ledgerInfo = new LedgerInfo();
            ResponseInfo responseInfo = new ResponseInfo();
            Coroutine ledgerInfoCor = StartCoroutine(
                GetInfo((_ledgerInfo, _responseInfo) =>
                {
                    ledgerInfo = _ledgerInfo;
                    responseInfo = _responseInfo;
                })
            );

            yield return ledgerInfoCor;

            if (responseInfo.status != ResponseInfo.Status.Success)
            {
                Debug.LogError("Error getting ledger info: " + responseInfo.message);
                yield break;
            }

            this.ChainId = ledgerInfo.ChainId;
        }

        /// <summary>
        /// Set Endpoint for RPC / REST call.
        /// </summary>
        /// <param name="url">Base URL for REST API.</param>
        public RestClient SetEndPoint(string url)
        {
            Endpoint = new Uri(url);
            return this;
        }
        #endregion

        #region Account Accessors

        /// <summary>
        /// Get Account Details.
        /// Return the authentication key and the sequence number for an account address. Optionally, a ledger version can be specified.
        /// If the ledger version is not specified in the request, the latest ledger version is used.
        /// </summary>
        /// <param name="callback">Callback function used after response is received.</param>
        /// <param name="accountAddress">Address of the account.</param>
        /// <returns>Calls <c>callback</c> function with <c>(AccountData, ResponseInfo)</c>: \n
        /// An object that contains the account's:
        /// <c>sequence_number</c>, a string containing a 64-bit unsigned integer.
        /// Example: <code>32425224034</code>
        /// <c>authentication_key</c> All bytes (Vec) data is represented as hex-encoded string prefixed with 0x and fulfilled with two hex digits per byte.
        /// Unlike the Address type, HexEncodedBytes will not trim any zeros.
        /// Example: <code>0x88fbd33f54e1126269769780feb24480428179f552e2313fbe571b72e62a1ca1</code>, it is null if the request fails \n
        /// and a response object that contains the response details.
        /// </returns>
        public IEnumerator GetAccount(Action<AccountData, ResponseInfo> callback, AccountAddress accountAddress)
        {
            string accountsURL = Endpoint + "/accounts/" + accountAddress.ToString();
            Uri accountsURI = new Uri(accountsURL);
            var request = RequestClient.SubmitRequest(accountsURI);

            request.SendWebRequest();

            while (!request.isDone)
            {
                yield return null;
            }

            ResponseInfo responseInfo = new ResponseInfo();

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                responseInfo.status = ResponseInfo.Status.Failed;
                responseInfo.message = request.error;

                callback(null, responseInfo);
            }
            else if (request.responseCode == 404)
            {
                responseInfo.status = ResponseInfo.Status.NotFound;
                responseInfo.message = "Account not found";
                callback(null, responseInfo);
            }
            else if (request.responseCode >= 400)
            {
                responseInfo.status = ResponseInfo.Status.Failed;
                responseInfo.message = request.error;
                callback(null, responseInfo);
            }
            else
            {
                responseInfo.status = ResponseInfo.Status.Success;
                responseInfo.message = request.downloadHandler.text;

                AccountData accountData = JsonConvert.DeserializeObject<AccountData>(request.downloadHandler.text);
                callback(accountData, responseInfo);
            }

            request.Dispose();
        }

        /// <summary>
        /// Get an account's balance.    
        /// 
        /// The <c>/account</{address}/resource/{coin_type}</c> endpoint for AptosCoin returns the following response:     
        /// Gets Account Sequence Number
        /// </summary>
        /// <param name="callback">Callback function used after response is received.</param>
        /// <param name="accountAddress">Address of the account.</param>
        /// <returns>Calls <c>callback</c> function with <c>(string, ResponseInfo)</c>: \n
        /// A Sequence number as a string - null if the request fails, and a response object containing the response details. </returns>
        public IEnumerator GetAccountSequenceNumber(Action<string, ResponseInfo> callback, AccountAddress accountAddress)
        {
            AccountData accountData = new AccountData();
            ResponseInfo responseInfo = new ResponseInfo();
            Coroutine cor = StartCoroutine(GetAccount((_accountData, _responseInfo) => {
                accountData = _accountData;
                responseInfo = _responseInfo;
            }, accountAddress));
            yield return cor;

            if(responseInfo.status != ResponseInfo.Status.Success)
            {
                callback(null, responseInfo);
                yield break;
            }

            string sequenceNumber = accountData.SequenceNumber;

            callback(sequenceNumber, responseInfo);
        }

        /// <summary>
        /// Get an account's balance.
        ///
        /// The <c>/account</{address}/resource/{coin_type}</c> endpoint for AptosCoin returns the following response:
        /// <code>
        /// {
        ///     "type":"0x1::coin::CoinStore<0x1::aptos_coin::AptosCoin>",
        ///     "data":{
        ///         "coin":{
        ///             "value":"3400034000"
        ///         },
        ///         "deposit_events":{
        ///             "counter":"68",
        ///             "guid":{
        ///                 "id":{
        ///                     "addr":"0xd89fd73ef7c058ccf79fe4c1c38507d580354206a36ae03eea01ddfd3afeef07",
        ///                     "creation_num":"2"
        ///                 }
        ///             }
        ///         },
        ///         "frozen":false,
        ///         "withdraw_events":{
        ///             "counter":"0",
        ///             "guid":{
        ///                 "id":{
        ///                     "addr":"0xd89fd73ef7c058ccf79fe4c1c38507d580354206a36ae03eea01ddfd3afeef07",
        ///                     "creation_num":"3"
        ///                 }
        ///             }
        ///         }
        ///     }
        /// }
        /// </code>
        /// </summary>
        /// <param name="callback">Callback function used after response is received.</param>
        /// <param name="accountAddress">Address of the account.</param>
        /// <returns>Calls <c>callback</c> function with <c>(AccountResourceCoin.Coin, ResponseInfo)</c>: \n
        /// A representation of the coin, and an object containing the response details.</returns>
        public IEnumerator GetAccountBalance(Action<AccountResourceCoin.Coin, ResponseInfo> callback, Accounts.AccountAddress accountAddress)
        {
            string accountsURL = Endpoint + "/accounts/" + accountAddress.ToString() + "/resource/" + Constants.APTOS_COIN_TYPE;
            Uri accountsURI = new Uri(accountsURL);
            var request = RequestClient.SubmitRequest(accountsURI);

            request.SendWebRequest();
            while (!request.isDone)
            {
                yield return null;
            }

            ResponseInfo responseInfo = new ResponseInfo();

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                responseInfo.status = ResponseInfo.Status.Failed;
                responseInfo.message = "Connection error. " + request.error;
                callback(null, responseInfo);
            }
            else if (request.responseCode == 404)
            {
                responseInfo.status = ResponseInfo.Status.NotFound;
                responseInfo.message = "Resource not found. " + request.error;

                AccountResourceCoin.Coin coin = new AccountResourceCoin.Coin();
                coin.Value = "0";
                callback(coin, responseInfo);
            }
            else
            {
                responseInfo.status = ResponseInfo.Status.Success;
                responseInfo.message = request.downloadHandler.text;
                AccountResourceCoin acctResourceCoin = JsonConvert.DeserializeObject<AccountResourceCoin>(request.downloadHandler.text);
                AccountResourceCoin.Coin coin = new AccountResourceCoin.Coin();
                coin.Value = acctResourceCoin.DataProp.Coin.Value;
               
                callback(coin, responseInfo);
            }

            request.Dispose();
            //yield return null;
        }

        /// <summary>
        /// Get a resource of a given type from an account.
        /// NOTE: The response is a complex object of types only known to the developer writing the contracts.
        /// This function return a string and expect the developer to deserialize it into an object.
        /// See <see cref="GetAccountResourceCollection(Action{ResourceCollection, ResponseInfo}, AccountAddress, string)">GetAccountResourceCollection</see> for an example.
        /// </summary>
        /// <param name="callback">Callback function used when response is received.</param>
        /// <param name="accountAddress">Address of the account.</param>
        /// <param name="resourceType">Type of resource being queried for.</param>
        /// <returns>Calls <c>callback</c> function with <c>(bool, long, string)</c>: \n
        /// -- bool: success boolean \n
        /// -- long: - error code, string - JSON response to be deserialized by the consumer of the function\n
        /// -- string: - the response which may contain the resource details</returns>
        public IEnumerator GetAccountResource(Action<bool, long, string> callback, Accounts.AccountAddress accountAddress, string resourceType = "", string ledgerVersion = "")
        {
            string accountsURL = Endpoint + "/accounts/" + accountAddress.ToString() + "/resource/" + resourceType;
            Uri accountsURI = new Uri(accountsURL);
            UnityWebRequest request = UnityWebRequest.Get(accountsURI);
            request.SendWebRequest();
            while (!request.isDone)
            {
                yield return null;
            }

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                callback(false, 0, "ERROR: Connection Error: " + request.error);
            }
            else if (request.responseCode == 404)
            {
                callback(false, request.responseCode, request.error);
            }
            else if (request.responseCode >= 400)
            {
                callback(false, request.responseCode, request.error);
            }
            else
            {
                callback(true, request.responseCode, request.downloadHandler.text);
            }

            request.Dispose();
        }

        /// <summary>
        /// Get resources (in a JSON format) from a given account.
        /// </summary>
        /// <param name="callback">allback function used when response is received.</param>
        /// <param name="accountAddress">Address of the account.</param>
        /// <param name="ledgerVersion">Type of resource being queried for.</param>
        /// <returns></returns>
        public IEnumerator GetAccountResources(Action<bool, long, string> callback, Accounts.AccountAddress accountAddress, string ledgerVersion = "")
        {
            string accountsURL = Endpoint + "/accounts/" + accountAddress.ToString() + "/resources";
            Uri accountsURI = new Uri(accountsURL);
            UnityWebRequest request = UnityWebRequest.Get(accountsURI);
            request.SendWebRequest();
            while (!request.isDone)
            {
                yield return null;
            }

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                callback(false, 0, "ERROR: Connection Error: " + request.error);
            }
            else if (request.responseCode == 404)
            {
                callback(false, request.responseCode, request.error);
            }
            else if (request.responseCode >= 400)
            {
                callback(false, request.responseCode, request.error);
            }
            else
            {
                callback(true, request.responseCode, request.downloadHandler.text);
            }

            request.Dispose();
        }

        /// <summary>
        /// Get Account Resource
        /// </summary>
        /// <param name="callback">Callback function used after response is received.</param>
        /// <param name="accountAddress">Address of the account.</param>
        /// <param name="resourceType">Type of resource being queried for.</param>
        /// <returns>Calls <c>callback</c> function with <c>(ResourceCollection, ResponseInfo)</c>:\n
        /// An object representing a collection resource - null if the request fails, and a response object contains the response details.</returns>
        public IEnumerator GetAccountResourceCollection(Action<ResourceCollection, ResponseInfo> callback, Accounts.AccountAddress accountAddress, string resourceType)
        {
            string accountsURL = Endpoint + "/accounts/" + accountAddress.ToString() + "/resource/" + resourceType;
            Uri accountsURI = new Uri(accountsURL);
            var request = RequestClient.SubmitRequest(accountsURI);

            request.SendWebRequest();
            while (!request.isDone)
            {
                yield return null;
            }

            ResponseInfo responseInfo = new ResponseInfo();
            responseInfo.message = "Error when getting account resource. ";

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                responseInfo.status = ResponseInfo.Status.Failed;
                responseInfo.message += request.error;
                callback(null, responseInfo);
            }

            if (request.responseCode >= 400)
            {
                responseInfo.status = ResponseInfo.Status.Failed;
                responseInfo.message += "Account respurce not found. " + request.error;
                callback(null, responseInfo);
                yield break;
            }
            else
            {
                ResourceCollection acctResource = JsonConvert.DeserializeObject<ResourceCollection>(request.downloadHandler.text);
                responseInfo.status = ResponseInfo.Status.Success;
                responseInfo.message = request.downloadHandler.text;
                callback(acctResource, responseInfo);
            }

            request.Dispose();
            yield return null;
        }

        /// <summary>
        /// Gets table item that represents a coin resource
        /// See <see cref="GetTableItem(Action{string}, string, string, string, string)">GetTableItem</see>
        /// </summary>
        /// <param name="callback">Callback function used after response is received.</param>
        /// <param name="handle">The identifier for the given table.</param>
        /// <param name="keyType">String representation of an on-chain Move tag that is exposed in the transaction.</param>
        /// <param name="valueType">String representation of an on-chain Move type value.</param>
        /// <param name="key">The value of the table item's key, e.g. the name of a collection</param>
        /// <returns>Calls <c>callback</c> function with <c>(AccountResourceCoing, ResponseInfo)</c>:\n
        /// An object representing the account resource that holds the coin's information - null if the request fails, and a response object the contains the response details.</returns>
        public IEnumerator GetTableItemCoin(Action<AccountResourceCoin, ResponseInfo> callback, string handle, string keyType, string valueType, string key)
        {
            string getTableItemURL = Endpoint + "/tables/" + handle + "/item/";
            Uri getTableItemURI = new Uri(getTableItemURL);
            var request = RequestClient.SubmitRequest(getTableItemURI);

            request.SendWebRequest();
            while (!request.isDone)
            {
                yield return null;
            }

            ResponseInfo responseInfo = new ResponseInfo();
            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                responseInfo.status = ResponseInfo.Status.Failed;
                responseInfo.message = "Error while sending request for table item. " + request.error;
                callback(null, responseInfo);
            }
            if (request.responseCode == 404)
            {
                responseInfo.status = ResponseInfo.Status.NotFound;
                responseInfo.message = "Table item not found. " + request.error;
                callback(null, responseInfo);
            }
            else
            {
                string response = request.downloadHandler.text;
                AccountResourceCoin acctResource = JsonConvert.DeserializeObject<AccountResourceCoin>(response);
                responseInfo.status = ResponseInfo.Status.Success;
                responseInfo.message = response;
                callback(acctResource, responseInfo);
            }

            request.Dispose();
            yield return null;
        }

        /// <summary>
        /// Get a  table item at a specific ledger version from the table identified
        /// by the handle {table_handle} in the path and a [simple] "key" (TableItemRequest)
        /// provided by the request body.
        ///
        /// Further details are provider <see cref="https://fullnode.devnet.aptoslabs.com/v1/spec#/operations/get_table_item">here</see>
        /// </summary>
        /// <param name="callback">Callback function used after response is received.</param>
        /// <param name="handle">The identifier for the given table</param>
        /// <param name="keyType">String representation of an on-chain Move tag that is exposed in the transaction, e.g. "0x1::string::String"</param>
        /// <param name="valueType">String representation of an on-chain Move type value, e.g. "0x3::token::CollectionData"</param>
        /// <param name="key">The value of the table item's key, e.g. the name of a collection.</param>
        /// <returns>Calls <c>callback</c> function with a JSON object representing a table item - null if the request fails.</returns>
        public IEnumerator GetTableItem(Action<string> callback, string handle, string keyType, string valueType, string key)
        {
            TableItemRequest tableItemRequest = new TableItemRequest
            {
                KeyType = keyType,
                ValueType = valueType,
                Key = key
            };
            string tableItemRequestJson = JsonConvert.SerializeObject(tableItemRequest);

            string getTableItemURL = Endpoint + "/tables/" + handle + "/item";
            Uri getTableItemURI = new Uri(getTableItemURL);
            var request = RequestClient.SubmitRequest(getTableItemURI, UnityWebRequest.kHttpVerbPOST);

            byte[] jsonToSend = new UTF8Encoding().GetBytes(tableItemRequestJson);
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            request.SendWebRequest();
            while (!request.isDone)
            {
                yield return null;
            }

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                callback(null);
            }
            if (request.responseCode == 404)
            {
                callback(null);
            }
            else
            {
                string response = request.downloadHandler.text;
                callback(response);
            }

            request.Dispose();
            yield return null;
        }

        /// <summary>
        /// Get a  table item for a NFT from the table identified
        /// by the handle {table_handle} in the path and a complex key provided by the request body.
        ///
        /// See <see cref="GetTableItem(Action{string}, string, string, string, string)">GetTableItem</see> for a get table item using a generic string key.
        /// </summary>
        /// <param name="callback">Callback function used after response is received.</param>
        /// <param name="handle">The identifier for the given table.</param>
        /// <param name="keyType">String representation of an on-chain Move tag that is exposed in the transaction.</param>
        /// <param name="valueType">String representation of an on-chain Move type value.</param>
        /// <param name="key">A complex key object used to search for the table item. For example:
        /// <code>
        /// {
        ///     "token_data_id":{
        ///         "creator":"0xcd7820caacab04fbf1d7e563f4d329f06d2ce3140d654729d99258b5b68a27bf",
        ///         "collection":"Alice's",
        ///         "name":"Alice's first token"
        ///     },
        ///     "property_version":"0"
        /// }
        /// </code>
        /// </param>
        /// <returns>Calls <c>callback</c> function with <c>(TableItemToken, ResponseInfo)</c>: \n
        /// An object containing the details of the token - null if the request fails, and a response object containing the response details.
        /// </returns>
        public IEnumerator GetTableItemNFT(Action<TableItemToken, ResponseInfo> callback, string handle, string keyType, string valueType, TokenIdRequest key)
        {
            TableItemRequestNFT tableItemRequest = new TableItemRequestNFT
            {
                KeyType = keyType,
                ValueType = valueType,
                Key = key
            };
            string tableItemRequestJson = JsonConvert.SerializeObject(tableItemRequest);

            string getTableItemURL = Endpoint + "/tables/" + handle + "/item";
            Uri getTableItemURI = new Uri(getTableItemURL);
            var request = RequestClient.SubmitRequest(getTableItemURI, UnityWebRequest.kHttpVerbPOST);

            byte[] jsonToSend = new UTF8Encoding().GetBytes(tableItemRequestJson);
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            request.SendWebRequest();
            while (!request.isDone)
            {
                yield return null;
            }

            ResponseInfo responseInfo = new ResponseInfo();
            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                responseInfo.status = ResponseInfo.Status.Failed;
                responseInfo.message = "Error while sending request for table item. " + request.error;
                callback(null, responseInfo);
            }

            if (request.responseCode == 404)
            {
                TableItemToken tableItemToken = new TableItemToken();
                tableItemToken.Id = new Aptos.Unity.Rest.Model.Id();
                tableItemToken.Id.TokenDataId = new Aptos.Unity.Rest.Model.TokenDataId();
                tableItemToken.Id.TokenDataId.Creator = key.TokenDataId.Creator;
                tableItemToken.Id.TokenDataId.Collection = key.TokenDataId.Collection;
                tableItemToken.Id.TokenDataId.Name = key.TokenDataId.Name;
                tableItemToken.Amount = "0";

                responseInfo.status = ResponseInfo.Status.Success;
                responseInfo.message = "Table item not found. " + request.error;

                callback(tableItemToken, responseInfo);
            }
            else if (request.responseCode >= 400)
            {
                responseInfo.status = ResponseInfo.Status.Failed;
                responseInfo.message = request.error;
                callback(null, responseInfo);
            }
            else
            {
                string response = request.downloadHandler.text;
                TableItemToken tableItemToken = tableItemToken = JsonConvert.DeserializeObject<TableItemToken>(response);
                responseInfo.status = ResponseInfo.Status.Success;
                responseInfo.message = response;

                callback(tableItemToken, responseInfo);
            }

            request.Dispose();
            yield return null;
        }

        /// <summary>
        /// Get a table item that contains a token's (NFT) metadata.
        /// In this case we are using a complex key to retrieve the table item.
        ///
        /// Note: the response received from the REST <c>/table</c>  for this methods
        /// has a particular format specific to the SDK example.
        ///
        /// Developers will have to implement their own custom object's to match
        /// the properties of the NFT, meaning the content in the table item.
        ///
        /// The response for <c>/table</c> in the SDK example has the following format:
        /// <code>
        /// {
        ///     "default_properties":{
        ///         "map":{
        ///             "data":[ ]
        ///         }
        ///     },
        ///     "description":"Alice's simple token",
        ///     "largest_property_version":"0",
        ///     "maximum":"1",
        ///     "mutability_config":{
        ///         "description":false,
        ///         "maximum":false,
        ///         "properties":false,
        ///         "royalty":false,
        ///         "uri":false
        ///     },
        ///     "name":"Alice's first token",
        ///     "royalty":{
        ///         "payee_address":"0x8b8a8935cd90a87eaf47c7f309b6935e305e48b47f95982d65153b03032b3f33",
        ///         "royalty_points_denominator":"1000000",
        ///         "royalty_points_numerator":"0"
        ///     },
        ///     "supply":"1",
        ///     "uri":"https://aptos.dev/img/nyan.jpeg"
        /// }
        /// </code>
        /// </summary>
        /// <param name="callback">Callback function used after response is received.</param>
        /// <param name="handle">The identifier for the given table.</param>
        /// <param name="keyType">String representation of an on-chain Move tag that is exposed in the transaction.</param>
        /// <param name="valueType">String representation of an on-chain Move type value.</param>
        /// <param name="key">A complex key object used to search for the table item. In this case it's a TokenDataId object that contains the token / collection info</param>
        /// <returns>Calls <c>callback</c> function with <c>(TableItemTokenMetadata, ResponseInfo)</c>: \n
        /// An object that represent the NFT's metadata - null if the request fails, and a response object with the response details.
        /// </returns>
        public IEnumerator GetTableItemTokenData(Action<TableItemTokenMetadata, ResponseInfo> callback, string handle, string keyType, string valueType, TokenDataId key)
        {
            TableItemRequestTokenData tableItemRequest = new TableItemRequestTokenData
            {
                KeyType = keyType,
                ValueType = valueType,
                Key = key
            };
            string tableItemRequestJson = JsonConvert.SerializeObject(tableItemRequest);

            string getTableItemURL = Endpoint + "/tables/" + handle + "/item";
            Uri getTableItemURI = new Uri(getTableItemURL);
            var request = RequestClient.SubmitRequest(getTableItemURI, UnityWebRequest.kHttpVerbPOST);

            byte[] jsonToSend = new UTF8Encoding().GetBytes(tableItemRequestJson);
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            request.SendWebRequest();
            while (!request.isDone)
            {
                yield return null;
            }

            ResponseInfo responseInfo = new ResponseInfo();
            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                responseInfo.status = ResponseInfo.Status.Failed;
                responseInfo.message = "Error while requesting table item. " + request.error;
                callback(null, responseInfo);
            }
            if (request.responseCode == 404)
            {
                responseInfo.status = ResponseInfo.Status.NotFound;
                responseInfo.message = "Table item not found. " + request.error;
                callback(null, responseInfo);
            }
            else
            {
                string response = request.downloadHandler.text;
                TableItemTokenMetadata tableItemToken = JsonConvert.DeserializeObject<TableItemTokenMetadata>(response);

                responseInfo.status = ResponseInfo.Status.Success;
                responseInfo.message = response;
                callback(tableItemToken, responseInfo);
            }

            request.Dispose();
            yield return null;
        }


        /// <summary>
        /// Aggregates the values from a resource query.
        /// </summary>
        /// <param name="Callback"></param>
        /// <param name="AccountAddress"></param>
        /// <param name="ResourceType"></param>
        /// <param name="AggregatorPath"></param>
        /// <returns></returns>
        public IEnumerator AggregatorValue(
            Action<string, ResponseInfo> Callback,
            AccountAddress AccountAddress,
            string ResourceType,
            List<string> AggregatorPath
        )
        {
            throw new NotImplementedException();
        }

        #endregion


        #region Ledger Accessors
        /// <summary>
        /// Get the latest ledger information, including data such as chain ID, role type, ledger versions, epoch, etc.
        /// </summary>
        /// <param name="callback">Callback function used after response is received.</param>
        /// <returns>Calls <c>callback</c>function with <c>(LedgerInfo, response)</c>: \n
        /// An object that contains the chain details - null if the request fails, and a response object that contains the response details. </returns>
        public IEnumerator GetInfo(Action<LedgerInfo, ResponseInfo> callback)
        {
            var request = RequestClient.SubmitRequest(Endpoint);

            request.SendWebRequest();
            while (!request.isDone)
            {
                yield return null;
            }

            ResponseInfo responseInfo = new ResponseInfo();

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                responseInfo.status = ResponseInfo.Status.Failed;
                responseInfo.message = request.error;
                callback(null, responseInfo);
            }
            else if (request.responseCode >= 404)
            {
                responseInfo.status = ResponseInfo.Status.NotFound;
                responseInfo.message = request.error;
                callback(null, responseInfo);
            }
            else
            {
                string response = request.downloadHandler.text;
                LedgerInfo ledgerInfo = JsonConvert.DeserializeObject<LedgerInfo>(response);
                responseInfo.status = ResponseInfo.Status.Success;
                responseInfo.message = response;
                callback(ledgerInfo, responseInfo);
            }

            request.Dispose();
        }
        #endregion

        #region Transactions
        public IEnumerator SimulateTransaction(
            Action<string, ResponseInfo> callback,
            RawTransaction transaction,
            Account sender
        )
        {
            byte[] emptySignature = new byte[64]; // all 0's
            Authenticator authenticator = new Authenticator(
                new Ed25519Authenticator(
                    sender.PublicKey,
                    new Signature(emptySignature)

                )
            );

            SignedTransaction signedTransaction = new SignedTransaction(transaction, authenticator);

            string simulateTxnEndpoint = Endpoint + "/transactions/simulate";
            UnityWebRequest request = new UnityWebRequest(simulateTxnEndpoint, "POST");
            request.SetRequestHeader("Content-Type", "application/x.aptos.signed_transaction+bcs");

            request.uploadHandler = new UploadHandlerRaw(signedTransaction.Bytes());
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SendWebRequest();
            while (!request.isDone)
            {
                yield return null;
            }

            ResponseInfo responseInfo = new ResponseInfo();
            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                responseInfo.status = ResponseInfo.Status.Failed;
                responseInfo.message = "Error while submitting simulated transaction. " + request.error;
                callback(null, responseInfo);
            }
            if (request.responseCode == 404)
            {
                responseInfo.status = ResponseInfo.Status.NotFound;
                responseInfo.message = "Endppoint not found. " + request.error;
                callback(null, responseInfo);
            }
            else
            {
                string response = request.downloadHandler.text;

                responseInfo.status = ResponseInfo.Status.Success;
                responseInfo.message = response;
                callback(response, responseInfo);
            }

            request.Dispose();
            yield return null;

        }

        /// <summary>
        /// Submits a BCS transaction.
        /// </summary>
        /// <param name="callback">Callback function used after response is received with the JSON response.</param>
        /// <param name="SignedTransaction">The signed transaction.</param>
        /// <returns></returns>
        public IEnumerator SubmitBCSTransaction(
            Action<string, ResponseInfo> callback,
            SignedTransaction SignedTransaction
        )
        {
            string submitTxnEndpoint = Endpoint + "/transactions";
            UnityWebRequest request = new UnityWebRequest(submitTxnEndpoint, "POST");
            request.SetRequestHeader("Content-Type", "application/x.aptos.signed_transaction+bcs");
            request.uploadHandler = new UploadHandlerRaw(SignedTransaction.Bytes());
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SendWebRequest();
            while (!request.isDone)
            {
                yield return null;
            }

            ResponseInfo responseInfo = new ResponseInfo();
            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                responseInfo.status = ResponseInfo.Status.Failed;
                responseInfo.message = "Error while submitting BCS transaction. " + request.error;
                callback(null, responseInfo);
            }
            if (request.responseCode == 404)
            {
                responseInfo.status = ResponseInfo.Status.NotFound;
                responseInfo.message = "Endppoint for BCS transaction not found. " + request.error;
                callback(null, responseInfo);
            }
            else
            {
                string response = request.downloadHandler.text;

                responseInfo.status = ResponseInfo.Status.Success;
                responseInfo.message = response;
                callback(response, responseInfo);
            }

            request.Dispose();
            yield return null;
        }

        /// <summary>
        /// Execute the Move view function with the given parameters and return its execution result.
        ///
        /// Even if the function returns a single value, it will be wrapped in an array.
        ///
        /// Usage example where we determine an accounts AptosCoin balance:
        /// <code>
        /// string[] data = new string[] {};
        /// ViewRequest viewRequest = new ViewRequest();
        /// viewRequest.Function = "0x1::coin::balance";
        /// viewRequest.TypeArguments = new string[] { "0x1::aptos_coin::AptosCoin" };
        /// viewRequest.Arguments = new string[] { bobAddress.ToString() };
        /// Coroutine getBobAccountBalanceView = StartCoroutine(RestClient.Instance.View((_data, _responseInfo) =>
        /// {
        ///     data = _data;
        ///     responseInfo = _responseInfo;
        /// }, viewRequest));
        /// yield return getBobAccountBalanceView;
        /// if (responseInfo.status == ResponseInfo.Status.Failed) {
        ///     Debug.LogError(responseInfo.message);
        ///     yield break;
        /// }
        /// Debug.Log("Bob's Balance After Funding: " + ulong.Parse(data[0]));
        /// </code>
        /// </summary>
        /// <param name="callback">Callback function used after response is received.</param>
        /// <param name="viewRequest">The payload for the view function</param>
        /// <returns>A vec containing the values returned from the view functions.</returns>
        public IEnumerator View(Action<String[], ResponseInfo> callback, ViewRequest viewRequest)
        {
            var viewURL = Endpoint + "/view";
            var viewURI = new Uri(viewURL);
            var viewWebRequest = new UnityWebRequest(viewURI, "POST");
            var jsonToSend = new UTF8Encoding().GetBytes(JsonConvert.SerializeObject(viewRequest));
            viewWebRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
            viewWebRequest.downloadHandler = new DownloadHandlerBuffer();
            viewWebRequest.SetRequestHeader("Content-Type", "application/json");

            viewWebRequest.SendWebRequest();
            while (!viewWebRequest.isDone)
            {
                yield return null;
            }

            var responseInfo = new ResponseInfo();
            if (viewWebRequest.result != UnityWebRequest.Result.Success)
            {
                responseInfo.status = ResponseInfo.Status.Failed;
                responseInfo.message = "Error while submitting view function request. " + viewWebRequest.error;
                callback(null, responseInfo);
            }
            else // 200
            {
                var response = viewWebRequest.downloadHandler.text;
                var values = JsonConvert.DeserializeObject<String[]>(response);
                responseInfo.status = ResponseInfo.Status.Success;
                responseInfo.message = response;
                callback(values, responseInfo);
            }

            viewWebRequest.Dispose();
        }

        /// <summary>
        /// 1) Generates a transaction request \n
        /// 2) submits that to produce a raw transaction \n
        /// 3) signs the raw transaction \n
        /// 4) submits the signed transaction \n
        /// </summary>
        /// <param name="callback">Callback function used when response is received.</param>
        /// <param name="sender">Account submitting the transaction.</param>
        /// <param name="payload">Transaction payload.</param>
        /// <returns>Calls <c>callback</c>function with <c>(Transaction, ResponseInfo)</c>:\n
        /// An object that represents the transaction submitted - null if the transaction fails, and a response object with the response detials.
        /// </returns>
        public IEnumerator SubmitTransaction(Action<Transaction, ResponseInfo> callback, Account sender, EntryFunction entryFunction)
        {
            ///////////////////////////////////////////////////////////////////////
            // 1) Generate a transaction request
            ///////////////////////////////////////////////////////////////////////
            string sequenceNumber = "";
            ResponseInfo responseInfo = new ResponseInfo();
            Coroutine cor_sequenceNumber = StartCoroutine(GetAccountSequenceNumber((_sequenceNumber, _responseInfo) => {
                sequenceNumber = _sequenceNumber;
                responseInfo = _responseInfo;
            }, sender.AccountAddress));
            yield return cor_sequenceNumber;

            if (responseInfo.status != ResponseInfo.Status.Success)
            {
                callback(null, responseInfo);
            }

            var expirationTimestamp = (DateTime.Now.ToUnixTimestamp() + Constants.EXPIRATION_TTL).ToString();

            var txnRequest = new TransactionRequest()
            {
                Sender = sender.AccountAddress.ToString(),
                SequenceNumber = sequenceNumber,
                MaxGasAmount = Constants.MAX_GAS_AMOUNT.ToString(),
                GasUnitPrice = Constants.GAS_UNIT_PRICE.ToString(),
                ExpirationTimestampSecs = expirationTimestamp,
                EntryFunction = entryFunction
            };

            var txnRequestJson = JsonConvert.SerializeObject(txnRequest, new TransactionRequestConverter());

            ///////////////////////////////////////////////////////////////////////
            // 2) Submits that to produce a raw transaction
            ///////////////////////////////////////////////////////////////////////
            string encodedSubmission = "";

            Coroutine cor_encodedSubmission = StartCoroutine(EncodeSubmission((_encodedSubmission) => {
                encodedSubmission = _encodedSubmission;
            }, txnRequestJson));
            yield return cor_encodedSubmission;

            byte[] toSign = StringToByteArray(encodedSubmission.Trim('"')[2..]);

            ///////////////////////////////////////////////////////////////////////
            // 3) Signs the raw transaction
            ///////////////////////////////////////////////////////////////////////
            Signature signature = sender.Sign(toSign);

            txnRequest.Signature = new SignatureData()
            {
                Type = Constants.ED25519_SIGNATURE,
                PublicKey = "0x" + CryptoBytes.ToHexStringLower(sender.PublicKey),
                Signature = signature.ToString()
            };

            txnRequestJson = JsonConvert.SerializeObject(txnRequest, new TransactionRequestConverter());

            string transactionURL = Endpoint + "/transactions";
            Uri transactionsURI = new Uri(transactionURL);
            var request = RequestClient.SubmitRequest(transactionsURI, UnityWebRequest.kHttpVerbPOST);

            byte[] jsonToSend = new UTF8Encoding().GetBytes(txnRequestJson);
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            request.SendWebRequest();
            while (!request.isDone)
            {
                yield return null;
            }

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                responseInfo.status = ResponseInfo.Status.Failed;
                responseInfo.message = "Error while submitting transaction. " + request.error;
                callback(null, responseInfo);
            }
            else if (request.responseCode >= 404)
            {
                responseInfo.status = ResponseInfo.Status.Failed;
                responseInfo.message = "Error 404 when submitting transaction.";
                callback(null, responseInfo);
            }
            else // Either 200, or 202
            {
                string response = request.downloadHandler.text;
                Transaction transaction = JsonConvert.DeserializeObject<Transaction>(response, new TransactionConverter());

                responseInfo.status = ResponseInfo.Status.Success;
                responseInfo.message = response;
                callback(transaction, responseInfo);
            }

            request.Dispose();
        }

        /// <summary>
        /// A Coroutine that polls for a transaction hash until it is confimred in the blockchain
        /// Times out if the transaction hash is not found after querying for N times.
        ///
        /// Waits for a transaction query to respond whether a transaction submitted has been confirmed in the blockchain.
        /// Queries for a given transaction hash (txnHash) using <see cref="TransactionPending"/>
        /// by polling / looping until we find a "Success" transaction response, or until it times out after <see cref="TransactionWaitInSeconds"/>.
        ///
        /// </summary>
        /// <param name="callback">Callback function used when response is received.</param>
        /// <param name="txnHash">Transaction hash.</param>
        /// <returns>Calls <c>callback</c> function with (bool, ResponseInfo): \n
        /// A boolean that is: \n
        /// -- true if the transaction hash was found after polling and the transaction was succesfully commited in the blockhain \n
        /// -- false if we did not find the transaction hash and timed out \n
        ///
        /// A response object that contains the response details.
        /// </returns>
        public IEnumerator WaitForTransaction(Action<bool, ResponseInfo> callback, string txnHash)
        {
            int count = 0;  // Current attempt at querying for hash
            bool isTxnPending = true; // Has the transaction been confirmed in the blockchain

            bool isTxnSuccessful = false; // Was the transaction successful
            ResponseInfo responseInfo = new ResponseInfo();
            responseInfo.status = ResponseInfo.Status.Failed;

            while (isTxnPending)
            {
                ResponseInfo responseInfoTxnPending = new ResponseInfo();

                // Check if the transaction hash can be found
                Coroutine transactionPendingCor = StartCoroutine(TransactionPending((_isPending, _responseInfo) => {
                    isTxnPending = _isPending;
                    responseInfoTxnPending = _responseInfo;
                }, txnHash));
                yield return transactionPendingCor;

                // If transaction hash has been found in the blockchain (not "pending"), check if it was succesful
                if (!isTxnPending)
                {
                    Transaction transaction = JsonConvert.DeserializeObject<Transaction>(responseInfoTxnPending.message, new TransactionConverter());

                    // If the transaction has the "success" property, set the boolean response to true and break
                    if (transaction.GetType().GetProperty("Success") != null && transaction.Success)
                    {
                        responseInfo.status = ResponseInfo.Status.Success;
                        responseInfo.message = responseInfoTxnPending.message;

                        isTxnSuccessful = true;
                        break;
                    }
                }

                // Timeout if the transaction is still pending (hash not found) and we have queried N times
                // Set the boolean response to false, break -- we did not find the transaction
                if (count > ClientConfig.TRANSACTION_WAIT_IN_SECONDS)
                {
                    responseInfo.message = "Response Timed Out After Querying " + count + "Times";
                    isTxnSuccessful = false;
                    break;
                }

                count += 1;
                yield return new WaitForSeconds(2f);
            }

            yield return new WaitForSeconds(3f);
            callback(isTxnSuccessful, responseInfo);
        }

        /// <summary>
        /// Query to see if transaction has been 'confirmed' in the blockchain by using the transaction hash.
        /// A 404 error will be returned if the transaction hasn't been confirmed.
        /// Once the transaction is confirmed it will have a `pending_transaction` state.
        /// </summary>
        /// <param name="callback">Callback function used when response is received.</param>
        /// <param name="txnHash">Transaction hash being queried for.</param>
        /// <returns>Calls <c>callback</c>function with <c>(bool, ResponseInfo)</c>: \n
        /// A boolean that is: \n
        /// -- true if transaction is still pending / hasn't been found, meaning 404, error in response, or `pending_transaction` is true \n
        /// -- false if transaction has been found, meaning `pending_transaction` is true \n
        ///
        /// A response object that contains the response details.
        /// </returns>
        public IEnumerator TransactionPending(Action<bool, ResponseInfo> callback, string txnHash)
        {
            string txnURL = Endpoint + "/transactions/by_hash/" + txnHash;
            Uri txnURI = new Uri(txnURL);
            var request = RequestClient.SubmitRequest(txnURI);

            request.SendWebRequest();
            while (!request.isDone)
            {
                yield return null;
            }

            ResponseInfo responseInfo = new ResponseInfo();

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                responseInfo.status = ResponseInfo.Status.Failed;
                responseInfo.message = "Connection error. " + request.error;

                callback(true, responseInfo);
                request.Dispose();
                yield return new WaitForSeconds(1f);
            }
            else if (request.responseCode == 404)
            {
                responseInfo.status = ResponseInfo.Status.Failed;
                responseInfo.message = "Transaction Not Found: " + request.responseCode;

                callback(true, responseInfo);
                request.Dispose();
                yield return new WaitForSeconds(1f);
            }
            else if (request.responseCode >= 400)
            {
                responseInfo.status = ResponseInfo.Status.Failed;
                responseInfo.message = "Transaction Call Error: " + request.responseCode + " : " + request.downloadHandler.text;

                callback(true, responseInfo);
                request.Dispose();
                yield return new WaitForSeconds(1f);
            }
            else
            {
                var transactionResult = JsonConvert.DeserializeObject<Transaction>(request.downloadHandler.text, new TransactionConverter())!;
                bool isPending = transactionResult.Type.Equals("pending_transaction");

                responseInfo.status = ResponseInfo.Status.Success;
                responseInfo.message = request.downloadHandler.text;

                callback(isPending, responseInfo);

                request.Dispose();
                yield return new WaitForSeconds(1f);
            }
        }

        public IEnumerator TransactionByHash(Action<Transaction, ResponseInfo> callback, string txnHash)
        {
            string txnURL = Endpoint + "/transactions/by_hash/" + txnHash;
            Uri txnURI = new Uri(txnURL);
            var request = RequestClient.SubmitRequest(txnURI);

            request.SendWebRequest();
            while (!request.isDone)
            {
                yield return null;
            }

            ResponseInfo responseInfo = new ResponseInfo();

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                responseInfo.status = ResponseInfo.Status.Failed;
                responseInfo.message = "Connection error. " + request.error;

                callback(null, responseInfo);
                request.Dispose();
                yield return new WaitForSeconds(1f);
            }
            else if (request.responseCode == 404)
            {
                responseInfo.status = ResponseInfo.Status.Failed;
                responseInfo.message = "Transaction Not Found: " + request.responseCode;

                callback(null, responseInfo);
                request.Dispose();
                yield return new WaitForSeconds(1f);
            }
            else if (request.responseCode >= 400)
            {
                responseInfo.status = ResponseInfo.Status.Failed;
                responseInfo.message = "Transaction Call Error: " + request.responseCode + " : " + request.downloadHandler.text;

                callback(null, responseInfo);
                request.Dispose();
                yield return new WaitForSeconds(1f);
            }
            else
            {
                var transactionResult = JsonConvert.DeserializeObject<Transaction>(request.downloadHandler.text, new TransactionConverter())!;
                responseInfo.status = ResponseInfo.Status.Success;
                responseInfo.message = request.downloadHandler.text;

                callback(transactionResult, responseInfo);

                request.Dispose();
                yield return new WaitForSeconds(1f);
            }
        }

        #endregion

        #region Transaction Helpers

        public IEnumerable CreateMultiAgentBCSTransaction(Action<SignedTransaction> Callback, Account Sender, List<Account> SecondaryAccounts, BCS.TransactionPayload Payload)
        {
            string sequenceNumber = "";
            ResponseInfo responseInfo = new ResponseInfo();

            Coroutine cor_sequenceNumber = StartCoroutine(GetAccountSequenceNumber((_sequenceNumber, _responseInfo) => {
                sequenceNumber = _sequenceNumber;
                responseInfo = _responseInfo;
            }, Sender.AccountAddress));
            yield return cor_sequenceNumber;

            if (responseInfo.status != ResponseInfo.Status.Success)
            {
                throw new Exception("Unable to get sequence number for: " + Sender.AccountAddress);
            }

            List<AccountAddress> secondaryAddressList = new List<AccountAddress>();
            foreach (Account account in SecondaryAccounts)
                secondaryAddressList.Add(account.AccountAddress);

            if(ChainId == null) // If ChainId is null, the set up the REST Client, get the chain ID.
            {
                Coroutine restClientSetupCor = StartCoroutine(this.SetUp());
                yield return restClientSetupCor;
            }

            ulong expirationTimestamp = ((ulong)(DateTime.Now.ToUnixTimestamp() + Constants.EXPIRATION_TTL));

            MultiAgentRawTransaction rawTransaction = new MultiAgentRawTransaction(
                new RawTransaction(
                    Sender.AccountAddress,
                    int.Parse(sequenceNumber),
                    Payload,
                    ClientConfig.MAX_GAS_AMOUNT,
                    ClientConfig.GAS_UNIT_PRICE,
                    expirationTimestamp,
                    (int)this.ChainId
                ),
                new BCS.Sequence(secondaryAddressList.ToArray())
            );

            byte[] keyedTxn = rawTransaction.Keyed();

            List<Tuple<AccountAddress, Authenticator>> secondarySigners = new List<Tuple<AccountAddress, Authenticator>>();
            foreach (Account account in SecondaryAccounts)
            {
                secondarySigners.Add(
                    Tuple.Create<AccountAddress, Authenticator>(
                        account.AccountAddress,
                        new Authenticator(
                            new Ed25519Authenticator(account.PublicKey, account.Sign(keyedTxn))
                        )
                    )
                );
            }

            Authenticator authenticator = new Authenticator(
                new MultiAgentAuthenticator(
                    new Authenticator(
                        new Ed25519Authenticator(Sender.PublicKey, Sender.Sign(keyedTxn))
                    ),
                    secondarySigners
                )
            );

            Callback(new SignedTransaction(rawTransaction.Inner(), authenticator));
        }

        public IEnumerator CreateBCSTransaction(
            Action<RawTransaction> Callback,
            Account Sender,
            BCS.TransactionPayload payload)
        {
            string sequenceNumber = "";
            ResponseInfo responseInfo = new ResponseInfo();

            Coroutine cor_sequenceNumber = StartCoroutine(GetAccountSequenceNumber((_sequenceNumber, _responseInfo) => {
                sequenceNumber = _sequenceNumber;
                responseInfo = _responseInfo;
            }, Sender.AccountAddress));
            yield return cor_sequenceNumber;

            if (responseInfo.status != ResponseInfo.Status.Success)
            {
                throw new Exception("Unable to get sequence number for: " + Sender.AccountAddress + ".\n" + responseInfo.message);
            }

            if (ChainId == null) // If ChainId is null, the set up the REST Client, get the chain ID.
            {
                Coroutine restClientSetupCor = StartCoroutine(this.SetUp());
                yield return restClientSetupCor;
            }

            ulong expirationTimestamp = ((ulong)(DateTime.Now.ToUnixTimestamp() + Constants.EXPIRATION_TTL));

            RawTransaction rawTxn = new RawTransaction(
                Sender.AccountAddress,
                int.Parse(sequenceNumber),
                payload,
                ClientConfig.MAX_GAS_AMOUNT,
                ClientConfig.GAS_UNIT_PRICE,
                expirationTimestamp,
                (int)this.ChainId
            );

            Callback(rawTxn);

            yield return null;
        }

        public IEnumerator CreateBCSSignedTransaction(
            Action<SignedTransaction> Callback,
            Account Sender,
            BCS.TransactionPayload Payload
        )
        {
            RawTransaction rawTransaction = null;

            Coroutine cor_createBCSTransaction = StartCoroutine(CreateBCSTransaction((_rawTransaction) => {
                rawTransaction = _rawTransaction;
            }, Sender, Payload));
            yield return cor_createBCSTransaction;

            Signature signature = Sender.Sign(rawTransaction.Keyed());
            Authenticator authenticator = new Authenticator(
                new Ed25519Authenticator(Sender.PublicKey, signature)
            );

            Callback(new SignedTransaction(rawTransaction, authenticator));
        }

        #endregion

        #region Transaction Wrappers
        /// <summary>
        /// Transfer a given coin amount from a given Account to the recipient's account Address.
        /// Returns the sequence number of the transaction used to transfer.
        ///
        /// NOTE: Recipient address must hold APT before hand, and or have been airdrop APT if testing on devnet.
        /// </summary>
        /// <param name="callback">Callback function used when response is received.</param>
        /// <param name="sender">Account executing the transfer.</param>
        /// <param name="to">Address of recipient.</param>
        /// <param name="amount">Amount of tokens.</param>
        /// <returns>Calls <c>callback</c>function with <c>(Transaction, ResponseInfo)</c>: \n
        /// An object the represents the transaction submitted - null if the transfer failed, and a response object that contains the response details.
        /// </returns>
        public IEnumerator Transfer(Action<Transaction, ResponseInfo> callback, Account sender, string to, long amount)
        {
            ResponseInfo responseInfo = new ResponseInfo();
            // Check is address is valid
            if (!HdWallet.Utils.Utils.IsValidAddress(to))
            {
                responseInfo.status = ResponseInfo.Status.Failed;
                responseInfo.message = "Recipient address is invalid. " + to;
                callback(null, responseInfo);
                yield break;
            }

            EntryFunction payload = new EntryFunction(
                new ModuleId(AccountAddress.FromHex("0x1"), "aptos_account"),
                "transfer",
                new TagSequence(new ISerializableTag[] { }),
                new BCS.Sequence(
                    new ISerializable[]
                    {
                        AccountAddress.FromHex(to),
                        new U64((ulong)amount)
                    }
                )
            );

            Transaction submitTxn = new Transaction();
            responseInfo = new ResponseInfo();
            Coroutine cor_response = StartCoroutine(SubmitTransaction((_submitTxn, _responseInfo) => {
                submitTxn = _submitTxn;
                responseInfo = _responseInfo;
            }, sender, payload));
            yield return cor_response;

            if (responseInfo.status == ResponseInfo.Status.Success)
            {
                Transaction transaction = JsonConvert.DeserializeObject<Transaction>(responseInfo.message, new TransactionConverter());
                callback(transaction, responseInfo);
            }
            else
            {
                callback(null, responseInfo);
            }
        }

        public IEnumerator BCSTransfer(
            Action<string, ResponseInfo> Callback,
            Account Sender,
            AccountAddress Recipient,
            int Amount
        )
        {
            ISerializable[] transactionArguments =
            {
                Recipient,
                new U64((ulong)Amount)
            };

            EntryFunction payload = EntryFunction.Natural(
                new ModuleId(AccountAddress.FromHex("0x1"), "aptos_account"),
                "transfer",
                new TagSequence(new ISerializableTag[] { }),
                new BCS.Sequence(transactionArguments)
            );

            SignedTransaction signedTransaction = null;
            ResponseInfo responseInfo = new ResponseInfo();

            Coroutine cor_createBcsSIgnedTransaction = StartCoroutine(CreateBCSSignedTransaction((_signedTransaction) => {
                signedTransaction = _signedTransaction;
            }, Sender, new BCS.TransactionPayload(payload)));
            yield return cor_createBcsSIgnedTransaction;

            string submitBcsTxnJsonResponse = "";

            Coroutine cor_submitBcsTransaction = StartCoroutine(SubmitBCSTransaction((_responseJson, _responseInfo) => {
                submitBcsTxnJsonResponse = _responseJson;
                responseInfo = _responseInfo;
            }, signedTransaction));
            yield return cor_submitBcsTransaction;

            Callback(submitBcsTxnJsonResponse, responseInfo);

            yield return null;
        }

        /// <summary>
        /// Encodes submission.
        /// This endpoint accepts an EncodeSubmissionRequest, which internally is a UserTransactionRequestInner
        /// (and optionally secondary signers) encoded as JSON, validates the request format, and then returns that request encoded in BCS.
        /// The client can then use this to create a transaction signature to be used in a SubmitTransactionRequest, which it then passes to the /transactions POST endpoint.
        ///
        /// If you are using an SDK that has BCS support, such as the official Rust, TypeScript, or Python SDKs, you may use the BCS encoding instead of this endpoint.
        ///
        /// To sign a message using the response from this endpoint:
        /// - Decode the hex encoded string in the response to bytes.
        /// - Sign the bytes to create the signature.
        /// - Use that as the signature field in something like Ed25519Signature, which you then use to build a TransactionSignature.
        /// </summary>
        /// <param name="callback">Callback function used after response is received.</param>
        /// <param name="txnRequestJson">Transaction request in JSON format.</param>
        /// <returns>
        /// Calls <c>callback</c> function with: \n
        /// All bytes (Vec) data that is represented as hex-encoded string prefixed with 0x and fulfilled with two hex digits per byte.
        /// Unlike the Address type, HexEncodedBytes will not trim any zeros.
        /// Example: <code>0x88fbd33f54e1126269769780feb24480428179f552e2313fbe571b72e62a1ca1</code>
        /// </returns>
        public IEnumerator EncodeSubmission(Action<string> callback, string txnRequestJson)
        {
            string transactionsEncodeURL = Endpoint + "/transactions/encode_submission";
            Uri txnEncodedUri = new Uri(transactionsEncodeURL);
            var request = RequestClient.SubmitRequest(txnEncodedUri, UnityWebRequest.kHttpVerbPOST);

            byte[] jsonToSend = new UTF8Encoding().GetBytes(txnRequestJson);
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            request.SendWebRequest();
            while (!request.isDone)
            {
                yield return null;
            }

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                yield return null;
            }
            else if (request.responseCode == 404)
            {
                yield return null;
            }
            else
            {
                var responseTxt = request.downloadHandler.text;
                callback(responseTxt);
            }

            request.Dispose();
        }

        /// <summary>
        /// Encodes submission. See <see cref="EncodeSubmission(Action{string}, string)">EncodeSubmission</see>.
        /// </summary>
        /// <param name="callback">Callback function used after response is received.</param>
        /// <param name="txnRequestJson">Transaction request in JSON format.</param>
        /// <returns>Calls <c>callback</c>function with a byte array representing the encoded submission.</returns>
        public IEnumerator EncodeSubmissionAsBytes(Action<byte[]> callback, string txnRequestJson)
        {
            string transactionsEncodeURL = Endpoint + "/transactions/encode_submission";
            Uri transactionsEncodeURI = new Uri(transactionsEncodeURL);
            var request = RequestClient.SubmitRequest(transactionsEncodeURI, UnityWebRequest.kHttpVerbPOST);

            byte[] jsonToSend = new UTF8Encoding().GetBytes(txnRequestJson);
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            request.SendWebRequest();
            while (!request.isDone)
            {
                yield return null;
            }

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                yield return null;
            }
            else if (request.responseCode == 404)
            {
                yield return null;
            }
            else
            {
                byte[] response = request.downloadHandler.data;
                callback(response);
            }

            request.Dispose();
        }

        #endregion

        #region Token Transaction Wrappers

        /// <summary>
        /// Create a NFT collection
        /// </summary>
        /// <param name="callback">Callback function used after response is received.</param>
        /// <param name="sender">Creator of the collection.</param>
        /// <param name="collectionName">Name of collection.</param>
        /// <param name="collectionDescription">Description of the collection.</param>
        /// <param name="uri">Collection's URI</param>
        /// <returns>Calls <c>callback</c> function with <c>(Transaction, ResponseInfo)</c>: \n
        /// An object the represents the transaction submitted - null if the transaction to create a collection failed, and a response object that contains the response details.
        /// </returns>
        public IEnumerator CreateCollection(Action<Transaction, ResponseInfo> callback, Account sender, string collectionName, string collectionDescription, string uri)
        {
            EntryFunction payload = EntryFunction.Natural(
                new ModuleId(AccountAddress.FromHex("0x3"), "token"),
                "create_collection_script",
                new TagSequence(new ISerializableTag[] { }),
                new BCS.Sequence(
                    new ISerializable[]
                    {
                        new BString(collectionName),
                        new BString(collectionDescription),
                        new BString(uri),
                        new U64(18446744073709551615),
                        new BCS.Sequence(new[] { new Bool(false), new Bool(false), new Bool(false) })
                    }
                )
            );

            BCS.TransactionPayload txnPayload = new BCS.TransactionPayload(payload);

            SignedTransaction signedTransaction = null;
            Coroutine cor_createBcsSIgnedTransaction = StartCoroutine(CreateBCSSignedTransaction((_signedTransaction) => {
                signedTransaction = _signedTransaction;
            }, sender, new BCS.TransactionPayload(payload)));
            yield return cor_createBcsSIgnedTransaction;

            string submitBcsTxnJsonResponse = "";
            ResponseInfo responseInfo = new ResponseInfo();

            Coroutine cor_submitBcsTransaction = StartCoroutine(SubmitBCSTransaction((_responseJson, _responseInfo) => {
                submitBcsTxnJsonResponse = _responseJson;
                responseInfo = _responseInfo;
            }, signedTransaction));
            yield return cor_submitBcsTransaction;

            Transaction createCollectionTxn = JsonConvert.DeserializeObject<Transaction>(submitBcsTxnJsonResponse, new TransactionConverter());
            callback(createCollectionTxn, responseInfo);
            yield return null;
        }

        /// <summary>
        /// Create Non-Fungible Token (NFT)
        /// See token <see cref="https://github.com/aptos-labs/aptos-core/blob/main/aptos-move/framework/aptos-token/sources/token.move#L365">reference.</see>
        /// </summary>
        /// <param name="callback">Callback function used when response is received.</param>
        /// <param name="senderRoyaltyPayeeAddress">Creator of the token, also the account the that will receive royalties. </param>
        /// <param name="collectionName">Name of the collection to which the token belongs to.</param>
        /// <param name="tokenName">Name of the token</param>
        /// <param name="description">Description of the token being minted</param>
        /// <param name="supply">Token supply</param>
        /// <param name="max">Max number of mints</param>
        /// <param name="uri">URI of where the token's asset lives (e.g. JPEG)</param>
        /// <param name="royaltyPointsPerMillion">Royalties defined in the millionths</param>
        /// <returns>Calls <c>callback</c> function with <c>(Transaction, ResponseInfo)</c>: \n
        /// An object the represents the transaction submitted - null if the transaction to create a token failed, and a response object that contains the response details.
        /// </returns>
        public IEnumerator CreateToken(Action<Transaction, ResponseInfo> callback
            , Account senderRoyaltyPayeeAddress, string collectionName, string tokenName, string description, int supply, int max, string uri, int royaltyPointsPerMillion)
        {
            EntryFunction payload = EntryFunction.Natural(
                new ModuleId(AccountAddress.FromHex("0x3"), "token"),
                "create_token_script",
                new TagSequence(new ISerializableTag[] { }),
                new BCS.Sequence(
                    new ISerializable[]
                    {
                        new BString(collectionName),
                        new BString(tokenName),
                        new BString(description),
                        new U64((ulong)supply),
                        new U64((ulong) supply),
                        new BString(uri),
                        senderRoyaltyPayeeAddress.AccountAddress,
                        new U64(1000000),
                        new U64((ulong)royaltyPointsPerMillion),
                        new BCS.Sequence(new[] { new Bool(false), new Bool(false), new Bool(false), new Bool(false), new Bool(false) }),
                        new BCS.Sequence(new BString[] {}),
                        new BCS.Sequence(new Bytes[] {}),
                        new BCS.Sequence(new BString[] {})
                    }
                )
            );

            BCS.TransactionPayload txnPayload = new BCS.TransactionPayload(payload);

            SignedTransaction signedTransaction = null;
            Coroutine cor_createBcsSIgnedTransaction = StartCoroutine(CreateBCSSignedTransaction((_signedTransaction) => {
                signedTransaction = _signedTransaction;
            }, senderRoyaltyPayeeAddress, new BCS.TransactionPayload(payload)));
            yield return cor_createBcsSIgnedTransaction;

            string submitBcsTxnJsonResponse = "";
            ResponseInfo responseInfo = new ResponseInfo();

            Coroutine cor_submitBcsTransaction = StartCoroutine(SubmitBCSTransaction((_responseJson, _responseInfo) => {
                submitBcsTxnJsonResponse = _responseJson;
                responseInfo = _responseInfo;
            }, signedTransaction));
            yield return cor_submitBcsTransaction;

            Transaction createTokenTxn = JsonConvert.DeserializeObject<Transaction>(submitBcsTxnJsonResponse, new TransactionConverter());
            callback(createTokenTxn, responseInfo);
            yield return null;
        }

        /// <summary>
        /// Offer a token to a given address.
        /// </summary>
        /// <param name="callback">Callback function used when response is received.</param>
        /// <param name="account">Account offering the token.</param>
        /// <param name="receiver">Address of recipient.</param>
        /// <param name="creator">Address of token creator.</param>
        /// <param name="collectionName">Name of the collection.</param>
        /// <param name="tokenName">Name of the token.</param>
        /// <param name="amount">Amount being offered.</param>
        /// <param name="propertyVersion">ersion of the token.</param>
        /// <returns>Calls <c>callback</c> function with <c>(Transaction, ResponseInfo)</c>: \n
        /// An object the represents the transaction submitted - null if the transaction to offer a token failed, and a response object that contains the response details.
        /// </returns>
        public IEnumerator OfferToken(Action<Transaction, ResponseInfo> callback
            , Account account, Accounts.AccountAddress receiver, Accounts.AccountAddress creator
            , string collectionName, string tokenName, int amount, int propertyVersion = 0)
        {
            ResponseInfo responseInfo = new ResponseInfo();
            // Check is recipient address is valid
            if (!HdWallet.Utils.Utils.IsValidAddress(receiver.ToString()))
            {
                responseInfo.status = ResponseInfo.Status.Failed;
                responseInfo.message = "Recipient address is invalid.";
                callback(null, responseInfo);
                yield break;
            }

            // Check is creator address is valid
            if (!HdWallet.Utils.Utils.IsValidAddress(creator.ToString()))
            {
                responseInfo.status = ResponseInfo.Status.Failed;
                responseInfo.message = "Creator address is invalid.";
                callback(null, responseInfo);
                yield break;
            }

            EntryFunction payload = EntryFunction.Natural(
                new ModuleId(AccountAddress.FromHex("0x3"), "token_transfers"),
                "offer_script",
                new TagSequence(new ISerializableTag[] { }),
                new BCS.Sequence(
                    new ISerializable[]
                    {
                        receiver,
                        creator,
                        new BString(collectionName),
                        new BString(tokenName),
                        new U64((ulong)propertyVersion),
                        new U64((ulong) amount)
                    }
                )
            );

            BCS.TransactionPayload txnPayload = new BCS.TransactionPayload(payload);

            SignedTransaction signedTransaction = null;
            Coroutine cor_createBcsSIgnedTransaction = StartCoroutine(CreateBCSSignedTransaction((_signedTransaction) => {
                signedTransaction = _signedTransaction;
            }, account, new BCS.TransactionPayload(payload)));
            yield return cor_createBcsSIgnedTransaction;

            string submitBcsTxnJsonResponse = "";

            Coroutine cor_submitBcsTransaction = StartCoroutine(SubmitBCSTransaction((_responseJson, _responseInfo) => {
                submitBcsTxnJsonResponse = _responseJson;
                responseInfo = _responseInfo;
            }, signedTransaction));
            yield return cor_submitBcsTransaction;

            Transaction offerTokenTxn = JsonConvert.DeserializeObject<Transaction>(submitBcsTxnJsonResponse, new TransactionConverter());
            callback(offerTokenTxn, responseInfo);
            yield return null;
        }

        /// <summary>
        /// Claim a token that was offered by <paramref name="sender"/>
        /// </summary>
        /// <param name="callback">Callback function used when response is received.</param>
        /// <param name="account">Account making the claim</param>
        /// <param name="sender">Address of the sender of the non-fungible token (NFT)</param>
        /// <param name="creator">Address of the creator of the token (NFT)</param>
        /// <param name="collectionName">Name of the NFT collection</param>
        /// <param name="tokenName">Name of the token</param>
        /// <param name="propertyVersion">Token version, defaults to 0</param>
        /// <returns>Calls <c>callback</c>function with <c>(Transaction, ResponseInfo)</c>: \n
        /// An object the represents the transaction submitted - null if the transaction to claim a token failed, and a response object that contains the response details.
        /// </returns>
        public IEnumerator ClaimToken(Action<Transaction, ResponseInfo> callback
            , Account account, AccountAddress sender, AccountAddress creator
            , string collectionName, string tokenName, int propertyVersion = 0)
        {

            ResponseInfo responseInfo = new ResponseInfo();
            // Check is sender address is valid
            if (!HdWallet.Utils.Utils.IsValidAddress(sender.ToString()))
            {
                responseInfo.status = ResponseInfo.Status.Failed;
                responseInfo.message = "Recipient address is invalid.";
                callback(null, responseInfo);
                yield break;
            }

            // Check is creator address is valid
            if (!HdWallet.Utils.Utils.IsValidAddress(creator.ToString()))
            {
                responseInfo.status = ResponseInfo.Status.Failed;
                responseInfo.message = "Creator address is invalid.";
                callback(null, responseInfo);
                yield break;
            }

            EntryFunction payload = EntryFunction.Natural(
                new ModuleId(AccountAddress.FromHex("0x3"), "token_transfers"),
                "claim_script",
                new TagSequence(new ISerializableTag[] {}),
                new BCS.Sequence(
                    new ISerializable[]
                    {
                        sender,
                        creator,
                        new BString(collectionName),
                        new BString(tokenName),
                        new U64((ulong)propertyVersion)
                    }
                )
            );

            BCS.TransactionPayload txnPayload = new BCS.TransactionPayload(payload);

            SignedTransaction signedTransaction = null;
            Coroutine cor_createBcsSIgnedTransaction = StartCoroutine(CreateBCSSignedTransaction((_signedTransaction) => {
                signedTransaction = _signedTransaction;
            }, account, new BCS.TransactionPayload(payload)));
            yield return cor_createBcsSIgnedTransaction;

            string submitBcsTxnJsonResponse = "";

            Coroutine cor_submitBcsTransaction = StartCoroutine(SubmitBCSTransaction((_responseJson, _responseInfo) => {
                submitBcsTxnJsonResponse = _responseJson;
                responseInfo = _responseInfo;
            }, signedTransaction));
            yield return cor_submitBcsTransaction;

            Transaction claimTokenTxn = JsonConvert.DeserializeObject<Transaction>(submitBcsTxnJsonResponse, new TransactionConverter());
            callback(claimTokenTxn, responseInfo);
            yield return null;
        }
        #endregion

        #region Token Accessors

        /// <summary>
        /// Get token information.
        /// </summary>
        /// <param name="callback">Callback function used when response is received.</param>
        /// <param name="ownerAddress">Address of token owner.</param>
        /// <param name="creatorAddress">Address of token creator.</param>
        /// <param name="collectionName">Name of the collection.</param>
        /// <param name="tokenName">Name of the token.</param>
        /// <param name="propertyVersion">Version of the token.</param>
        /// <returns>Calls <c>callback</c> function with <c>(TableItemToken, ResponseInfo)</c>: \n
        /// An object the represents the transaction submitted - null if the transaction to get a token failed, and a response object that contains the response details.
        /// </returns>
        public IEnumerator GetToken(Action<TableItemToken, ResponseInfo> callback, Accounts.AccountAddress ownerAddress, Accounts.AccountAddress creatorAddress,
            string collectionName, string tokenName, int propertyVersion = 0)
        {
            bool success = false;
            long responseCode = 0;
            string tokenStoreResourceResp = "";
            Coroutine accountResourceCor = StartCoroutine(GetAccountResource((_success, _responseCode, _returnResult) =>
            {
                success = _success;
                responseCode = _responseCode;
                tokenStoreResourceResp = _returnResult;
            }, ownerAddress, "0x3::token::TokenStore"));
            yield return accountResourceCor;

            ResponseInfo responseInfo = new ResponseInfo();

            if (!success)
            {
                if (responseCode == 404)
                    responseInfo.status = ResponseInfo.Status.NotFound;
                else
                    responseInfo.status = ResponseInfo.Status.Failed;

                responseInfo.message = tokenStoreResourceResp;
                callback(null, responseInfo);
                yield break;
            }

            AccountResourceTokenStore accountResource = JsonConvert.DeserializeObject<AccountResourceTokenStore>(tokenStoreResourceResp);
            string tokenStoreHandle = accountResource.DataProp.Tokens.Handle;

            TokenIdRequest tokenId = new TokenIdRequest
            {
                TokenDataId = new TokenDataId()
                {
                    Creator = creatorAddress.ToString(),
                    Collection = collectionName,
                    Name = tokenName
                },
                PropertyVersion = propertyVersion.ToString()
            };

            string tokenIdJson = JsonConvert.SerializeObject(tokenId);

            TableItemToken tableItemToken = new TableItemToken();
            Coroutine getTableItemCor = StartCoroutine(GetTableItemNFT((_tableItemToken, _responseInfo) =>
            {
                tableItemToken = _tableItemToken;
                responseInfo = _responseInfo;
            }, tokenStoreHandle, "0x3::token::TokenId", "0x3::token::Token", tokenId));
            yield return getTableItemCor;

            callback(tableItemToken, responseInfo);
        }

        /// <summary>
        /// Get balance for a given non-fungible token (NFT).
        /// </summary>
        /// <param name="callback">Callback function used when response is received.</param>
        /// <param name="ownerAddress">Address of token owner.</param>
        /// <param name="creatorAddress">Address of token creator.</param>
        /// <param name="collectionName">Name of the collection.</param>
        /// <param name="tokenName">Name of the token.</param>
        /// <param name="propertyVersion">Version of the token.</param>
        /// <returns>Token balance as a string.</returns>
        public IEnumerator GetTokenBalance(Action<string> callback
            , Accounts.AccountAddress ownerAddress, Accounts.AccountAddress creatorAddress, string collectionName, string tokenName, int propertyVersion = 0)
        {
            TableItemToken tableItemToken = new TableItemToken();
            ResponseInfo responseInfo = new ResponseInfo();

            Coroutine accountResourceCor = StartCoroutine(GetToken((_tableItemToken, _responseInfo) =>
            {
                tableItemToken = _tableItemToken;
                responseInfo = _responseInfo;

            }, ownerAddress, creatorAddress, collectionName, tokenName, propertyVersion));
            yield return accountResourceCor;

            if (responseInfo.status == ResponseInfo.Status.Success)
            {
                string tokenBalance = tableItemToken.Amount;
                callback(tokenBalance);
            }
            else
            {
                callback("0");
            }
            yield return null;
        }

        /// <summary>
        /// Read Collection's token data table.
        /// An example
        /// <code>
        /// {
        ///     "default_properties":{
        ///         "map":{
        ///             "data":[ ]
        ///         }
        ///     },
        ///     "description":"Alice's simple token",
        ///     "largest_property_version":"0",
        ///     "maximum":"1",
        ///     "mutability_config":{
        ///         "description":false,
        ///         "maximum":false,
        ///         "properties":false,
        ///         "royalty":false,
        ///         "uri":false
        ///     },
        ///     "name":"Alice's first token",
        ///     "royalty":{
        ///         "payee_address":"0x3f99ffee67853e129173b9df0e2e9c6af6f08fe2a4417daf43df46ad957a8bbe",
        ///         "royalty_points_denominator":"1000000",
        ///         "royalty_points_numerator":"0"
        ///     },
        ///     "supply":"1",
        ///     "uri":"https://aptos.dev/img/nyan.jpeg"
        /// }
        /// </code>
        /// </summary>
        /// <param name="callback">Callback function used when response is received.</param>
        /// <param name="creator">Address of the creator.</param>
        /// <param name="collectionName">Name of the collection.</param>
        /// <param name="tokenName">Name of the token.</param>
        /// <param name="propertyVersion">Version of the token.</param>
        /// <returns>Calls <c>callback</c> function with <c>(TableItemToken, ResponseInfo)</c>: \n
        /// An object the represents the NFT's token metadata - null if the transaction to get a token failed, and a response object that contains the response details.
        /// </returns>
        public IEnumerator GetTokenData(Action<TableItemTokenMetadata, ResponseInfo> callback, Accounts.AccountAddress creator,
            string collectionName, string tokenName, int propertyVersion = 0)
        {
            bool success = false;
            long responseCode = 0;
            string collectionResourceResp = "";
            Coroutine accountResourceCor = StartCoroutine(GetAccountResource((_success, _responseCode, returnResult) =>
            {
                success = _success;
                responseCode = _responseCode;
                collectionResourceResp = returnResult;
            }, creator, "0x3::token::Collections"));

            yield return accountResourceCor;

            ResponseInfo responseInfo = new ResponseInfo();

            if (!success)
            {
                responseInfo.status = ResponseInfo.Status.Failed;
                responseInfo.message = collectionResourceResp;
                callback(null, responseInfo);
                yield break;
            }

            ResourceCollection resourceCollection = JsonConvert.DeserializeObject<ResourceCollection>(collectionResourceResp);
            string tokenDataHandle = resourceCollection.DataProp.TokenData.Handle;

            TokenDataId tokenDataId = new TokenDataId
            {
                Creator = creator.ToString(),
                Collection = collectionName,
                Name = tokenName
            };

            string tokenDataIdJson = JsonConvert.SerializeObject(tokenDataId);

            TableItemTokenMetadata tableItemTokenMetadata = new TableItemTokenMetadata();

            Coroutine getTableItemCor = StartCoroutine(
                GetTableItemTokenData((_tableItemTokenMetadata, _responseInfo) => {
                    tableItemTokenMetadata = _tableItemTokenMetadata;
                    responseInfo = _responseInfo;
                }
                    , tokenDataHandle
                    , "0x3::token::TokenDataId"
                    , "0x3::token::TokenData"
                    , tokenDataId)
                );

            yield return getTableItemCor;
            callback(tableItemTokenMetadata, responseInfo);
        }

        /// <summary>
        /// Get collection information.
        /// This return a JSON representation that will be parsed by the developer that know the specific return types.
        /// </summary>
        /// <param name="callback">Callback function used when response is received.</param>
        /// <param name="creator">Address of the creator.</param>
        /// <param name="collectionName">Name of the collection.</param>
        /// <returns>A JSON string representation of the collection information - null if the request fails.</returns>
        public IEnumerator GetCollection(Action<string> callback, Accounts.AccountAddress creator,
            string collectionName)
        {
            bool success = false;
            long responseCode = 0;
            string collectionResourceResp = "";
            Coroutine getAccountResourceCor = StartCoroutine(GetAccountResource((_success, _responseCode, returnResult) =>
            {
                success = _success;
                responseCode = _responseCode;
                collectionResourceResp = returnResult;
            }, creator, "0x3::token::Collections"));
            yield return getAccountResourceCor;

            if (!success)
            {
                callback("Account resource not found");
                yield break;
            }

            ResourceCollection resourceCollection = JsonConvert.DeserializeObject<ResourceCollection>(collectionResourceResp);
            string tokenDataHandle = resourceCollection.DataProp.CollectionData.Handle;

            string tableItemResp = "";
            Coroutine getTableItemCor = StartCoroutine(
                GetTableItem(
                    (returnResult) => {
                        tableItemResp = returnResult;
                    }
                    , tokenDataHandle
                    , "0x1::string::String"
                    , "0x3::token::CollectionData"
                    , collectionName)
                );
            yield return getTableItemCor;
            callback(tableItemResp);
        }

        public IEnumerator TransferObject(Action<string, ResponseInfo> Callback, Account Owner, AccountAddress Object, AccountAddress To)
        {
            ISerializable[] transactionArguments =
            {
                Object,
                To
            };

            EntryFunction payload = EntryFunction.Natural(
                new ModuleId(AccountAddress.FromHex("0x1"), "object"),
                Constants.APTOS_TRANSFER_CALL,
                new TagSequence(new ISerializableTag[] { }),
                new BCS.Sequence(transactionArguments)
            );

            SignedTransaction signedTransaction = null;
            ResponseInfo responseInfo = new ResponseInfo();

            Coroutine cor_createBcsSIgnedTransaction = StartCoroutine(CreateBCSSignedTransaction((_signedTransaction) => {
                signedTransaction = _signedTransaction;
            }, Owner, new BCS.TransactionPayload(payload)));
            yield return cor_createBcsSIgnedTransaction;

            string submitBcsTxnJsonResponse = "";

            Coroutine cor_submitBcsTransaction = StartCoroutine(SubmitBCSTransaction((_responseJson, _responseInfo) => {
                submitBcsTxnJsonResponse = _responseJson;
                responseInfo = _responseInfo;
            }, signedTransaction));
            yield return cor_submitBcsTransaction;

            Callback(submitBcsTxnJsonResponse, responseInfo);

            yield return null;
        }

        /// <summary>
        /// Get a resource of a given type from an account.
        /// NOTE: The response is a complex object of types only known to the developer writing the contracts.
        /// This function return a string and expect the developer to deserialize it into an object.
        /// See <see cref="GetAccountResourceCollection(Action{ResourceCollection, ResponseInfo}, AccountAddress, string)">GetAccountResourceCollection</see> for an example.
        /// </summary>
        /// <param name="callback">Callback function used when response is received.</param>
        /// <param name="accountAddress">Address of the account.</param>
        /// <param name="resourceType">Type of resource being queried for.</param>
        /// <returns>Calls <c>callback</c> function with <c>(bool, long, string)</c>: \n
        /// -- bool: success boolean \n
        /// -- long: - error code, string - JSON response to be deserialized by the consumer of the function\n
        /// -- string: - the response which may contain the resource details</returns>
        public IEnumerator GetAccountResource(Action<bool, long, string> callback, Accounts.AccountAddress accountAddress, string resourceType)
        {
            string accountsURL = Endpoint + "/accounts/" + accountAddress.ToString() + "/resource/" + resourceType;
            Uri accountsURI = new Uri(accountsURL);
            var request = RequestClient.SubmitRequest(accountsURI);
            
            request.SendWebRequest();
            while (!request.isDone)
            {
                yield return null;
            }

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                callback(false, 0, "ERROR: Connection Error: " + request.error);
            }
            else if (request.responseCode == 404)
            {
                callback(false, request.responseCode, request.error);
            }
            else if (request.responseCode >= 400)
            {
                callback(false, request.responseCode, request.error);
            }
            else
            {
                callback(true, request.responseCode, request.downloadHandler.text);
            }

            request.Dispose();
        }
        #endregion

        #region Utilities
        /// <summary>
        /// Utility Function coverts byte array to hex
        /// </summary>
        /// <param name="seed"></param>
        /// <returns></returns>
        private string ToHex(byte[] seed)
        {
            var hex = BitConverter
                .ToString(seed)
                .Replace("-", "")
                .ToLower();

            return hex;
        }

        /// <summary>
        /// Convert byte array to string.
        /// </summary>
        /// <param name="hex">Hexadecimal string</param>
        /// <returns>Byte array representing the hex string.</returns>
        public byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        /// <summary>
        /// Turns byte array to hexadecimal string.
        /// </summary>
        /// <param name="bytes">Byte array</param>
        /// <returns>String that represents byte array of hexadecials.</returns>
        public string ToHexadecimalRepresentation(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder(bytes.Length << 1);
            foreach (byte b in bytes)
            {
                sb.AppendFormat("{0:X2}", b);
            }
            return sb.ToString();
        }
        #endregion
    }
}
