using Aptos.Accounts;
using Aptos.Rest;
using Aptos.Rpc.Model;
using Aptos.Unity.Rest.Model;
using Aptos.Utilities.BCS;
using Chaos.NaCl;
using NBitcoin;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Transaction = Aptos.Unity.Rest.Model.Transaction;

namespace Aptos.Unity.Rest
{
    /// <summary>
    /// The Aptos REST Client contains a set of [standalone] Coroutines
    /// that can started within any Unity script.
    /// 
    /// Consideration must be placed into the wait time required 
    /// for a transaction to be committed into the blockchain.
    /// </summary>
    public class RestClient : MonoBehaviour
    {
        public static RestClient Instance { get; set; }

        public static int TransactionWaitInSeconds = 20;

        public Uri Endpoint { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        #region Setup
        /// <summary>
        /// Set Endpoint for RPC / REST call
        /// </summary>
        /// <param name="url"></param>
        public void SetEndPoint(string url)
        {
            Endpoint = new Uri(url);
        }
        #endregion

        #region Account Accessors

        /// <summary>
        /// Gets Account Details
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="accountAddress"></param>
        /// <returns></returns>
        public IEnumerator GetAccount(Action<string> callback, string accountAddress)
        {
            string accountsURL = Endpoint + "/accounts/" + accountAddress;
            Uri accountsURI = new Uri(accountsURL);
            UnityWebRequest request = UnityWebRequest.Get(accountsURI);
            request.SendWebRequest();
            while (!request.isDone)
            {
                yield return null;
            }

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                callback(request.error);
            }
            else
            {
                callback(request.downloadHandler.text);
            }

            request.Dispose();
        }

        /// <summary>
        /// Gets Account Sequence Number
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="accountAddress"></param>
        /// <returns></returns>
        public IEnumerator GetAccountSequenceNumber(Action<string> callback, string accountAddress)
        {
            string accountDataResp = "";

            Coroutine cor = StartCoroutine(GetAccount((_accountDataResp) => {
                accountDataResp = _accountDataResp;
            }, accountAddress));
            yield return cor;

            AccountData accountData = JsonConvert.DeserializeObject<AccountData>(accountDataResp);
            string sequenceNumber = accountData.SequenceNumber;

            callback(sequenceNumber);
        }

        /// <summary>
        /// Get Account Balance
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="accountAddress"></param>
        /// <returns></returns>
        public IEnumerator GetAccountBalance(Action<bool, string> callback, Accounts.AccountAddress accountAddress)
        {
            string accountsURL = Endpoint + "/accounts/" + accountAddress.ToString() + "/resource/" + Constants.APTOS_COIN_TYPE;
            Uri accountsURI = new Uri(accountsURL);
            UnityWebRequest request = UnityWebRequest.Get(accountsURI);
            request.SendWebRequest();
            while (!request.isDone)
            {
                yield return null;
            }

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError("Error While Sending: " + request.error);
                callback(false, "Connection error.");
            }
            else if (request.responseCode == 404)
            {
                callback(false, "Resource not found.");
            }
            else
            {
                callback(true, request.downloadHandler.text);
            }

            request.Dispose();
        }

