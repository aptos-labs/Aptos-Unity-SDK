using System.Collections;
using System.Collections.Generic;
using Aptos.Accounts;
using Aptos.Unity.Rest;
using Aptos.Unity.Rest.Model;
using Aptos.Unity.Rest.Model.Resources;
using Newtonsoft.Json;
using UnityEngine;

namespace Aptos.Unity.Sample
{
    public class AptosToken : MonoBehaviour
    {
        void Start()
        {
            StartCoroutine(RunAptosClientExample());
        }

        IEnumerator RunAptosClientExample()
        {
            #region REST & Faucet Client Setup
            Debug.Log("<color=cyan>=== =========================== ===</color>");
            Debug.Log("<color=cyan>=== Set Up Faucet & REST Client ===</color>");
            Debug.Log("<color=cyan>=== =========================== ===</color>");
            string faucetEndpoint = "https://faucet.devnet.aptoslabs.com";

            FaucetClient faucetClient = FaucetClient.Instance;
            RestClient restClient = RestClient.Instance.SetEndPoint(Constants.DEVNET_BASE_URL);
            Coroutine restClientSetupCor = StartCoroutine(RestClient.Instance.SetUp());
            yield return restClientSetupCor;

            AptosTokenClient tokenClient = AptosTokenClient.Instance.SetUp(restClient);
            #endregion

            #region Create Accounts
            Account alice = Account.Generate();
            Account bob = Account.Generate();
            Debug.Log("<color=cyan>=== ========= ===</color>");
            Debug.Log("<color=cyan>=== Addresses ===</color>");
            Debug.Log("<color=cyan>=== ========= ===</color>");
            Debug.Log("Alice: " + alice.AccountAddress.ToString());
            Debug.Log("Bob: " + bob.AccountAddress.ToString());
            #endregion

            #region Fund Alice Account Through Devnet Faucet
            AccountAddress aliceAddress = alice.AccountAddress;

            bool success = false;
            ResponseInfo responseInfo = new ResponseInfo();
            Coroutine fundAliceAccountCor = StartCoroutine(faucetClient.FundAccount((_success, _responseInfo) =>
            {
                success = _success;
                responseInfo = _responseInfo;
            }, aliceAddress.ToString(), 100000000, faucetEndpoint));
            yield return fundAliceAccountCor;

            if (responseInfo.status != ResponseInfo.Status.Success)
            {
                Debug.LogError("Faucet funding for Alice failed: " + responseInfo.message);
                yield break;
            }
            #endregion

            #region Fund Bob Account Through Devnet Faucet
            AccountAddress bobAddress = bob.AccountAddress;

            success = false;
            responseInfo = new ResponseInfo();
            Coroutine fundBobAccountCor = StartCoroutine(FaucetClient.Instance.FundAccount((_success, _responseInfo) =>
            {
                success = _success;
                responseInfo = _responseInfo;
            }, bobAddress.ToString(), 100000000, faucetEndpoint));
            yield return fundBobAccountCor;

            if (responseInfo.status != ResponseInfo.Status.Success)
            {
                Debug.LogError("Faucet funding for Bob failed: " + responseInfo.message);
                yield break;
            }
            #endregion

            #region Initial Coin Balances
            Debug.Log("<color=cyan>=== ===================== ===</color>");
            Debug.Log("<color=cyan>=== Initial Coin Balances ===</color>");
            Debug.Log("<color=cyan>=== ===================== ===</color>");
            AccountResourceCoin.Coin coin = new AccountResourceCoin.Coin();
            Coroutine getAliceBalanceCor1 = StartCoroutine(RestClient.Instance.GetAccountBalance((_coin, _responseInfo) =>
            {
                coin = _coin;
                responseInfo = _responseInfo;
            }, aliceAddress));
            yield return getAliceBalanceCor1;

            if (responseInfo.status == ResponseInfo.Status.Failed)
            {
                Debug.LogError(responseInfo.message);
                yield break;
            }

            Debug.Log("Alice's Balance After Funding: " + coin.Value);

            Coroutine getBobAccountBalance = StartCoroutine(RestClient.Instance.GetAccountBalance((_coin, _responseInfo) =>
            {
                coin = _coin;
                responseInfo = _responseInfo;
            }, bobAddress));
            yield return getBobAccountBalance;

            if (responseInfo.status == ResponseInfo.Status.Failed)
            {
                Debug.LogError(responseInfo.message);
                yield break;
            }

            Debug.Log("Bob's Balance After Funding: " + coin.Value);
            #endregion

            string collectionName = "Alice's";
            string tokenName = "Alice's first token";

            #region Create Collection
            Debug.Log("<color=cyan>=== Creating Collection ===</color>");

            if (tokenClient == null)
            {
                Debug.Log("TOKEN CLIENT IS NULL");
            }

            string createCollectionTxn = "";
            Coroutine createCollectionCor = StartCoroutine(tokenClient.CreateCollection((_createCollectionTxn, _responseInfo) => {
                createCollectionTxn = _createCollectionTxn;
                responseInfo = _responseInfo;
            },
                alice,
                "Alice's simple collection",
                1,
                collectionName,
                "https://aptos.dev",
                true,
                true,
                true,
                true,
                true,
                true,
                true,
                true,
                true,
                0,
                1
            ));

            yield return createCollectionCor;

            if (responseInfo.status != ResponseInfo.Status.Success)
            {
                Debug.LogError("Cannot create collection. " + responseInfo.message);
            }

            Debug.Log("Create Collection Response: " + responseInfo.message);
            Debug.Log("Transaction Details: " + createCollectionTxn);

            CreateCollectionResponse txnResponse = JsonConvert.DeserializeObject<CreateCollectionResponse>(createCollectionTxn);
            string transactionHash = txnResponse.Hash;
            Debug.Log("Transaction Hash: " + transactionHash);
            #endregion

            #region Wait For Transaction
            bool waitForTxnSuccess = false;
            Coroutine waitForTransactionCor = StartCoroutine(
                RestClient.Instance.WaitForTransaction((_pending, _responseInfo) =>
                {
                    waitForTxnSuccess = _pending;
                    responseInfo = _responseInfo;
                }, transactionHash)
            );
            yield return waitForTransactionCor;

            if (!waitForTxnSuccess)
            {
                Debug.LogWarning("Transaction was not found. Breaking out of example: Error: " + responseInfo.message);
                yield break;
            }
            #endregion

            #region Get Account Resource
            Debug.Log("<color=cyan>=== Get Account Resource for Alice ===</color>");
            success = false;
            long responseCode = 0;
            string accountResourceResp = "";
            Coroutine accountResourceCor = StartCoroutine(restClient.GetAccountResource((_success, _responseCode, _returnResult) =>
            {
                success = _success;
                responseCode = _responseCode;
                accountResourceResp = _returnResult;
            }, alice.AccountAddress, "0x1::account::Account"));
            yield return accountResourceCor;

             responseInfo = new ResponseInfo();

            if (!success)
            {
                if (responseCode == 404)
                    responseInfo.status = ResponseInfo.Status.NotFound;
                else
                    responseInfo.status = ResponseInfo.Status.Failed;

                responseInfo.message = accountResourceResp;
                yield break;
            }

            AccountResource accountResource = JsonConvert.DeserializeObject<AccountResource>(accountResourceResp);
            int creationNum = int.Parse(accountResource.Data.GuidCreationNum);
            Debug.Log("Creation Num: " + creationNum);
            #endregion

            #region Mint Token
            Debug.Log("<color=cyan>=== ========== ===</color>");
            Debug.Log("<color=cyan>=== Mint Token ===</color>");
            Debug.Log("<color=cyan>=== ========== ===</color>");

            string mintTokenTxn = "";
            Coroutine mintTokenCor = StartCoroutine(tokenClient.MintToken((_mintTokenTxn, _responseInfo) =>
            {
                mintTokenTxn = _mintTokenTxn;
                responseInfo = _responseInfo;
            },
                alice,
                collectionName,
                "Alice's simple token",
                tokenName,
                "https://aptos.dev/img/nyan.jpeg",
                new PropertyMap(new List<Property> { Property.StringProp("string", "string value") })
            ));

            yield return mintTokenCor;

            if (responseInfo.status != ResponseInfo.Status.Success)
            {
                Debug.LogError("Cannot mint token. " + responseInfo.message);
            }

            Debug.Log("Mint Token Response: " + responseInfo.message);
            Debug.Log("Transaction Details: " + mintTokenTxn);

            CreateTokenResponse createTxnResponse = JsonConvert.DeserializeObject<CreateTokenResponse>(mintTokenTxn);
            string createTokenTxnHash = createTxnResponse.Hash;

            Debug.Log("Transaction Hash: " + createTokenTxnHash);
            #endregion

            #region Wait for Transaction
            waitForTxnSuccess = false;
            waitForTransactionCor = StartCoroutine(
                RestClient.Instance.WaitForTransaction((_pending, _responseInfo) =>
                {
                    waitForTxnSuccess = _pending;
                    responseInfo = _responseInfo;
                }, createTokenTxnHash)
            );
            yield return waitForTransactionCor;

            if (!waitForTxnSuccess)
            {
                Debug.LogWarning("Transaction was not found. Breaking out of example: Error: " + responseInfo.message);
                yield break;
            }
            #endregion

            #region Collection Address and Token Address
            AccountAddress collectionAddress = AccountAddress.ForNamedCollection(
                alice.AccountAddress, collectionName
            );

            List<AccountAddress> mintedTokens = new List<AccountAddress>();

            Coroutine mintedTokensCor = StartCoroutine(tokenClient.TokensMintedFromTransaction((_tokenArray, _responseInfo) => {
                mintedTokens = _tokenArray;
                responseInfo = _responseInfo;
            }, createTokenTxnHash));

            yield return mintedTokensCor;

            AccountAddress tokenAddress = mintedTokens[0];

            Debug.Log("AccountAddress ForNamedCollection: " + collectionAddress.ToString());
            Debug.Log("AccountAddress ForGuidObject: " + tokenAddress.ToString());
            #endregion

            #region Token Client read_object Collection
            Debug.Log("<color=cyan>=== ====================== ===</color>");
            Debug.Log("<color=cyan>=== Read Collection Object ===</color>");
            Debug.Log("<color=cyan>=== =================== ===</color>");
            ReadObject readObjectCollection = new ReadObject(null);
            Coroutine readObjectCollectionCor = StartCoroutine(tokenClient.ReadObject((_readObjectCollection, _responseInfo) =>
            {
                readObjectCollection = _readObjectCollection;
                responseInfo = _responseInfo;
            },
            collectionAddress
            ));

            yield return readObjectCollectionCor;

            Debug.Log("Alice's collection: " + readObjectCollection);
            #endregion

            #region Token client read_object Token Address
            Debug.Log("<color=cyan>=== Read Token Object ===</color>");
            ReadObject readObjectToken = new ReadObject(null);
            Coroutine readObjectTokenCor = StartCoroutine(tokenClient.ReadObject((_readObjectToken, _responseInfo) =>
            {
                readObjectToken = _readObjectToken;
                responseInfo = _responseInfo;
            }, tokenAddress ));

            yield return readObjectTokenCor;

            if(responseInfo.status == ResponseInfo.Status.NotFound)
            {
                Debug.LogError("ERROR: " + responseInfo.message);
                yield break;
            }

            Debug.Log("Alice's token: " + readObjectToken);
            #endregion

            #region Add token property
            Debug.Log(
                "<color=cyan>=== ================== ===</color>\n" +
                "<color=cyan>=== Add Token Property ===</color>\n" +
                "<color=cyan>=== ================== ===</color>"
            );
            string responseString = "";

            // Add token property
            Coroutine addTokenPropertyCor = StartCoroutine(tokenClient.AddTokenProperty((_responseString, _responseInfo) =>
            {
                responseString = _responseString;
                responseInfo = _responseInfo;
            }, alice, tokenAddress, Property.BoolProp("test", false)));

            yield return addTokenPropertyCor;

            AddTokenPropertyResponse addTokenPropertyResponse = JsonConvert.DeserializeObject<AddTokenPropertyResponse>(responseString);
            transactionHash = addTokenPropertyResponse.Hash;

            // Wait for transaction
            waitForTxnSuccess = false;
            waitForTransactionCor = StartCoroutine(
                RestClient.Instance.WaitForTransaction((_pending, _responseInfo) =>
                {
                    waitForTxnSuccess = _pending;
                    responseInfo = _responseInfo;
                }, transactionHash)
            );
            yield return waitForTransactionCor;

            if (!waitForTxnSuccess)
            {
                Debug.LogWarning("Transaction was not found. Breaking out of example: Error: " + responseInfo.message);
                yield break;
            }

            // Read token object after adding property
            readObjectToken = new ReadObject(null);
            readObjectTokenCor = StartCoroutine(tokenClient.ReadObject((_readObjectToken, _responseInfo) =>
            {
                readObjectToken = _readObjectToken;
                responseInfo = _responseInfo;
            }, tokenAddress));

            yield return readObjectTokenCor;

            Debug.Log("Alice's token: " + readObjectToken);
            #endregion

            #region Remove token property
            Debug.Log("<color=cyan>=== =================== ===</color>");
            Debug.Log("<color=cyan>=== Remove Property ===</color>");
            Debug.Log("<color=cyan>=== =================== ===</color>");
            responseString = "";
            Coroutine removeTokenPropertyCor = StartCoroutine(tokenClient.RemoveTokenProperty((_responseString, _responseInfo) =>
            {
                responseString = _responseString;
                responseInfo = _responseInfo;
            }, alice, tokenAddress, "string"));

            yield return removeTokenPropertyCor;

            // Wait for transaction
            waitForTxnSuccess = false;
            waitForTransactionCor = StartCoroutine(
                RestClient.Instance.WaitForTransaction((_pending, _responseInfo) =>
                {
                    waitForTxnSuccess = _pending;
                    responseInfo = _responseInfo;
                }, transactionHash)
            );
            yield return waitForTransactionCor;

            if (!waitForTxnSuccess)
            {
                Debug.LogWarning("Transaction was not found. Breaking out of example: Error: " + responseInfo.message);
                yield break;
            }

            // Read token object after removing property
            readObjectToken = new ReadObject(null);
            readObjectTokenCor = StartCoroutine(tokenClient.ReadObject((_readObjectToken, _responseInfo) =>
            {
                readObjectToken = _readObjectToken;
                responseInfo = _responseInfo;
            }, tokenAddress));

            yield return readObjectTokenCor;

            Debug.Log("Alice's token: " + readObjectToken);
            #endregion

            #region Update Token Property
            Debug.Log("<color=cyan>=== ================== ===</color>");
            Debug.Log("<color=cyan>=== Update Token Property ===</color>");
            Debug.Log("<color=cyan>=== ================== ===</color>");

            responseString = "";
            responseInfo = new ResponseInfo();

            // Add token property
            Coroutine updateTokenPropertyCor = StartCoroutine(tokenClient.UpdateTokenProperty((_responseString, _responseInfo) =>
            {
                responseString = _responseString;
                responseInfo = _responseInfo;
            }, alice, tokenAddress, Property.BoolProp("test", true)));

            yield return updateTokenPropertyCor;

            UpdateTokenResponse UpdateTokenPropertyResponse = JsonConvert.DeserializeObject<UpdateTokenResponse>(responseString);
            transactionHash = addTokenPropertyResponse.Hash;

            // Wait for transaction
            waitForTxnSuccess = false;
            waitForTransactionCor = StartCoroutine(
                RestClient.Instance.WaitForTransaction((_pending, _responseInfo) =>
                {
                    waitForTxnSuccess = _pending;
                    responseInfo = _responseInfo;
                }, transactionHash)
            );
            yield return waitForTransactionCor;

            if (!waitForTxnSuccess)
            {
                Debug.LogWarning("Transaction was not found. Breaking out of example: Error: " + responseInfo.message);
                yield break;
            }

            // Read token object after updating property
            readObjectToken = new ReadObject(null);
            readObjectTokenCor = StartCoroutine(tokenClient.ReadObject((_readObjectToken, _responseInfo) =>
            {
                readObjectToken = _readObjectToken;
                responseInfo = _responseInfo;
            }, tokenAddress));

            yield return readObjectTokenCor;

            Debug.Log("Alice's token: " + readObjectToken);
            #endregion

            #region Add token property -- binary data
            Debug.Log(
                "<color=cyan>=== ==================================== ===</color>\n" +
                "<color=cyan>=== Add Token Property - Binary Sequence ===</color>\n" +
                "<color=cyan>=== ==================================== ===</color>\n"
            );
            responseString = "";

            // Add token property
            addTokenPropertyCor = StartCoroutine(tokenClient.AddTokenProperty((_responseString, _responseInfo) =>
            {
                responseString = _responseString;
                responseInfo = _responseInfo;
            }, alice, tokenAddress, Property.BytesProp("bytes", new byte[] { 0x00, 0x01 })));

            yield return addTokenPropertyCor;

            addTokenPropertyResponse = JsonConvert.DeserializeObject<AddTokenPropertyResponse>(responseString);
            transactionHash = addTokenPropertyResponse.Hash;

            // Wait for transaction
            waitForTxnSuccess = false;
            waitForTransactionCor = StartCoroutine(
                RestClient.Instance.WaitForTransaction((_pending, _responseInfo) =>
                {
                    waitForTxnSuccess = _pending;
                    responseInfo = _responseInfo;
                }, transactionHash)
            );
            yield return waitForTransactionCor;

            if (!waitForTxnSuccess)
            {
                Debug.LogWarning("Transaction was not found. Breaking out of example: Error: " + responseInfo.message);
                yield break;
            }

            // Read token object after adding property
            readObjectToken = new ReadObject(null);
            readObjectTokenCor = StartCoroutine(tokenClient.ReadObject((_readObjectToken, _responseInfo) =>
            {
                readObjectToken = _readObjectToken;
                responseInfo = _responseInfo;
            }, tokenAddress));

            yield return readObjectTokenCor;

            Debug.Log("Alice's token: " + readObjectToken);
            #endregion

            #region Transferring Tokens
            Debug.Log(
                "<color=cyan>=== ======================================== ===</color>\n" +
                "<color=cyan>=== Transferring the Token from Alice to Bob ===</color>\n" +
                "<color=cyan>=== ======================================== ===</color>\n"
            );
            Debug.Log("Alice: " + alice.AccountAddress.ToString());
            Debug.Log("Bob: " + bob.AccountAddress.ToString());

            Coroutine transferTokenCor = StartCoroutine(restClient.TransferObject((_responseString, _responseInfo) => {
                responseString = _responseString;
                responseInfo = _responseInfo;
            }, alice, tokenAddress, bob.AccountAddress));

            yield return transferTokenCor;

            Debug.Log("Response: " + responseString);

            TransferObjectResponse transferObjectResponse = JsonConvert.DeserializeObject<TransferObjectResponse>(responseString);
            transactionHash = transferObjectResponse.Hash;

            // Wait for transaction
            waitForTxnSuccess = false;
            waitForTransactionCor = StartCoroutine(
                RestClient.Instance.WaitForTransaction((_pending, _responseInfo) =>
                {
                    waitForTxnSuccess = _pending;
                    responseInfo = _responseInfo;
                }, transactionHash)
            );

            yield return waitForTransactionCor;

            if (!waitForTxnSuccess)
            {
                Debug.LogWarning("Transaction was not found. Breaking out of example: Error: " + responseInfo.message);
                yield break;
            }

            ReadObject transferObjectReadObject = new ReadObject(null);
            readObjectCollectionCor = StartCoroutine(tokenClient.ReadObject((_readObjectCollection, _responseInfo) =>
            {
                transferObjectReadObject = _readObjectCollection;
                responseInfo = _responseInfo;
            },
            tokenAddress
            ));

            yield return readObjectCollectionCor;

            Debug.Log("Alice's transferred token: " + transferObjectReadObject);
            #endregion

            yield return null;
        }
    }
}