        /// <summary>
        /// Get Account Resource
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="accountAddress"></param>
        /// <param name="resourceType"></param>
        /// <returns></returns>
        public IEnumerator GetAccountResourceCollection(Action<ResourceCollection> callback, Accounts.AccountAddress accountAddress, string resourceType)
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
                Debug.LogError("Error While Sending: " + request.error);
                callback(null);
            }
            if (request.responseCode >= 404)
            {
                Debug.LogError("Account Resource Not Found: " + request.error);
                callback(null);
            }
            else
            {
                ResourceCollection acctResource = JsonConvert.DeserializeObject<ResourceCollection>(request.downloadHandler.text);
                callback(acctResource);
            }

            request.Dispose();
            yield return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="accountAddress"></param>
        /// <param name="resourceType"></param>
        /// <returns></returns>
        public IEnumerator GetTableItemCoin(Action<AccountResourceCoin> callback, string handle, string keyType, string valueType, string key)
        {
            TableItemRequest tableItemRequest = new TableItemRequest
            {
                KeyType = keyType,
                ValueType = valueType,
                Key = key
            };

            string getTableItemURL = Endpoint + "/tables/" + handle + "/item/";
            Uri getTableItemURI = new Uri(getTableItemURL);

            UnityWebRequest request = UnityWebRequest.Get(getTableItemURI);
            request.SendWebRequest();
            while (!request.isDone)
            {
                yield return null;
            }

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError("Error While Sending: " + request.error);
                callback(null);
            }
            if (request.responseCode == 404)
            {
                Debug.LogError("Table Item Not Found: " + request.error);
                callback(null);
            }
            else
            {
                //AccountResourceCollection acctResource = JsonConvert.DeserializeObject<AccountResourceCollection>(request.downloadHandler.text);
                //callback(acctResource);
            }

            request.Dispose();
            yield return null;
        }

        /// <summary>
        /// Get a standard table item at a specific ledger vevrsion from the table identified
        /// by the handle {table_handle} in the path adn the "key" (TableItemRequest)
        /// provided by the request body.
        /// 
        /// https://fullnode.devnet.aptoslabs.com/v1/spec#/operations/get_table_item
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="handle"></param> The identifier for the given table
        /// <param name="keyType"></param> String representation of an on-chain Move tag that is exposed in the transaction
        /// <param name="valueType"></param> String representation of an on-chain Move type value
        /// <param name="key"></param>The value of the table item's key
        /// <returns></returns> Callback withthe response
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
            Debug.Log("GET TABLE ITEM URI: " + getTableItemURI);

            var request = new UnityWebRequest(getTableItemURI, "POST");
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
                Debug.LogError("Error While Sending: " + request.error);
                callback(null);
            }
            if (request.responseCode == 404)
            {
                Debug.LogError("Table Item Not Found: " + request.error);
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

        /// Get a table item of a NFT
        /// <inheritdoc cref="RestClient.GetTableItem(Action{string}, string, string, string, string)"/>
        /// <param name="key"></param> A TokeIdRequest object that contains the token / collection info
        public IEnumerator GetTableItemNFT(Action<string> callback, string handle, string keyType, string valueType, TokenIdRequest key)
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

            var request = new UnityWebRequest(getTableItemURI, "POST");
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
                Debug.LogError("Error While Sending: " + request.error);
                callback(null);
            }
            if (request.responseCode == 404)
            {
                Debug.LogWarning("Table Item Not Found: " + request.error);

                Debug.Log("CREATOR: " + key.TokenDataId.Creator + " COLLECTION: " + key.TokenDataId.Collection + " NAME: " + key.TokenDataId.Name);

                TableItemToken tableItemToken = new TableItemToken();
                tableItemToken.Id = new Aptos.Unity.Rest.Model.Id();
                tableItemToken.Id.TokenDataId = new Aptos.Unity.Rest.Model.TokenDataId();
                tableItemToken.Id.TokenDataId.Creator = key.TokenDataId.Creator;
                tableItemToken.Id.TokenDataId.Collection = key.TokenDataId.Collection;
                tableItemToken.Id.TokenDataId.Name = key.TokenDataId.Name;
                tableItemToken.Amount = "0";

                string tableItemTokenJson = JsonConvert.SerializeObject(tableItemToken);
                callback(tableItemTokenJson);
            }
            else
            {
                string response = request.downloadHandler.text;
                callback(response);
            }

            request.Dispose();
            yield return null;
        }

        /// Get a table item of a token data
        /// <inheritdoc cref="RestClient.GetTableItem(Action{string}, string, string, string, string)"/>
        /// <param name="key"></param> a TokenDataId object that contains the token / collection info
        public IEnumerator GetTableItemTokenData(Action<string> callback, string handle, string keyType, string valueType, TokenDataId key)
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

            var request = new UnityWebRequest(getTableItemURI, "POST");
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
                Debug.LogError("Error While Sending: " + request.error);
                callback(null);
            }
            if (request.responseCode == 404)
            {
                Debug.LogError("Table Item Not Found: " + request.error);
                TableItemToken tableItemToken = new TableItemToken
                {
                    Id = new Aptos.Unity.Rest.Model.Id
                    {
                        TokenDataId =
                        {
                            Creator = key.Creator,
                            Collection = key.Collection,
                            Name = key.Name
                        }
                    },
                    Amount = "0"
                };

                string tableItemTokenJson = JsonConvert.SerializeObject(tableItemToken);
                callback(tableItemTokenJson);
            }
            else
            {
                string response = request.downloadHandler.text;
                callback(response);
            }

            request.Dispose();
            yield return null;
        }
        #endregion

        #region Ledger Accessors
        /// <summary>
        /// Get the latest ledger information, including data such as chain ID, role type, ledger versions, epoch, etc.
        /// </summary>
        /// <param name="callback">Callback function</param>
        /// <returns>(boolean, response) true if repsonse is successful, false otherwise.
        /// </returns>
        public IEnumerator GetInfo(Action<bool, string> callback)
        {
            UnityWebRequest request = UnityWebRequest.Get(Endpoint);
            request.SendWebRequest();
            while (!request.isDone)
            {
                yield return null;
            }

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                callback(false, request.error);
            }
            else if(request.responseCode >= 404)
            {
                callback(false, request.error);
            }
            else
            {
                callback(true, request.downloadHandler.text);
            }

            request.Dispose();
        }
        #endregion

        #region Transactions

        /// <summary>
        /// 1) Generates a transaction request
        /// 2) submits that to produce a raw transaction
        /// 3) signs the raw transaction
        /// 4) submits the signed transaction
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="sender"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        public IEnumerator SubmitTransaction(Action<bool, string> callback, Account sender, TransactionPayload payload)
        {
            ///////////////////////////////////////////////////////////////////////
            // 1) Generate a transaction request
            ///////////////////////////////////////////////////////////////////////
            string sequenceNumber = "";

            Coroutine cor_sequenceNumber = StartCoroutine(GetAccountSequenceNumber((_sequenceNumber) => {
                sequenceNumber = _sequenceNumber;
            }, sender.AccountAddress.ToString()));
            yield return cor_sequenceNumber;

            var expirationTimestamp = (DateTime.Now.ToUnixTimestamp() + Constants.EXPIRATION_TTL).ToString();

            var txnRequest = new TransactionRequest()
            {
                Sender = sender.AccountAddress.ToString(),
                SequenceNumber = sequenceNumber,
                MaxGasAmount = Constants.MAX_GAS_AMOUNT.ToString(),
                GasUnitPrice = Constants.GAS_UNIT_PRICE.ToString(),
                ExpirationTimestampSecs = expirationTimestamp,
                Payload = payload
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

            byte[] toSign = StringToByteArrayTwo(encodedSubmission.Trim('"')[2..]);

            ///////////////////////////////////////////////////////////////////////
            // 3) Signs the raw transaction
            ///////////////////////////////////////////////////////////////////////
            byte[] signature = sender.Sign(toSign);

            txnRequest.Signature = new SignatureData()
            {
                Type = Constants.ED25519_SIGNATURE,
                PublicKey = "0x" + CryptoBytes.ToHexStringLower(sender.PublicKey),
                Signature = "0x" + CryptoBytes.ToHexStringLower(signature)
            };

            txnRequestJson = JsonConvert.SerializeObject(txnRequest, new TransactionRequestConverter());

            string transactionURL = Endpoint + "/transactions";
            Uri transactionsURI = new Uri(transactionURL);
            var request = new UnityWebRequest(transactionsURI, "POST");
            byte[] jsonToSend = new UTF8Encoding().GetBytes(txnRequestJson);
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            //TODO: Fix Error Responses
            request.SendWebRequest();
            while (!request.isDone)
            {
                yield return null;
            }

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError("Error While Submitting Transaction: " + request.error);
                callback(false, request.error);
            }
            else if (request.responseCode >= 404)
            {
                Debug.LogWarning("Transaction Response: " + request.responseCode);
                callback(false, "Error: " + request.responseCode);
            }
            else // Either 200, or 202
            {
                Debug.Log("RESPONSE CODE: " + request.responseCode + " TEXT: " + request.downloadHandler.text);
                string response = request.downloadHandler.text;
                callback(true, response);
            }

            request.Dispose();
        }

        /// <summary>
        /// Waits for Transaction query to return whether transaction has been confirmed in the blockchain.
        /// Times out otherwise.
        /// 
        /// Queries for a given transaction hash (txnHash) using <see cref="TransactionPending"/>
        /// by polling / looping until we find a "Success" transaction response 
        /// , or until it times out after <see cref="TransactionWaitInSeconds"/>.
        /// 
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="txnHash"></param>
        /// <returns>(bool, callback) 
        /// -- true if the transaction hash was found after polling 
        /// -- false if we did not find the transaction hash and timed out
        /// </returns>
        public IEnumerator WaitForTransaction(Action<bool, string> callback, string txnHash)
        {
            bool transactionPending = true;
            int count = 0;
            while (transactionPending)
            {
                Coroutine transactionPendingCor = StartCoroutine(TransactionPending((pending, response) => {
                    transactionPending = pending;
                    // If transaction is NOT pending
                    if (!transactionPending)
                    {
                        Transaction transaction = JsonConvert.DeserializeObject<Transaction>(response, new TransactionConverter());
                        if(transaction.GetType().GetProperty("Success") != null && transaction.Success) {
                            callback(true, response);
                        }
                    }
                    count += 1;

                }, txnHash));

                yield return new WaitForSeconds(2f);

                if (count > TransactionWaitInSeconds)
                {
                    // Transaction hash wasn't found after n types
                    callback(false, "Response Timed Out After Querying " + count + "Times");
                    break;
                }
            }
        }

        /// <summary>
        /// Query to see if transaction has been 'confirmed' in the blockchain by using the transaction hash.
        /// A 404 error will be returned if the transaction hasn't been confirmed.
        /// Once the transaction is confirmed it will have a `pending_transaction` state.
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="txnHash"></param>
        /// <returns>(bool, string)
        /// -- true if transaction is still pending / hasn't been found, meaning 404, error in response, or `pending_transaction` is true
        /// -- false if transaction has been found, meaning `pending_transaction` is true
        /// </returns>
        public IEnumerator TransactionPending(Action<bool, string> callback, string txnHash)
        {
            string accountsURL = Endpoint + "/transactions/by_hash/" + txnHash;
            Uri accountsURI = new Uri(accountsURL);
            UnityWebRequest request = UnityWebRequest.Get(accountsURI);
            request.SendWebRequest();
            while (!request.isDone)
            {
                yield return null;
            }

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                callback(true, "Connection Error");
                request.Dispose();
                yield return new WaitForSeconds(1f);
            }
            else if (request.responseCode == 404)
            {
                callback(true, "Transaction Not Found: " + request.responseCode);
                request.Dispose();
                yield return new WaitForSeconds(1f);
            }
            else if (request.responseCode >= 400)
            {
                callback(true, "Transaction Call Error: " + request.responseCode + " : " + request.downloadHandler.text);
                request.Dispose();
                yield return new WaitForSeconds(1f);
            }
            else
            {
                var transactionResult = JsonConvert.DeserializeObject<Transaction>(request.downloadHandler.text, new TransactionConverter())!;
                bool isPending = transactionResult.Type.Equals("pending_transaction");

                if (isPending)
                {
                    Debug.LogWarning("Transaction is Pending: " + request.downloadHandler.text);
                }

                callback(isPending, request.downloadHandler.text);

                request.Dispose();
                yield return new WaitForSeconds(1f);
            }
        }

        #endregion

        #region Transaction Wrappers
        /// <summary>
        /// Transfer a given coin amount from a given Account to the recipient's account Address.
        /// Returns the sequence number of the transaction used to transfer.
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="sender"></param>
        /// <param name="to"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public IEnumerator Transfer(Action<string> callback, Account sender, string to, long amount)
        {
            var transferPayload = new TransactionPayload()
            {
                Type = Constants.ENTRY_FUNCTION_PAYLOAD,
                Function = Constants.COIN_TRANSFER_FUNCTION,
                TypeArguments = new string[] { Constants.APTOS_ASSET_TYPE },
                Arguments = new Arguments()
                {
                    ArgumentStrings = new string[] { to, amount.ToString() }
                }
            };

            bool success = false;
            string response = "";
            Coroutine cor_response = StartCoroutine(SubmitTransaction((_success, _response) => {
                success = _success;
                response = _response;
            }, sender, transferPayload));
            yield return cor_response;

            callback(response);
        }

        /// <summary>
        /// TODO: Complete
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="sender"></param>
        /// <param name="recipient"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public IEnumerator BCSTransfer(Action<string> callback, Account sender, string recipient, int amount)
        {
            Utilities.BCS.AccountAddress recipientArg = new Utilities.BCS.AccountAddress(recipient);
            U64 amountArg = new U64((ulong)amount);

            Utilities.BCS.AccountAddress accountAddress = new Utilities.BCS.AccountAddress("0x01");
            ModuleId moduleId = new ModuleId(accountAddress, "coin");

            ISerializable[] transactionArguments =
            {
                recipientArg,
                amountArg
            };

            StructTag structTag = new StructTag(accountAddress, "aptos_coin", "AptosCoin", new ISerializableTag[0]);
            ISerializableTag[] typeTags = new ISerializableTag[] { structTag };

            EntryFunction entryFunction = new EntryFunction(
                moduleId, 
                "transfer",
                new TagSequence(typeTags), 
                new Utilities.BCS.Sequence(transactionArguments)
            );

            TransactionPayload txnPayload = new TransactionPayload();

            yield return null;
        }

        /// <summary>
        /// Calls encode submission
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="txnRequestJson"></param>
        /// <returns></returns>
        public IEnumerator EncodeSubmission(Action<string> callback, string txnRequestJson)
        {
            string transactionsEncodeURL = Endpoint + "/transactions/encode_submission";
            Uri accountsURI = new Uri(transactionsEncodeURL);

            var request = new UnityWebRequest(accountsURI, "POST");
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
                Debug.Log("Error While Sending: " + request.error);
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

        public IEnumerator EncodeSubmissionAsBytes(Action<byte[]> callback, string txnRequestJson)
        {
            string transactionsEncodeURL = Endpoint + "/transactions/encode_submission";
            Uri transactionsEncodeURI = new Uri(transactionsEncodeURL);

            var request = new UnityWebRequest(transactionsEncodeURI, "POST");
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
                Debug.Log("Error While Sending: " + request.error);
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
        /// <param name="callback"></param>
        /// <param name="sender"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="uri"></param>
        /// <returns></returns>
        public IEnumerator CreateCollection(Action<string> callback, Account sender, string collectionName, string collectionDescription, string uri)
        {
            // STEP 1: Create Transaction Arguments
            Arguments arguments = new Arguments()
            {
                ArgumentStrings = new string[] { collectionName, collectionDescription, uri, "18446744073709551615" },
                MutateSettings = new bool[] { false, false, false }
            };

            // STEP 2: Create Payload Containing Transaction Arguments
            // TYPE: entry_function_payload
            // TYPE: script_function_payload
            // FUNCTION: create_collection_script
            TransactionPayload createCollectionPayload = new TransactionPayload()
            {
                Type = Constants.ENTRY_FUNCTION_PAYLOAD,
                Function = Constants.CREATE_COLLECTION_SCRIPT, // Contract Address
                TypeArguments = new string[] { },
                Arguments = arguments
            };

            string payloadJson = JsonConvert.SerializeObject(createCollectionPayload, new TransactionPayloadConverter());

            ///////////////////////////////////////////////////////////////////////
            // 1) Generate a transaction request
            ///////////////////////////////////////////////////////////////////////
            string sequenceNumber = "";

            Coroutine cor_sequenceNumber = StartCoroutine(GetAccountSequenceNumber((_sequenceNumber) => {
                sequenceNumber = _sequenceNumber;
            }, sender.AccountAddress.ToString()));
            yield return cor_sequenceNumber;

            var expirationTimestamp = (DateTime.Now.ToUnixTimestamp() + Constants.EXPIRATION_TTL).ToString();

            var txnRequest = new TransactionRequest()
            {
                Sender = sender.AccountAddress.ToString(),
                SequenceNumber = sequenceNumber,
                MaxGasAmount = Constants.MAX_GAS_AMOUNT.ToString(),
                GasUnitPrice = Constants.GAS_UNIT_PRICE.ToString(),
                ExpirationTimestampSecs = expirationTimestamp,
                Payload = createCollectionPayload
            };

            string txnRequestJson = JsonConvert.SerializeObject(txnRequest, new TransactionRequestConverter());

            ///////////////////////////////////////////////////////////////////////
            // 2) Submits raw transaction to get encoded submission
            ///////////////////////////////////////////////////////////////////////
            string encodedSubmission = "";

            Coroutine cor_encodedSubmission = StartCoroutine(EncodeSubmission((_encodedSubmission) => {
                encodedSubmission = _encodedSubmission;
            }, txnRequestJson));
            yield return cor_encodedSubmission;

            ///////////////////////////////////////////////////////////////////////
            // STEP 3: Sign Ttransaction
            ///////////////////////////////////////////////////////////////////////
            byte[] toSign = StringToByteArrayTwo(encodedSubmission.Trim('"')[2..]);
            byte[] signature = sender.Sign(toSign);

            txnRequest.Signature = new SignatureData()
            {
                Type = "ed25519_signature",
                PublicKey = "0x" + CryptoBytes.ToHexStringLower(sender.PublicKey), // Works ..
                Signature = "0x" + CryptoBytes.ToHexStringLower(signature) // Works ..
            };

            ///////////////////////////////////////////////////////////////////////
            // STEP 4: Submit Transaction
            ///////////////////////////////////////////////////////////////////////
            txnRequestJson = JsonConvert.SerializeObject(txnRequest, new TransactionRequestConverter());
            txnRequestJson = txnRequestJson.Trim();

            string transactionURL = Endpoint + "/transactions";
            Uri transactionsURI = new Uri(transactionURL);
            var request = new UnityWebRequest(transactionsURI, "POST");
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(txnRequestJson);
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            request.SendWebRequest();
            while (!request.isDone)
            {
                yield return null;
            }

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError("Error While Submitting Transaction: " + request.error);
                //return request.error;
                callback(request.error);
            }
            else if (request.responseCode == 404)
            {
                callback("??????????????");
            }
            //else if (request.responseCode == 400)
            //{
            //    // VM Error
            //}
            else
            {
                Debug.Log("RESPONSE CODE: " + request.responseCode);
                string response = request.downloadHandler.text;
                callback(response);
            }

            request.Dispose();
            yield return null;
        }

        /// <summary>
        /// Create Non-Fungible Token (NFT)
        /// https://github.com/aptos-labs/aptos-core/blob/main/aptos-move/framework/aptos-token/sources/token.move#L365
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="sender"></param>
        /// <param name="collectionName"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="supply"></param>
        /// <param name="uri"></param>
        /// <param name="royaltyPointsPerMillion"></param>
        /// <returns></returns>
        public IEnumerator CreateToken(Action<string> callback
            , Account senderRoyaltyPayeeAddress, string collectionName, string name, string description, int supply, int max, string uri, int royaltyPointsPerMillion)
        {
            Arguments arguments = new Arguments()
            {
                ArgumentStrings = new string[] {
                    collectionName, name, description, supply.ToString(),
                    max.ToString(), uri, senderRoyaltyPayeeAddress.AccountAddress.ToString(),
                    "1000000", royaltyPointsPerMillion.ToString()
                },
                MutateSettings = new bool[] { false, false, false, false, false },
                PropertyKeys = new string[] { },
                PropertyValues = new int[] { },
                PropertyTypes = new string[] { },
            };

            TransactionPayload txnPayload = new TransactionPayload()
            {
                Type = Constants.ENTRY_FUNCTION_PAYLOAD,
                Function = Constants.CREATE_TOKEN_SCRIPT_FUNCTION,
                TypeArguments = new string[] { },
                Arguments = arguments
            };

            string payloadJson = JsonConvert.SerializeObject(txnPayload, new TransactionPayloadConverter());

            string sequenceNumber = "";

            Coroutine cor_sequenceNumber = StartCoroutine(GetAccountSequenceNumber((_sequenceNumber) => {
                sequenceNumber = _sequenceNumber;
            }, senderRoyaltyPayeeAddress.AccountAddress.ToString()));
            yield return cor_sequenceNumber;

            var expirationTimestamp = (DateTime.Now.ToUnixTimestamp() + Constants.EXPIRATION_TTL).ToString();

            TransactionRequest txnRequest = new TransactionRequest()
            {
                Sender = senderRoyaltyPayeeAddress.AccountAddress.ToString(),
                SequenceNumber = sequenceNumber,
                MaxGasAmount = Constants.MAX_GAS_AMOUNT.ToString(),
                GasUnitPrice = Constants.GAS_UNIT_PRICE.ToString(),
                ExpirationTimestampSecs = expirationTimestamp,
                Payload = txnPayload
            };

            string txnRequestJson = JsonConvert.SerializeObject(txnRequest, new TransactionRequestConverter());

            ///////////////////////////////////////////////////////////////////////
            // 2) Submits that to produce a raw transaction
            ///////////////////////////////////////////////////////////////////////

            string encodedSubmission = "";

            Coroutine cor_encodedSubmission = StartCoroutine(EncodeSubmission((_encodedSubmission) => {
                encodedSubmission = _encodedSubmission;
            }, txnRequestJson));
            yield return cor_encodedSubmission;

            byte[] toSign = StringToByteArrayTwo(encodedSubmission.Trim('"')[2..]);
            byte[] signature = senderRoyaltyPayeeAddress.Sign(toSign);

            txnRequest.Signature = new SignatureData()
            {
                Type = Constants.ED25519_SIGNATURE,
                PublicKey = "0x" + CryptoBytes.ToHexStringLower(senderRoyaltyPayeeAddress.PublicKey),
                Signature = "0x" + CryptoBytes.ToHexStringLower(signature)
            };

            string signedTxnRequestJson = JsonConvert.SerializeObject(txnRequest, new TransactionRequestConverter());
            txnRequestJson = txnRequestJson.Trim();

            Debug.Log("TXN REQUEST JSON: " + txnRequestJson);

            string transactionURL = Endpoint + "/transactions";
            Uri transactionsURI = new Uri(transactionURL);
            var request = new UnityWebRequest(transactionsURI, "POST");
            byte[] jsonToSend = new UTF8Encoding().GetBytes(signedTxnRequestJson);
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
                Debug.LogError("Error While Submitting Transaction: " + request.error);
                callback(request.error);
            }
            else if (request.responseCode == 404)
            {
                callback("ERROR 404: " + request.downloadHandler.text);
            }
            else if (request.responseCode == 400)
            {
                callback("ERROR 400: " + request.downloadHandler.text);
            }
            else
            {
                Debug.Log("CREATE NFT TOKEN RESPONSE CODE: " + request.responseCode);
                string response = request.downloadHandler.text;
                callback(response);
            }

            request.Dispose();
            yield return null;
        }

        public IEnumerator OfferToken(Action<string> callback
            , Account account, Accounts.AccountAddress receiver, Accounts.AccountAddress creator
            , string collectionName, string tokenName, string amount, string propertyVersion = "0")
        {
            Arguments arguments = new Arguments()
            {
                ArgumentStrings = new string[] {
                      receiver.ToHexString()
                    , creator.ToHexString()
                    , collectionName
                    , tokenName
                    , propertyVersion
                    , amount

                }
            };

            TransactionPayload txnPayload = new TransactionPayload()
            {
                Type = Constants.ENTRY_FUNCTION_PAYLOAD,
                Function = Constants.TOKEN_TRANSFER_OFFER_SCRIPT,
                TypeArguments = new string[] { },
                Arguments = arguments
            };

            string payloadJson = JsonConvert.SerializeObject(txnPayload, new TransactionPayloadConverter());

            string sequenceNumber = "";

            Coroutine cor_sequenceNumber = StartCoroutine(GetAccountSequenceNumber((_sequenceNumber) => {
                sequenceNumber = _sequenceNumber;
            }, account.AccountAddress.ToString()));
            yield return cor_sequenceNumber;

            var expirationTimestamp = (DateTime.Now.ToUnixTimestamp() + Constants.EXPIRATION_TTL).ToString();

            TransactionRequest txnRequest = new TransactionRequest()
            {
                Sender = account.AccountAddress.ToString(),
                SequenceNumber = sequenceNumber,
                MaxGasAmount = Constants.MAX_GAS_AMOUNT.ToString(),
                GasUnitPrice = Constants.GAS_UNIT_PRICE.ToString(),
                ExpirationTimestampSecs = expirationTimestamp,
                Payload = txnPayload
            };

            string txnRequestJson = JsonConvert.SerializeObject(txnRequest, new TransactionRequestConverter());

            ///////////////////////////////////////////////////////////////////////
            // 2) Submits that to produce a raw transaction
            ///////////////////////////////////////////////////////////////////////
            string encodedSubmission = "";

            Coroutine cor_encodedSubmission = StartCoroutine(EncodeSubmission((_encodedSubmission) => {
                encodedSubmission = _encodedSubmission;
            }, txnRequestJson));
            yield return cor_encodedSubmission;

            byte[] toSign = StringToByteArrayTwo(encodedSubmission.Trim('"')[2..]);
            byte[] signature = account.Sign(toSign);

            txnRequest.Signature = new SignatureData()
            {
                Type = Constants.ED25519_SIGNATURE,
                PublicKey = "0x" + CryptoBytes.ToHexStringLower(account.PublicKey),
                Signature = "0x" + CryptoBytes.ToHexStringLower(signature)
            };

            string signedTxnRequestJson = JsonConvert.SerializeObject(txnRequest, new TransactionRequestConverter());
            txnRequestJson = txnRequestJson.Trim();

            string transactionURL = Endpoint + "/transactions";
            Uri transactionsURI = new Uri(transactionURL);
            var request = new UnityWebRequest(transactionsURI, "POST");
            byte[] jsonToSend = new UTF8Encoding().GetBytes(signedTxnRequestJson);
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
                Debug.LogError("Error While Submitting Transaction: " + request.error);
                callback(request.error);
            }
            else if (request.responseCode == 404)
            {
                callback("ERROR 404: " + request.downloadHandler.text);
            }
            else if (request.responseCode == 400)
            {
                callback("ERROR 400: " + request.downloadHandler.text);
            }
            else
            {
                string response = request.downloadHandler.text;
                callback(response);
            }

            request.Dispose();
            yield return null;
        }

        public IEnumerator ClaimToken(Action<string> callback
            , Account account, Accounts.AccountAddress sender, Accounts.AccountAddress creator
            , string collectionName, string tokenName, string propertyVersion = "0")
        {
            Arguments arguments = new Arguments()
            {
                ArgumentStrings = new string[] {
                      sender.ToHexString()
                    , creator.ToHexString()
                    , collectionName
                    , tokenName
                    , propertyVersion
                },
            };

            TransactionPayload txnPayload = new TransactionPayload()
            {
                Type = Constants.ENTRY_FUNCTION_PAYLOAD,
                Function = Constants.TOKEN_TRANSFER_CLAIM_SCRIPT,
                TypeArguments = new string[] { },
                Arguments = arguments
            };

            string payloadJson = JsonConvert.SerializeObject(txnPayload, new TransactionPayloadConverter());

            string sequenceNumber = "";

            Coroutine cor_sequenceNumber = StartCoroutine(GetAccountSequenceNumber((_sequenceNumber) => {
                sequenceNumber = _sequenceNumber;
            }, account.AccountAddress.ToString()));
            yield return cor_sequenceNumber;

            var expirationTimestamp = (DateTime.Now.ToUnixTimestamp() + Constants.EXPIRATION_TTL).ToString();

            TransactionRequest txnRequest = new TransactionRequest()
            {
                Sender = account.AccountAddress.ToString(),
                SequenceNumber = sequenceNumber,
                MaxGasAmount = Constants.MAX_GAS_AMOUNT.ToString(),
                GasUnitPrice = Constants.GAS_UNIT_PRICE.ToString(),
                ExpirationTimestampSecs = expirationTimestamp,
                Payload = txnPayload
            };

            string txnRequestJson = JsonConvert.SerializeObject(txnRequest, new TransactionRequestConverter());

            ///////////////////////////////////////////////////////////////////////
            // 2) Submits that to produce a raw transaction
            ///////////////////////////////////////////////////////////////////////
            string encodedSubmission = "";

            Coroutine cor_encodedSubmission = StartCoroutine(EncodeSubmission((_encodedSubmission) => {
                encodedSubmission = _encodedSubmission;
            }, txnRequestJson));
            yield return cor_encodedSubmission;

            byte[] toSign = StringToByteArrayTwo(encodedSubmission.Trim('"')[2..]);
            byte[] signature = account.Sign(toSign);

            txnRequest.Signature = new SignatureData()
            {
                Type = Constants.ED25519_SIGNATURE,
                PublicKey = "0x" + CryptoBytes.ToHexStringLower(account.PublicKey),
                Signature = "0x" + CryptoBytes.ToHexStringLower(signature)
            };

            string signedTxnRequestJson = JsonConvert.SerializeObject(txnRequest, new TransactionRequestConverter());
            txnRequestJson = txnRequestJson.Trim();

            string transactionURL = Endpoint + "/transactions";
            Uri transactionsURI = new Uri(transactionURL);
            var request = new UnityWebRequest(transactionsURI, "POST");
            byte[] jsonToSend = new UTF8Encoding().GetBytes(signedTxnRequestJson);
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
                Debug.LogError("Error While Submitting Transaction: " + request.error);
                callback(request.error);
            }
            else if (request.responseCode == 404)
            {
                callback("ERROR 404: " + request.downloadHandler.text);
            }
            else if (request.responseCode == 400)
            {
                callback("ERROR 400: " + request.downloadHandler.text);
            }
            else
            {
                string response = request.downloadHandler.text;
                callback(response);
            }

            request.Dispose();
            yield return null;
        }

        public IEnumerator DirectTransferToken(Action<string> callback
            , Account sender, Account receiver, Accounts.AccountAddress receive, Accounts.AccountAddress creatorsAddress
            , string collectionName, string tokenName, string amount, string propertyVersion = "0")
        {
            Arguments arguments = new Arguments()
            {
                ArgumentStrings = new string[] {
                      creatorsAddress.ToHexString()
                    , collectionName
                    , tokenName
                    , propertyVersion
                    , amount
                }
            };

            TransactionPayload txnPayload = new TransactionPayload()
            {
                Type = Constants.ENTRY_FUNCTION_PAYLOAD,
                Function = Constants.DIRECT_TRANSFER_SCRIPT,
                TypeArguments = new string[] { },
                Arguments = arguments
            };

            string payloadJson = JsonConvert.SerializeObject(txnPayload, new TransactionPayloadConverter());

            string sequenceNumber = "";

            Coroutine cor_sequenceNumber = StartCoroutine(GetAccountSequenceNumber((_sequenceNumber) => {
                sequenceNumber = _sequenceNumber;
            }, sender.AccountAddress.ToString()));
            yield return cor_sequenceNumber;

            var expirationTimestamp = (DateTime.Now.ToUnixTimestamp() + Constants.EXPIRATION_TTL).ToString();

            TransactionRequest txnRequest = new TransactionRequest()
            {
                Sender = sender.AccountAddress.ToString(),
                SequenceNumber = sequenceNumber,
                MaxGasAmount = Constants.MAX_GAS_AMOUNT.ToString(),
                GasUnitPrice = Constants.GAS_UNIT_PRICE.ToString(),
                ExpirationTimestampSecs = expirationTimestamp,
                Payload = txnPayload
            };

            string txnRequestJson = JsonConvert.SerializeObject(txnRequest, new TransactionRequestConverter());

            ///////////////////////////////////////////////////////////////////////
            // 2) Submits that to produce a raw transaction
            ///////////////////////////////////////////////////////////////////////
            string encodedSubmission = "";

            Coroutine cor_encodedSubmission = StartCoroutine(EncodeSubmission((_encodedSubmission) => {
                encodedSubmission = _encodedSubmission;
            }, txnRequestJson));
            yield return cor_encodedSubmission;

            byte[] toSign = StringToByteArrayTwo(encodedSubmission.Trim('"')[2..]);
            byte[] signature = sender.Sign(toSign);

            txnRequest.Signature = new SignatureData()
            {
                Type = Constants.ED25519_SIGNATURE,
                PublicKey = "0x" + CryptoBytes.ToHexStringLower(sender.PublicKey),
                Signature = "0x" + CryptoBytes.ToHexStringLower(signature)
            };

            string signedTxnRequestJson = JsonConvert.SerializeObject(txnRequest, new TransactionRequestConverter());
            txnRequestJson = txnRequestJson.Trim();

            string transactionURL = Endpoint + "/transactions";
            Uri transactionsURI = new Uri(transactionURL);
            var request = new UnityWebRequest(transactionsURI, "POST");
            byte[] jsonToSend = new UTF8Encoding().GetBytes(signedTxnRequestJson);
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
                Debug.LogError("Error While Submitting Transaction: " + request.error);
                callback(request.error);
            }
            else if (request.responseCode == 404)
            {
                callback("ERROR 404: " + request.downloadHandler.text);
            }
            else if (request.responseCode == 400)
            {
                callback("ERROR 400: " + request.downloadHandler.text);

            }
            else
            {
                string response = request.downloadHandler.text;
                callback(response);
            }

            request.Dispose();
            yield return null;
        }

        #endregion

        #region Token Accessors
        public IEnumerator GetToken(Action<string> callback, Accounts.AccountAddress ownerAddress, Accounts.AccountAddress creatorAddress,
            string collectionName, string tokenName, string propertyVersion = "0")
        {
            string tokenStoreResourceResp = "";
            Coroutine accountResourceCor = StartCoroutine(GetAccountResource((returnResult) =>
            {
                tokenStoreResourceResp = returnResult;
            }, ownerAddress, "0x3::token::TokenStore"));
            yield return accountResourceCor;

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
                PropertyVersion = propertyVersion
            };

            string tokenIdJson = JsonConvert.SerializeObject(tokenId);

            string tableItemResp = "";
            Coroutine getTableItemCor = StartCoroutine(GetTableItemNFT((returnResult) =>
            {
                tableItemResp = returnResult;
            }, tokenStoreHandle, "0x3::token::TokenId", "0x3::token::Token", tokenId));
            yield return getTableItemCor;

            callback(tableItemResp);
        }
        public IEnumerator GetTokenBalance(Action<string> callback
            , Accounts.AccountAddress ownerAddress, Accounts.AccountAddress creatorAddress, string collectionName, string tokenName, string propertyVersion = "0")
        {
            string tokenResp = "";
            Coroutine accountResourceCor = StartCoroutine(GetToken((returnResult) =>
            {
                tokenResp = returnResult;
            }, ownerAddress, creatorAddress, collectionName, tokenName, propertyVersion));
            yield return accountResourceCor;

            TableItemToken tableItemToken = JsonConvert.DeserializeObject<TableItemToken>(tokenResp);
            string tokenBalance = tableItemToken.Amount;

            callback(tokenBalance);
            yield return null;
        }

        /// <summary>
        /// Read Collection's token data table 
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="creator"></param>
        /// <param name="collectionName"></param>
        /// <param name="tokenName"></param>
        /// <param name="propertyVersion"></param>
        /// <returns></returns>
        public IEnumerator GetTokenData(Action<string> callback, Accounts.AccountAddress creator,
            string collectionName, string tokenName, string propertyVersion = "0")
        {
            string collectionResourceResp = "";
            Coroutine accountResourceCor = StartCoroutine(GetAccountResource((returnResult) =>
            {
                collectionResourceResp = returnResult;
            }, creator, "0x3::token::Collections"));

            yield return accountResourceCor;

            Debug.Log("GetTokenData Collection: " + collectionResourceResp);

            ResourceCollection resourceCollection = JsonConvert.DeserializeObject<ResourceCollection>(collectionResourceResp);
            string tokenDataHandle = resourceCollection.DataProp.TokenData.Handle;

            Debug.Log("TOKEN DATA HANDLE: " + tokenDataHandle);
            TokenDataId tokenDataId = new TokenDataId
            {
                Creator = creator.ToHexString(),
                Collection = collectionName,
                Name = tokenName
            };

            string tokenDataIdJson = JsonConvert.SerializeObject(tokenDataId);

            string tableItemResp = "";
            Coroutine getTableItemCor = StartCoroutine(
                GetTableItemTokenData(
                    (returnResult) => { tableItemResp = returnResult;}
                    , tokenDataHandle
                    , "0x3::token::TokenDataId"
                    , "0x3::token::TokenData"
                    , tokenDataId)
                );

            yield return getTableItemCor;
            callback(tableItemResp);
        }

        public IEnumerator GetCollection(Action<string> callback, Accounts.AccountAddress creator,
            string collectionName, string propertyVersion = "0")
        {
            string collectionResourceResp = "";
            Coroutine getAccountResourceCor = StartCoroutine(GetAccountResource((returnResult) =>
            {
                collectionResourceResp = returnResult;
            }, creator, "0x3::token::Collections"));
            yield return getAccountResourceCor;

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

        public IEnumerator GetAccountResource(Action<string> callback, Accounts.AccountAddress accountAddress, string resourceType)
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
                Debug.LogError("Error While Sending: " + request.error);
                callback("ERROR: Connection Error: " + request.error);
            }
            else if (request.responseCode == 404)
            {
                Debug.LogError("Error Not Found: " + request.error);
                callback("ERROR: Resource Not Found: " + request.error);
            }
            else
            {
                callback(request.downloadHandler.text);
            }

            request.Dispose();
        }
        #endregion

        #region Package Publishing
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
        /// Turns a hexadecimal string to a byte array
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        /// <summary>
        /// Second version of StringToByteArray
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public byte[] StringToByteArrayTwo(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        /// <summary>
        /// Untested function to turn hexadecimal string to byte array
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
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